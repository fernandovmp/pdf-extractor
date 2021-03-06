using System.Text.Json;
using Microsoft.Extensions.Options;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;
using StackExchange.Redis;

namespace PdfExtractor.Application.Services;

public class FilaExtracao : IFilaPdf
{
    private readonly IOptions<Configuracao> _configuracao;

    public FilaExtracao(IOptions<Configuracao> configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task ColocarNaFilaAsync(PdfExtractorDTO pdf)
    {
        using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuracao.Value.ConexaoRedis);
        IDatabase database = redis.GetDatabase();
        string json = JsonSerializer.Serialize(pdf);
        await database.ListLeftPushAsync("pdf", json)
            .ConfigureAwait(false); ;
    }

    public async Task<PdfExtractorDTO?> ObterProximoDaFilaAsync()
    {
        using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_configuracao.Value.ConexaoRedis);
        IDatabase database = redis.GetDatabase();
        string json = await database.ListLeftPopAsync("pdf")
            .ConfigureAwait(false);
        if (json is null)
        {
            return null;
        }
        return JsonSerializer.Deserialize<PdfExtractorDTO>(json)!;
    }
}
