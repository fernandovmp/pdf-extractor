FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/app

COPY src src
RUN dotnet restore src/PdfExtractor.WebApi

RUN dotnet publish src/PdfExtractor.WebApi -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /src/app/out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet PdfExtractor.WebApi.dll
