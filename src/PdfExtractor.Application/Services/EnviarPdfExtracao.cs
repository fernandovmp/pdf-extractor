using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;

namespace PdfExtractor.Application.Services;

public class EnviarPdfExtracao : IEnviarPdf
{
    private readonly IFilaPdf _filaExtracao;
    private readonly IOptions<Configuracao> _configuracao;
    private readonly IStorage _storage;

    public EnviarPdfExtracao(IFilaPdf filaExtracao, IOptions<Configuracao> configuracao, IStorage storage)
    {
        _filaExtracao = filaExtracao;
        _configuracao = configuracao;
        _storage = storage;
    }

    public async Task<Guid> EnviarPdfAsync(EnviarPdfDTO enviarPdfDTO)
    {
        Guid id = Guid.NewGuid();
        string caminhoArquivo = Path.Combine("pdfs", id.ToString("N"));
        if (enviarPdfDTO.PdfStream is null)
        {
            throw new ArgumentNullException("Arquivo obrigatório");
        }
        using var stream = _storage.CriarArquivo(caminhoArquivo);
        await enviarPdfDTO.PdfStream.CopyToAsync(stream)
            .ConfigureAwait(false);
        try
        {
            var pdfDto = new PdfExtractorDTO
            {
                Id = id,
                NomeArquivo = enviarPdfDTO.NomeArquivo
            };
            await _filaExtracao.ColocarNaFilaAsync(pdfDto)
                .ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            _storage.ApagarArquivo(caminhoArquivo);
            throw;
        }
        return id;
    }
}
