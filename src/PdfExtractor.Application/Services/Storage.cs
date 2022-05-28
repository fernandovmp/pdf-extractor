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

    public void ApagarArquivo(string caminho)
    {
        string caminhoArquivo = ObterCaminhoAbsoluto(caminho);
        if (File.Exists(caminhoArquivo))
        {
            File.Delete(caminhoArquivo);
        }
    }

    public Stream CriarArquivo(string caminho)
    {
        string caminhoArquivo = ObterCaminhoAbsoluto(caminho);
        string? diretorio = Path.GetDirectoryName(caminhoArquivo);
        if (!string.IsNullOrWhiteSpace(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }
        return File.Open(caminhoArquivo, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
    }

    public Stream ObterStream(string caminho)
    {
        return ObterStream(caminho, FileShare.Read);
    }

    public Stream ObterStream(string caminho, FileShare fileShare)
    {
        string caminhoArquivo = ObterCaminhoAbsoluto(caminho);
        return File.Open(caminhoArquivo, FileMode.Open, FileAccess.Read, fileShare);
    }

    private string ObterCaminhoAbsoluto(string caminho)
    {
        string caminhoRaiz = _configuracao.Value.CaminhoStorage ?? "/storage";
        return Path.Combine(caminhoRaiz, caminho);
    }
}
