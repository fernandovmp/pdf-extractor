FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/app

COPY src src
RUN dotnet restore src/PdfExtractor.Worker

RUN dotnet publish src/PdfExtractor.Worker -c Release -o out

FROM ubuntu:22.04 AS runtime-deps
WORKDIR /runtime-deps
RUN apt update && apt install curl -y
RUN mkdir ghostscript \
    && curl https://github.com/ArtifexSoftware/ghostpdl-downloads/releases/download/gs9561/ghostscript-9.56.1-linux-x86_64.tgz \
    -L --output ghostscript/gs.tgz \
    && tar -xf ghostscript/gs.tgz -C ghostscript \
    && mv ghostscript/ghostscript-9.56.1-linux-x86_64/gs-9561-linux-x86_64 gs

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=runtime-deps /runtime-deps /runtime-deps
ENV PATH /runtime-deps:$PATH
COPY --from=build /src/app/out .
CMD dotnet PdfExtractor.Worker.dll
