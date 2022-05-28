namespace PdfExtractor.Application.Model;

public class EnviarPdfDTO
{
    public Stream? PdfStream { get; set; }
    public string? NomeArquivo { get; set; }
}
