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

    public PdfController(ILogger<PdfController> logger, IEnviarPdf enviarPdf, IStorage storage)
    {
        _logger = logger;
        _enviarPdf = enviarPdf;
        _storage = storage;
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
    public ActionResult GetInfo(Guid id)
    {
        string? urlArquivo = Url.ActionLink(nameof(DownloadArquivo), null, new { id });
        return Ok(new { Id = id, Arquivo = urlArquivo });
    }

    [HttpGet("{id:guid}/arquivo")]
    public ActionResult DownloadArquivo(Guid id)
    {
        Stream arquivo = _storage.ObterStream($"zips/{id:N}");
        return File(arquivo, "application/zip", fileDownloadName: $"{id:N}.zip", enableRangeProcessing: true);
    }
}
