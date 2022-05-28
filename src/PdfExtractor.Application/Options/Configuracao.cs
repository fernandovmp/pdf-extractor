namespace PdfExtractor.Application.Options;

public class Configuracao
{
    public string? ConexaoRedis { get; set; }
    public string? CaminhoStorage { get; set; }
    public string? CaminhoGhostscript { get; set; }
}
