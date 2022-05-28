FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/app

COPY src/PdfExtractor.Migrations.Runner src/PdfExtractor.Migrations.Runner
COPY src/PdfExtractor.Migrations src/PdfExtractor.Migrations
RUN dotnet restore src/PdfExtractor.Migrations.Runner

RUN dotnet publish src/PdfExtractor.Migrations.Runner -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /src/app/out .
CMD dotnet PdfExtractor.Migrations.Runner.dll 
