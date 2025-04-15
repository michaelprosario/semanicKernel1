#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using AppInfra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ContentIngestion;
public class ConsoleApplication
{
    private readonly DataUploader _dataUploader;
    public ConsoleApplication(DataUploader dataUploader)
    {
        _dataUploader = dataUploader;
    }

    public async Task ProcessFilesInDirectory(string folderPath)
    {
       if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"Error: Directory '{folderPath}' does not exist.");
            return;
        }

        string[] textFiles = Directory.GetFiles(folderPath, "*.txt");
        
        if (textFiles.Length == 0)
        {
            Console.WriteLine($"No text files found in '{folderPath}'.");
            return;
        }

        Console.WriteLine($"Found {textFiles.Length} text file(s) in '{folderPath}'.\n");

        List<ContentItemFragment> allFragments = new List<ContentItemFragment>();
        var contentFragmentMaker = new ContentFragmentMaker();
        foreach (string filePath in textFiles)
        {
            var fragments = contentFragmentMaker.ProcessFile(filePath);
            allFragments.AddRange(fragments);
        }

        await _dataUploader.GenerateEmbeddingsAndUpload("collection1", allFragments);
         
    }

    public void Run()
    {
        string folderPath = "/workspaces/semanicKernel1/ContentIngestion/content2";
    
        ProcessFilesInDirectory(folderPath);
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create configuration
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        // Create service collection
        var services = new ServiceCollection();
        ConfigureServices(services,config);

        // Build service provider
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the application instance from the service provider
        var app = serviceProvider.GetRequiredService<ConsoleApplication>();
        
        // Run the application
        app.Run();
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

        //var builder = Kernel.CreateBuilder()
        //    .AddOpenAITextEmbeddingGeneration(textEmbeddingModel, openAiApiKey);

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