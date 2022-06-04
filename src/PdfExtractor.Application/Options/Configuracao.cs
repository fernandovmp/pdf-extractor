namespace PdfExtractor.Application.Options;

public class Configuracao
{
    public string? ConexaoRedis { get; set; }
    public string? CaminhoStorage { get; set; }
    public string? CaminhoGhostscript { get; set; }
    public string? ConexaoPostgres { get; set; }
    public int TempoIntervaloProcessamento { get; set; }
}
