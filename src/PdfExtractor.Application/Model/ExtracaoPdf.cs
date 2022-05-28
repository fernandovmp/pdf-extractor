namespace PdfExtractor.Application.Model;

public class ExtracaoPdf
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string Status { get; set; } = "pendente";
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
