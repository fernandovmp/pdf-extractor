namespace PdfExtractor.Application.Interfaces;

public interface IStorage
{
    Stream ObterStream(string caminho);
}
