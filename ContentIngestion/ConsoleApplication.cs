#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using AppInfra;

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

        List<ContentItemFragment> allFragments = [];
        var contentFragmentMaker = new ContentFragmentMaker();
        foreach (string filePath in textFiles)
        {
            var fragments = contentFragmentMaker.ProcessFile(filePath);
            allFragments.AddRange(fragments);
        }

        await _dataUploader.GenerateEmbeddingsAndUpload("content_item_fragment", allFragments);
         
    }

    public async Task Run()
    {
        string folderPath = "C:\\dev\\AiPlayground1\\ContentIngestion\\content2";
    
        await ProcessFilesInDirectory(folderPath);
    }
}
