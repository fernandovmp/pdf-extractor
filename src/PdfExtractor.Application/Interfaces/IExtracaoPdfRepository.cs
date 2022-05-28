using PdfExtractor.Application.Model;

namespace PdfExtractor.Application.Interfaces;

public interface IExtracaoPdfRepository
{
    Task Adicionar(ExtracaoPdf extracaoPdf);
    Task<ExtracaoPdf> ObterPorId(Guid id);
    Task AtualizarStatus(Guid id, string status);
}
