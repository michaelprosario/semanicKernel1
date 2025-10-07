# RAG for .NET Beginners: Building Your First AI-Powered Document Chat

Hey there 👋 

If you're just getting started with AI development and wondering what all the fuss is about RAG (Retrieval-Augmented Generation), you're in the right place. Today we're going to break down a real-world .NET project that shows you exactly how to build an AI chat system that can answer questions about your own documents.

Don't worry if terms like "embeddings" or "vector databases" sound scary - by the end of this post, you'll understand exactly what they are and how to use them in your .NET applications.

## What Problem Are We Solving?

Picture this: You have a bunch of text files (maybe documentation, articles, or transcripts), and you want to build a chatbot that can answer questions about them. 

The naive approach might be to just dump all your text into ChatGPT's context window and hope for the best. But there are problems:
- ChatGPT has token limits (you can't send huge amounts of text)
- It's expensive to send lots of text every time
- The AI might get confused with too much information at once

RAG solves this by being smart about what information to show the AI. It's like having a really good librarian who finds the relevant books before you start researching.

## The Two-Step Dance: Ingestion + Retrieval

Our solution has two main parts:

1. **Ingestion** (`ContentIngestion/Program.cs`) - Prepare your documents for AI consumption
2. **Retrieval & Generation** (`RagChatArea.razor`) - Find relevant info and let AI answer questions

Let's dive into each part!

## Part 1: Document Ingestion - The Setup Phase

### Understanding the Basic .NET Structure

Let's start with `ContentIngestion/Program.cs`. If you're familiar with .NET console applications, this should look pretty standard:

```csharp
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
```

This is the standard pattern for a console app using Dependency Injection (DI). We're:
1. Setting up configuration (reading API keys, connection strings, etc.)
2. Registering services in the DI container
3. Building the container and running our app

### The Key Services We're Registering

In `ConfigureServices`, we register some important services:

```csharp
// Register text embedding generation service and Postgres vector store.
string textEmbeddingModel = "text-embedding-3-small";
string openAiApiKey = configuration["OPENAI_API_KEY"];
string postgresConnectionString = configuration["DB_CONNECTION"];

services.AddOpenAITextEmbeddingGeneration(textEmbeddingModel, openAiApiKey);
services.AddPostgresVectorStore(postgresConnectionString);
```

**What's happening here?**
- **Text Embedding Service**: This is our connection to OpenAI's API that converts text into mathematical vectors
- **Vector Store**: A special database (PostgreSQL with pgvector extension) that can store and search through these vectors

**For beginners**: Think of embeddings as a way to convert text into numbers that capture the "meaning" of the text. Similar concepts end up with similar numbers.

### Processing Files: The ContentFragmentMaker

Now let's look at how we actually process text files. The `ContentFragmentMaker` class does something really important - it breaks big text files into smaller, manageable pieces:

```csharp
public List<string> GetChunks(string text, int chunkSize, int overlapSize)
{
    List<string> chunks = [];
    int start = 0;

    while (start < text.Length)
    {
        int length = Math.Min(chunkSize, text.Length - start);
        chunks.Add(text.Substring(start, length));
        start += chunkSize - overlapSize;
    }

    return chunks;
}
```

**Why do we chunk text?**
1. **AI models have limits**: You can't send infinite text to AI models
2. **Better search**: Smaller chunks make it easier to find specific information
3. **Overlap prevents lost context**: The overlap ensures we don't accidentally split important information

**For beginners**: Imagine trying to find a recipe in a cookbook. It's easier to search through individual recipes than trying to scan the entire book at once.

### Text Cleaning

Before chunking, we clean up the text:

```csharp
public string RemoveNonAlphanumeric(string input)
{
    return System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
}

public string RemoveNewLines(string input)
{
    return input.Replace("\n", " ").Replace("\r", " ");
}
```

This removes special characters and normalizes whitespace. Think of it like preparing ingredients before cooking - you want clean, consistent input.

### The DataUploader: Where the Magic Happens

The `DataUploader` class is where we convert text into searchable vectors:

```csharp
public async Task GenerateEmbeddingsAndUpload(
    string collectionName, 
    IEnumerable<ContentItemFragment> fragments)
{
    var collection = vectorStore.GetCollection<string, ContentItemFragment>(collectionName);

    foreach (var fragment in fragments)
    {
        // Generate the text embedding.
        Console.WriteLine($"Generating embedding for fragment: {fragment.Id}");
        fragment.Embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(fragment.Content);

        // Upload 
        Console.WriteLine($"Upserting fragment: {fragment.Id}");
        await collection.UpsertAsync(fragment);
    }
}
```

**What's happening step by step:**
1. For each text chunk, call OpenAI's API to get an embedding (array of numbers)
2. Store both the original text AND the embedding in our vector database
3. The database can now find similar chunks by comparing these number arrays

### The ContentItemFragment Model

Let's look at our data model:

```csharp
public class ContentItemFragment
{
    [VectorStoreRecordKey(StoragePropertyName = "id")]        
    public string Id { get; set; }

    [VectorStoreRecordData(StoragePropertyName = "content_item_id")]
    public Guid ContentItemId { get; set; }

    [VectorStoreRecordVector(Dimensions: 4, DistanceFunction.CosineDistance, StoragePropertyName = "embedding")]
    public ReadOnlyMemory<float>? Embedding { get; set; }

    [VectorStoreRecordData(StoragePropertyName = "content")]
    [TextSearchResultValue]
    public string Content { get; set; } = string.Empty;

    [VectorStoreRecordData(StoragePropertyName = "source")]
    [TextSearchResultName]
    public string Source { get; set; } = string.Empty;
}
```

**For beginners**: These attributes tell the system:
- `VectorStoreRecordKey`: This is our primary key
- `VectorStoreRecordVector`: This field stores the embedding (the array of numbers)
- `TextSearchResultValue`: This is the actual text content we'll show users
- `TextSearchResultName`: This is like a title or source reference

## Part 2: The RAG Chat Interface - Where Users Interact

Now let's look at `RagChatArea.razor` - this is a Blazor component that creates our chat interface.

### Setting Up the Chat Brain

When the component initializes, it sets up the "search brain":

```csharp
protected override async Task OnInitializedAsync()
{
    string openAiApiKey = Configuration["OPENAI_API_KEY"];
    string modelId = "gpt-4o-mini";

    // Create a kernel with Azure OpenAI chat completion
    var builder = Kernel.CreateBuilder();
    builder.Services.AddOpenAIChatCompletion(modelId, openAiApiKey);

    // Build a text search plugin with vector store search and add to the kernel    
    var vectorStoreRecordCollection = vectorStore.GetCollection<string, ContentItemFragment>("content_item_fragment");
    textSearch = new VectorStoreTextSearch<ContentItemFragment>(vectorStoreRecordCollection, textEmbeddingGeneration);
    
    kernel = builder.Build();
    
    // Build a text search plugin with vector store search and add to the kernel
    var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
    kernel.Plugins.Add(searchPlugin);
}
```

**What's happening here?**
1. **Semantic Kernel Setup**: Microsoft's Semantic Kernel is like a Swiss Army knife for AI development
2. **Chat Completion**: This connects to OpenAI's GPT models for generating responses
3. **Search Plugin**: This creates a search tool that can find relevant documents
4. **Plugin Registration**: We add the search tool to our AI "kernel" so it can use it

**For beginners**: Think of Semantic Kernel as a framework that makes it easy to combine AI models with other tools (like search).

### The Smart Prompt Template

Here's where the real magic happens. Instead of just asking ChatGPT a question, we use a template that first searches our documents:

```handlebars
As an AI assistant named Chris, provide a concise and accurate answer to the user's question based on the information retrieved from the text search results below.
You should play the role of a leadership and business coach.
If the information is insufficient, respond with 'I don't know'.

{{#with (SearchPlugin-GetTextSearchResults query)}}  
    {{#each this}}  
    Name: {{Name}}
    Value: {{Value}}
    Link: {{Link}}
    -----------------
    {{/each}}  
{{/with}}  

{{query}}

Include citations to the relevant information where it is referenced in the response.
```

**What's this template doing?**
1. `{{#with (SearchPlugin-GetTextSearchResults query)}}` - This automatically searches our documents
2. `{{#each this}}` - Loop through each relevant document found
3. Show the AI the relevant content BEFORE asking it to answer
4. Ask for citations so users know where information came from

**For beginners**: This is using Handlebars templating. The curly braces `{{ }}` are placeholders that get filled in with actual data.

### The Chat Flow - Step by Step

When a user sends a message, here's exactly what happens:

```csharp
private async Task SendMessage()
{
    if (string.IsNullOrWhiteSpace(userInput) || isLoading)
        return;

    string message = userInput.Trim();
    userInput = string.Empty;

    // Add user message to history
    chatHistory.AddUserMessage(message);
    StateHasChanged();

    isLoading = true;
    StateHasChanged();

    try
    {
        // Get response from AI
        await GetAssistantResponse(message);
    }
    catch (Exception ex)
    {
        chatHistory.AddAssistantMessage($"I encountered an error: {ex.Message}");
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

And in `GetAssistantResponse`:

```csharp
private async Task GetAssistantResponse(string message)
{
    string promptTemplate = GetPromptTemplate();
    KernelArguments arguments = new() { { "query", message } };
    
    var result = await kernel.InvokePromptAsync(
        promptTemplate,
        arguments,
        templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
        promptTemplateFactory: promptTemplateFactory
    );
    
    var chatResult = result.ToString(); 
    chatHistory.AddMessage(AuthorRole.Assistant, chatResult ?? string.Empty);
}
```

**The step-by-step process:**
1. User types a question
2. The question gets sent to our search plugin
3. Search plugin converts the question to an embedding
4. Database finds similar document chunks
5. Relevant chunks get inserted into our prompt template
6. The full prompt (with relevant docs) gets sent to ChatGPT
7. ChatGPT responds based on the found documents
8. User sees the response with citations

## Key .NET Concepts You Should Understand

### Dependency Injection
```csharp
services.AddOpenAITextEmbeddingGeneration(textEmbeddingModel, openAiApiKey);
services.AddPostgresVectorStore(postgresConnectionString);
```
We register services in the DI container so they can be injected where needed.

### Async/Await Pattern
```csharp
fragment.Embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(fragment.Content);
```
AI operations take time, so we use async programming to avoid blocking the UI.

### Configuration System
```csharp
string openAiApiKey = Configuration["OPENAI_API_KEY"];
```
.NET's configuration system lets us read settings from various sources (environment variables, user secrets, etc.).

### Blazor Component Lifecycle
```csharp
protected override async Task OnInitializedAsync()
```
Blazor components have lifecycle methods where we can set up our services.

## Why This Architecture Works Well

**Separation of Concerns**: Ingestion and chat are separate - you could run ingestion as a batch job and chat as a web service.

**Scalability**: Vector search is fast even with thousands of documents.

**Flexibility**: Want to add new documents? Just run the ingestion process again.

**Accuracy**: The AI can only answer based on your documents, reducing hallucinations.

## Common Gotchas for .NET Developers

1. **Don't forget to install pgvector extension** in your PostgreSQL database
2. **API costs add up** - each embedding call costs money
3. **Chunk size matters** - too small and you lose context, too big and search becomes less precise
4. **Always handle exceptions** when calling external APIs

## Next Steps for Learning

If you want to build on this:

1. **Experiment with chunk sizes** - try different values and see how it affects search quality
2. **Add metadata filtering** - filter by document type, date, etc.
3. **Implement hybrid search** - combine vector search with traditional keyword search
4. **Add document upload** - let users upload their own files through the web interface
5. **Improve error handling** - add retry logic, better user feedback

## Wrapping Up

RAG might sound complicated, but it's really just three steps:
1. **Prepare**: Break documents into chunks and convert to vectors
2. **Search**: Find relevant chunks when users ask questions  
3. **Generate**: Let AI answer based on found information

The .NET ecosystem makes this surprisingly straightforward with libraries like Semantic Kernel and good database support. You don't need to be an AI expert - you just need to understand how to connect the pieces together.

The key insight is that modern AI works best when you give it relevant, focused information rather than everything at once. RAG is just a systematic way to do that.

Happy coding! 🚀

---

*Ready to try this yourself? You'll need:*
- *.NET 8 SDK*
- *PostgreSQL with pgvector extension*  
- *OpenAI API key*
- *Basic understanding of dependency injection and async programming*

*The code shows you exactly how to wire everything together - just follow the patterns and you'll have your own AI-powered document chat in no time!*