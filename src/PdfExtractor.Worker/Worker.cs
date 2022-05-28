using System.IO.Compression;
using ImageMagick;
using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;
using StackExchange.Redis;

namespace PdfExtractor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<Configuracao> _configuracao;
    private readonly IFilaPdf _filaExtracao;
    private readonly IStorage _storage;

    public Worker(ILogger<Worker> logger, IOptions<Configuracao> configuracao, IFilaPdf filaExtracao, IStorage storage)
    {
        _logger = logger;
        _configuracao = configuracao;
        _filaExtracao = filaExtracao;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuracao.Value.ConexaoRedis);
        ISubscriber subscriber = redis.GetSubscriber();
        ChannelMessageQueue canal = await subscriber.SubscribeAsync("extracao");
        canal.OnMessage(ExtrairImagensPdf);
        _logger.LogInformation("Inscreveu");
        await canal.Completion;
        await canal.UnsubscribeAsync();
        _logger.LogInformation("Desinscreveu");
    }

    private async Task ExtrairImagensPdf(ChannelMessage mensagem)
    {
        PdfExtractorDTO tarefa = await _filaExtracao.ObterProximoDaFilaAsync()
            .ConfigureAwait(false);
        string nomeArquivo = tarefa.Id.ToString("N");
        _logger.LogInformation($"Processando {nomeArquivo}");
        string caminhoPdf = Path.Combine("pdfs", nomeArquivo);
        string caminhoZip = Path.Combine("zips", nomeArquivo);
        try
        {
            using Stream pdfStream = _storage.ObterStream(caminhoPdf, FileShare.None);
            using Stream fileStream = _storage.CriarArquivo(caminhoZip);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: false);
            var settings = new MagickReadSettings();
            settings.Density = new Density(300, 300);
            using var images = new MagickImageCollection();
            images.Read(pdfStream, settings);
            int page = 1;
            foreach (var image in images)
            {
                var zipEntry = zip.CreateEntry($"{page:000000}.png");
                using Stream entryStream = zipEntry.Open();
                image.Format = MagickFormat.Png;
                image.Write(entryStream);
                page++;
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, e.Message);
            _storage.ApagarArquivo(caminhoZip);
        }
    }
}
