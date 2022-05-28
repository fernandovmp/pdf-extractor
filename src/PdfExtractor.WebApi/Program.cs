using PdfExtractor.Application.Interfaces;
using PdfExtractor.Application.Options;
using PdfExtractor.Application.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Configuracao>(builder.Configuration.GetSection("Configuracao"));
builder.Services.AddScoped<IFilaPdf, FilaExtracao>();
builder.Services.AddScoped<IEnviarPdf, EnviarPdfExtracao>();
builder.Services.AddScoped<IStorage, Storage>();
builder.Services.AddScoped<IExtracaoPdfRepository, ExtracaoPdfRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
