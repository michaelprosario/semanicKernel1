#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ContentIngestion;

class Program
{
    static async Task Main(string[] args)
    {
        // Create configuration
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .Build();
        
        // Create service collection
        var services = new ServiceCollection();
        ConfigureServices(services,config);

        // Build service provider
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the application instance from the service provider
        var app = serviceProvider.GetRequiredService<ConsoleApplication>();

        // Run the application
        await app.Run();
    }

    private static void ConfigureServices(ServiceCollection services, IConfiguration configuration)
    {
        // Register services

        // Register text embedding generation service and Postgres vector store.
        string textEmbeddingModel = "text-embedding-3-small";
        string openAiApiKey = configuration["OPENAI_API_KEY"];
        string postgresConnectionString = configuration["DB_CONNECTION"];

        Console.WriteLine($">>>>> Postgres connection string: {postgresConnectionString}");
        Console.WriteLine($">>>>> OpenAI API key: {openAiApiKey}");

        services.AddOpenAITextEmbeddingGeneration(textEmbeddingModel, openAiApiKey);
        services.AddPostgresVectorStore(postgresConnectionString);

        // Register the data uploader.
        services.AddSingleton<DataUploader>();        
        
        // Register application
        services.AddSingleton<ConsoleApplication>();
        
        // Optional: Add logging, configuration, etc.
        // services.AddLogging();
    }
}