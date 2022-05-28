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
    public Worker(ILogger<Worker> logger, IOptions<Configuracao> configuracao, IFilaPdf filaExtracao)
    {
        _logger = logger;
        _configuracao = configuracao;
        _filaExtracao = filaExtracao;
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
        string storage = _configuracao.Value.CaminhoStorage ?? "/storage";
        string caminhoPdf = Path.Combine(storage, "pdfs", nomeArquivo);
        string caminhoZip = Path.Combine(storage, "zips", nomeArquivo);
        try
        {
            Directory.CreateDirectory(Path.Combine(storage, "zips"));
            using Stream pdfStream = File.Open(caminhoPdf, FileMode.Open, FileAccess.Read, FileShare.None);
            using Stream fileStream = File.Create(caminhoZip);
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
            DeleteIfExists(caminhoZip);
        }
    }

    private void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
