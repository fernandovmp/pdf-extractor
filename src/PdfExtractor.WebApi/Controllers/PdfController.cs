using Microsoft.AspNetCore.Mvc;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.WebApi.Model;

namespace PdfExtractor.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PdfController : ControllerBase
{
    private readonly ILogger<PdfController> _logger;
    private readonly IEnviarPdf _enviarPdf;
    private readonly IStorage _storage;
    private readonly IExtracaoPdfRepository _extracaoPdfRepository;

    public PdfController(ILogger<PdfController> logger, IEnviarPdf enviarPdf, IStorage storage, IExtracaoPdfRepository extracaoPdfRepository)
    {
        _logger = logger;
        _enviarPdf = enviarPdf;
        _storage = storage;
        _extracaoPdfRepository = extracaoPdfRepository;
    }

    public async Task<ActionResult> PostPdf([FromForm] PostPdfRequest request)
    {
        Guid id = await _enviarPdf.EnviarPdfAsync(new EnviarPdfDTO
        {
            NomeArquivo = request.Arquivo?.FileName,
            PdfStream = request.Arquivo?.OpenReadStream()
        }).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetInfo), new { id = id }, new { Id = id });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetInfo(Guid id)
    {
        ExtracaoPdf extracaoPdf = await _extracaoPdfRepository.ObterPorId(id);
        if (extracaoPdf is null)
        {
            return NotFound();
        }
        string? urlArquivo = null;
        if (extracaoPdf.Status == "finalizado")
        {
            urlArquivo = Url.ActionLink(nameof(DownloadArquivo), null, new { id });
        }
        return Ok(new { Id = id, Status = extracaoPdf.Status, Arquivo = urlArquivo });
    }

    [HttpGet("{id:guid}/arquivo")]
    public async Task<ActionResult> DownloadArquivo(Guid id)
    {
        ExtracaoPdf extracaoPdf = await _extracaoPdfRepository.ObterPorId(id);
        if (extracaoPdf is null)
        {
            return NotFound();
        }
        string nomeArquivo = Path.GetFileNameWithoutExtension(extracaoPdf.NomeArquivo);
        Stream arquivo = _storage.ObterStream($"zips/{id:N}");
        return File(arquivo, "application/zip", fileDownloadName: $"{nomeArquivo}.zip", enableRangeProcessing: true);
    }
}
