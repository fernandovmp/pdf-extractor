using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;

namespace PdfExtractor.Application.Services;

public class EnviarPdfExtracao : IEnviarPdf
{
    private readonly IFilaPdf _filaExtracao;
    private readonly IOptions<Configuracao> _configuracao;

    public EnviarPdfExtracao(IFilaPdf filaExtracao, IOptions<Configuracao> configuracao)
    {
        _filaExtracao = filaExtracao;
        _configuracao = configuracao;
    }

    public async Task<Guid> EnviarPdfAsync(EnviarPdfDTO enviarPdfDTO)
    {
        string caminho = Path.Combine(_configuracao.Value.CaminhoStorage ?? "/storage", "pdfs");
        Directory.CreateDirectory(caminho);
        Guid id = Guid.NewGuid();
        string caminhoArquivo = Path.Combine(caminho, id.ToString("N"));
        using var stream = System.IO.File.Create(caminhoArquivo);
        if (enviarPdfDTO.PdfStream is null)
        {
            throw new ArgumentNullException("Arquivo obrigatório");
        }
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
            File.Delete(caminhoArquivo);
            throw;
        }
        return id;
    }
}
