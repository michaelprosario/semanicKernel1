using Microsoft.Extensions.DependencyInjection;
using AppInfra;



public class ConsoleApplication
{
    public ConsoleApplication()
    {
    }



    public void ProcessFilesInDirectory(string folderPath)
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

        // serialize the fragments to JSON
        string json = System.Text.Json.JsonSerializer.Serialize(allFragments, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
         
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
        // Create service collection
        var services = new ServiceCollection();
        ConfigureServices(services);

        // Build service provider
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the application instance from the service provider
        var app = serviceProvider.GetRequiredService<ConsoleApplication>();
        
        // Run the application
        app.Run();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Register services
        
        
        // Register application
        services.AddSingleton<ConsoleApplication>();
        
        // Optional: Add logging, configuration, etc.
        // services.AddLogging();
    }
}