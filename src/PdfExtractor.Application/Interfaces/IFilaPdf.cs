using PdfExtractor.Application.Model;

namespace PdfExtractor.Application.Interfaces;

public interface IFilaPdf
{
    Task ColocarNaFilaAsync(PdfExtractorDTO pdf);
    Task<PdfExtractorDTO?> ObterProximoDaFilaAsync();
}
