using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Options;
using PdfExtractor.Application.Services;
using PdfExtractor.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<Configuracao>(context.Configuration.GetSection("Configuracao"));
        services.AddSingleton<IFilaPdf, FilaExtracao>();
        services.AddSingleton<IEnviarPdf, EnviarPdfExtracao>();
        services.AddSingleton<IStorage, Storage>();
    })
    .Build();

await host.RunAsync();
