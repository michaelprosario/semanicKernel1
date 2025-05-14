#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using AppInfra;

namespace ContentIngestion;

public class DataUploader(
    IVectorStore vectorStore, 
    ITextEmbeddingGenerationService textEmbeddingGenerationService
    )
{
    /// <summary>
    /// Generate an embedding for each text paragraph and upload it to the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to upload the fragments to.</param>
    /// <param name="fragments">The fragments to upload.</param>
    /// <returns>An async task.</returns>
    public async Task GenerateEmbeddingsAndUpload(
        string collectionName, 
        IEnumerable<ContentItemFragment> fragments
        )
    {
        var collection = vectorStore.GetCollection<string, ContentItemFragment>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();

        foreach (var fragment in fragments)
        {
            // Generate the text embedding.
            Console.WriteLine($"Generating embedding for fragment: {fragment.Id}");
            fragment.Embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(fragment.Content);

            // Upload 
            Console.WriteLine($"Upserting fragment: {fragment.Id}");
            await collection.UpsertAsync(fragment);

            Console.WriteLine();
        }
    }
}