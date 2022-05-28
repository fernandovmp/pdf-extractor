using PdfExtractor.Application.Model;

namespace PdfExtractor.Application.Interfaces;

public interface IEnviarPdf
{
    Task<Guid> EnviarPdfAsync(EnviarPdfDTO enviarPdfDTO);
}
