using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using PdfExtractor.Migrations.Migrations;
using Polly;

string? envConexaoPostgres = Environment.GetEnvironmentVariable("Configuracao__ConexaoPostgres");
string? envQtdeTentativas = Environment.GetEnvironmentVariable("Configuracao__QtdeTentativas");
if (int.TryParse(envQtdeTentativas, out int qtdeTentativas) && !string.IsNullOrWhiteSpace(envConexaoPostgres))
{
    IServiceProvider serviceProvider = CreateServices(envConexaoPostgres);
    using IServiceScope scope = serviceProvider.CreateScope();
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    ExecutarMigracoes(runner, qtdeTentativas);
}

IServiceProvider CreateServices(string conexaoPostgres)
{
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddPostgres()
            .WithGlobalConnectionString(conexaoPostgres)
            .ScanIn(typeof(CriarTabelaExtracaoPdf).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole())
        .BuildServiceProvider(false);
}

void ExecutarMigracoes(IMigrationRunner runner, int qtdeTentativas)
{
    ISyncPolicy politicaTentativas = Policy
        .Handle<Exception>()
        .WaitAndRetry(qtdeTentativas, i => TimeSpan.FromSeconds(1) * i);
    politicaTentativas.Execute(() => runner.MigrateUp());
}
