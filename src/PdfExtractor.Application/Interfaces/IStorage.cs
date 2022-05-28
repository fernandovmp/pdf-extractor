namespace PdfExtractor.Application.Interfaces;

public interface IStorage
{
    Stream ObterStream(string caminho);
    Stream ObterStream(string caminho, FileShare fileShare);
    Stream CriarArquivo(string caminho);
    void ApagarArquivo(string caminho);
}
