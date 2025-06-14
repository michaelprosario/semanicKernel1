As a reflection from the Microsoft Build conference this year, I tried to explore the different technology pathways Microsoft offers related to GenAI technology.   Developers have no shortage of agent or LLM frameworks.   In the Microsoft ecosystem, you can observe a great deal of confusion at times.   Should I build my solutions using Microsoft.Extensions.AI?  Some of the really cool agents innovations from Microsoft were built with a framework called AutoGen.  Should I consider that?   Where does the Semantic kernel framework fit into this?  Why should I use the Microsoft agent frameworks when I could just use the SDK from the LLM provider?

If your team desires to operate at low level primitives of LLM tooling, you can consider using Microsoft.Extensions.AI.   I generally like the feel of the abstractions.   These abstractions often use a provider pattern so you can experiment with different providers related to LLMs, embeddings, and vector databases.   At the Build conference, Microsoft announced that all developer focused agent frameworks will consolidate under Semantic Kernel.   In future blog posts, we'll explore ways that semantic kernel will link into the Azure platform as a service offering, Azure AI foundry.  Semantic kernel builds up on the lower level abstractions from Microsoft.Extensions.AI.

For today, I thought it might be helpful to explore an OpenAI chat scenario using Dotnet, BlazorServer, and Semantic kernel.    This blog post will focus upon introducing some key concepts that we'll build up on for future post.

To explore some of the major ideas of semantic kernel, we will tear down code from a semantic kernel POC repository that I build. You can find all the code for this tutorial at https://github.com/michaelprosario/semanicKernel1. This repository leverages Blazor server for the UI.   In other parts of the repository, you can find code samples for doing retrieval augmented generation with Postgres(PGVector).

To run the code sample, please do the following:
``` bash 
git clone https://github.com/michaelprosario/semanicKernel1
cd semanticKernel1/AiPlayground1
export DB_CONNECTION="Connection string to postgres with PGVector"  
export OPENAI_API_KEY="MyOpenAIKey"
dotnet build
dotnet run
``` 

If you're new to Blazor, consider checking out the following tutorial:
https://dotnet.microsoft.com/en-us/learn/aspnet/blazor-tutorial/intro

In particular, we will start our review with the following file:
/semanicKernel1/blob/main/AiPlayground1/Components/Pages/ChatArea.razor.

### Key component members
``` csharp
private  ChatHistory  chatHistory  =  new();
private  string  userInput  =  string.Empty;
private  bool  isLoading  =  false;
private  ElementReference  chatContainerRef;
Kernel  kernel;
IChatCompletionService  ChatCompletionService;
```


- **chatHistory**: Semantic Kernel provides a helpful abstraction for tracking chat history. This tracks the conversation between the user and the AI agent.
- **kernel**: This will track our semantic kernel instance
- **ChatCompletionService**: This service will generate AI-powered chat completions. Its main purpose is to abstract away the specifics of different chat model providers. From the perspective of a conversation, the LLM is constantly responding to the current chat message from the user and the chat history.

  

In the following code, we setup our kernel and ChatCompletionService. For Dotnet Core developers, the service construction uses the same builder pattern used by Dotnet core middleware. To run this sample code, you will need to provide your own key to OPENAI SDK.

``` csharp
string  openAiApiKey  = Configuration["OPENAI_API_KEY"];
string  modelId  =  "gpt-4o-mini";
var  builder  = Kernel.CreateBuilder();
builder.Services.AddOpenAIChatCompletion(modelId, openAiApiKey);
kernel = builder.Build();
ChatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
```
In the following code, we execute the chat completion service to get responses from the AI. Notice that we add a user message to start this process. When the LLM responds, we the response to the chat history.

  

``` csharp

chatHistory.AddUserMessage(message);
OpenAIPromptExecutionSettings  executionSettings  =  new(){
FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Get response from AI
var  result  =  await ChatCompletionService.GetChatMessageContentAsync(
chatHistory,
executionSettings: executionSettings,
kernel
);

// Add AI response to history
chatHistory.AddMessage(result.Role, result.Content ??  string.Empty);

```

> Written with [StackEdit](https://stackedit.io/).