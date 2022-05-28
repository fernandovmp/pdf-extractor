using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Options;

namespace PdfExtractor.Application.Services;

public class Storage : IStorage
{
    private readonly IOptions<Configuracao> _configuracao;

    public Storage(IOptions<Configuracao> configuracao)
    {
        _configuracao = configuracao;
    }

    public Stream ObterStream(string caminho)
    {
        string caminhoRaiz = _configuracao.Value.CaminhoStorage ?? "/storage";
        string caminhoArquivo = Path.Combine(caminhoRaiz, caminho);
        return File.Open(caminhoArquivo, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
