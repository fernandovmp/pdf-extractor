using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Model;
using PdfExtractor.Application.Options;

namespace PdfExtractor.Application.Services;

public class ExtracaoPdfRepository : IExtracaoPdfRepository
{
    private readonly IOptions<Configuracao> _configuracao;

    public ExtracaoPdfRepository(IOptions<Configuracao> configuracao)
    {
        _configuracao = configuracao;
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private const string QueryInsert = @"insert into extracao_pdf (id, nome_arquivo, status) values (@Id, @NomeArquivo, @Status::status_extracao_enum);";
    private const string QueryUpdateStatus = @"update extracao_pdf set status = @Status::status_extracao_enum, data_atualizacao = current_timestamp where id = @Id";
    private const string QueryObterPorId = @"select id, nome_arquivo, status from extracao_pdf where id = @Id";

    public async Task Adicionar(ExtracaoPdf extracaoPdf)
    {
        using IDbConnection connection = new NpgsqlConnection(_configuracao.Value.ConexaoPostgres);
        await connection.ExecuteAsync(QueryInsert, extracaoPdf)
            .ConfigureAwait(false);
    }

    public async Task AtualizarStatus(Guid id, string status)
    {
        using IDbConnection connection = new NpgsqlConnection(_configuracao.Value.ConexaoPostgres);
        await connection.ExecuteAsync(QueryUpdateStatus, new { Id = id, Status = status })
            .ConfigureAwait(false);
    }

    public async Task<ExtracaoPdf> ObterPorId(Guid id)
    {
        using IDbConnection connection = new NpgsqlConnection(_configuracao.Value.ConexaoPostgres);
        return await connection.QueryFirstOrDefaultAsync<ExtracaoPdf>(QueryObterPorId, new { Id = id })
            .ConfigureAwait(false);
    }
}
