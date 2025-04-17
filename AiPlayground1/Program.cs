#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.SemanticKernel;
using AiPlayground1.Components;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using AppInfra;

var builder = WebApplication.CreateBuilder(args);

// Register text embedding generation service and Postgres vector store.
string textEmbeddingModel = "text-embedding-3-small";
string openAiApiKey = builder.Configuration["OPENAI_API_KEY"];
string postgresConnectionString = builder.Configuration["DB_CONNECTION"];

if (string.IsNullOrEmpty(openAiApiKey))
{
    throw new ArgumentException("OpenAI API key is not set. Please set the OPENAI_API_KEY environment variable.");
}
if (string.IsNullOrEmpty(postgresConnectionString))
{
    throw new ArgumentException("Postgres connection string is not set. Please set the DB_CONNECTION environment variable.");
}


builder.Services.AddOpenAITextEmbeddingGeneration(textEmbeddingModel, openAiApiKey);
builder.Services.AddPostgresVectorStore(postgresConnectionString);



builder.AddServiceDefaults();

// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<AiPlayground1.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
