using System.IO.Compression;
using ImageMagick;
using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;

namespace PdfExtractor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<Configuracao> _configuracao;
    private readonly IFilaPdf _filaExtracao;
    private readonly IStorage _storage;
    private readonly IExtracaoPdfRepository _extracaoPdfRepository;

    public Worker(ILogger<Worker> logger, IOptions<Configuracao> configuracao, IFilaPdf filaExtracao, IStorage storage, IExtracaoPdfRepository extracaoPdfRepository)
    {
        _logger = logger;
        _configuracao = configuracao;
        _filaExtracao = filaExtracao;
        _storage = storage;
        _extracaoPdfRepository = extracaoPdfRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                PdfExtractorDTO? tarefa = await _filaExtracao.ObterProximoDaFilaAsync()
                    .ConfigureAwait(false);
                if (tarefa is not null)
                {
                    await ExtrairImagensPdf(tarefa)
                        .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogInformation("Nenhuma tarefa na fila");
                    await Task.Delay(_configuracao.Value.TempoIntervaloProcessamento)
                        .ConfigureAwait(false);
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, e.Message);
                await Task.Delay(_configuracao.Value.TempoIntervaloProcessamento)
                    .ConfigureAwait(false);
            }

        }
    }

    private async Task ExtrairImagensPdf(PdfExtractorDTO tarefa)
    {
        string nomeArquivo = tarefa.Id.ToString("N");
        _logger.LogInformation($"Processando {nomeArquivo}");
        string caminhoPdf = Path.Combine("pdfs", nomeArquivo);
        string caminhoZip = Path.Combine("zips", nomeArquivo);
        try
        {
            await _extracaoPdfRepository.AtualizarStatus(tarefa.Id, "processando")
                .ConfigureAwait(false);
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
                image.Alpha(AlphaOption.Remove);
                image.Format = MagickFormat.Png;
                image.Write(entryStream);
                page++;
            }
            await _extracaoPdfRepository.AtualizarStatus(tarefa.Id, "finalizado")
                .ConfigureAwait(false);
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, e.Message);
            _storage.ApagarArquivo(caminhoZip);
            await _extracaoPdfRepository.AtualizarStatus(tarefa.Id, "erro")
                .ConfigureAwait(false);
        }
    }
}
