using FluentMigrator;
using FluentMigrator.Postgres;
namespace PdfExtractor.Migrations.Migrations;

[Migration(202205281310)]
public class CriarTabelaExtracaoPdf : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE status_extracao_enum AS ENUM('pendente', 'processando', 'finalizado', 'erro');");
        Create.Table("extracao_pdf")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("nome_arquivo").AsString(128).NotNullable()
            .WithColumn("status").AsCustom("status_extracao_enum").NotNullable().Indexed("ix_extracao_pdf_status")
            .WithColumn("data_criacao").AsCustom("timestamp(2)").WithDefaultValue(RawSql.Insert("CURRENT_TIMESTAMP"))
            .WithColumn("data_atualizacao").AsCustom("timestamp(2)").WithDefaultValue(RawSql.Insert("CURRENT_TIMESTAMP"));
    }

    public override void Down()
    {
        Delete.Table("extracao_pdf");
        Execute.Sql("DROP TYPE status_extracao_enum;");
    }
}
