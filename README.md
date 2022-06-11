# PDF Extractor

API que converte um PDF para imagens. A API recebe o PDF e o envia para uma fila no redis que é processada por um serviço em segundo plano,
este serviço converte as páginas do PDF e armazena um ZIP que pode ser baixado pela API.

## Tecnologias Usadas

- [.NET 6](https://dotnet.microsoft.com/en-us/) Plataforma de código aberto para desenvolvimento de aplicações
- [PostgreSQL](https://www.postgresql.org/) Banco de dados relacional de código aberto
- [Redis](https://redis.io/) Banco de dados em memória de código aberto
- [Dapper](https://dapper-tutorial.net/dapper): Micro ORM
- [FluentMigrator](https://fluentvalidation.net/) Biblioteca para criação de migrações
- [Magick.NET](https://github.com/dlemstra/Magick.NET) Biblioteca para manipulação de imagem

## Como rodar o projeto

Para rodar o projeto, uma das opções é usar o docker compose.

- [Docker](https://docs.docker.com/engine/install/)
- [Docker Compose](https://docs.docker.com/compose/install/)

Tendo o docker e o docker compose instalados, basta executar o seguinte
comando na pasta raiz do projeto.

```bash
docker compose up -d
```

Caso seja necessário escalar o número de instâncias do serviço que processa a fila de extração, o comando ficaria assim:

```
# "3" é o número de instancias que serão criadas
docker compose up -d --scale worker=3
```

Ao rodar dessa forma, uma pasta `volumes` será criada, dentro dela estarão os volumes do postgres e da aplicação. A pasta `volumes/storage` é
uma pasta usada pela aplicação e é onde serão armazenados os arquivos PDF e imagens geradas.

A API ficará exposta na porta `5000` e acessando a rota `/swagger` se tem acesso a lista dos endpoints da API.

## Como configurar o ambiente de desenvolvimento

É preciso ter as seguintes dependências configuradas:
- [PostgreSQL](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download/)
- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Ghostscript](https://www.ghostscript.com/releases/gsdnld.html)

Para gerar o build do projeto basta apenas rodar `dotnet build` na raiz do projeto, onde está o arquivo da solução (.sln).

Para rodar as migrações no banco, uma possibilidade é usar o projeto `PdfExtractor.Migrations.Runner`, o comando de execução precisa informar
duas variáveis de ambiente, a conexão com o postgres e quantidade de tentativas para aplicar as migrações.

```bash
Configuracao__ConexaoPostgres="conexao-do-postgres-aqui" Configuracao__QtdeTentativas=1 dotnet run --project src/PdfExtractor.Migrations.Runner
```

No arquivo `appsettings.json` tanto da API quanto do Service Worker é necessário adicionar a seguinte seção:

```json
{
    "Configuracao": {
        "ConexaoPostgres": "string de conexão com o Postgres",
        "CaminhoStorage": "caminho de uma pasta existente onde serão armazenados os arquivos",
        "ConexaoRedis": "string de conexão com o redis",
        "TempoIntervaloProcessamento": 1000 // Necessário apenas para o Service Worker
    }
}
```

Tendo isso configurados, tanto a API quando o Service Worker podem ser iniciados pelo comando `dotnet run`.

API:
```bash
dotnet run --project src/PdfExtractor.WebApi
```

A API ficará exposta na porta `5000` e acessando a rota `/swagger` se tem acesso a lista dos endpoints da API.

Antes de inciar o Service Worker, verifique se o Ghostscript está instalado e se o caminho para o seu executálvel
está disponível na variável de ambiente `$PATH`.

Worker:
```bash
dotnet run --project src/PdfExtractor.Worker
```
