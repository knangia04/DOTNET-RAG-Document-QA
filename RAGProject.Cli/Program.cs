// See https://aka.ms/new-console-template for more information
using RAGProject.Cli;
using SemanticSlicer;
using SemanticSlicer.Models;
using System.Security.Cryptography.X509Certificates;

try
{
    //Your code goes here

    var llmService = new LLMService(); // initializes LLM service with Gemini model and API key from GlobalSettings

    // await Samples.TestGenerateContent(llmService);
    // await Samples.TestEmbedding(llmService);
    // await Samples.TestVectorStore(llmService);
    // await Samples.ChunkDocument(GlobalSettings.ShakespeareWorks_DataSourceLocation);

    int delayMs = 1500; // delay between API requests to avoid rate limiting
    var vectorStore = new FileVectorStore(new GeminiEmbeddingGenerator(llmService), GlobalSettings.VECTOR_STORE_LOCATION); // initializes file-based vector store

    // Get list of already ingested chunk files - to avoid duplicate processing
    List<string> existingChunks = new List<string>();

    if (Directory.Exists(GlobalSettings.VECTOR_STORE_LOCATION))
    {
        existingChunks.AddRange(Directory.EnumerateFiles(GlobalSettings.VECTOR_STORE_LOCATION, "*.json"));
    }

    // Get all text documents in specified folder
    var documentFiles = Directory.GetFiles(GlobalSettings.DOCUMENTS_FOLDER_PATH, "*.txt");
    
    foreach (var file in documentFiles)
    {
        string fileName = Path.GetFileName(file);
        bool alreadyIngested = false;

        // Check if any existing chunk belongs to this file
        foreach (var chunkFile in existingChunks)
        {
            string json = File.ReadAllText(chunkFile);
            if (json.Contains(fileName))
            {
                alreadyIngested = true;
                break;
            }
        }

        // Skip ingestion if already processed
        if (alreadyIngested)
        {
            Console.WriteLine($"{fileName} already ingested. Skipping.");
            continue;
        }

        // Ingest the document
        Console.WriteLine($"Ingesting new document: {fileName}");
        
        var slicer = new Slicer(); // initialize document slicer
        var chunks = slicer.GetDocumentChunks(File.ReadAllText(file)); // split document into chunks
        Console.WriteLine($"Total chunks created: {chunks.Count}");

        // Add each chunk to vector store with metadata
        foreach (var chunk in chunks)
        {
            await vectorStore.AddChunkAsync(
                chunk.Content,
                new Dictionary<string, string>
                {
                    {"SourceFile", fileName}, // track which file the chunk came from
                    {"ChunkIndex", chunk.Index.ToString()} // track chunk index within the file
                });

            await Task.Delay(delayMs); // small delay to prevent API rate limiting
        }

        Console.WriteLine($"{fileName} ingestion complete.");

    }

    Console.WriteLine("All documents processed.");

    Console.WriteLine("\nYou can now ask questions based on the ingested documents. Type 'exit' to quit.");
    
    // Interactive Q&A loop
    while (true)
    {
        Console.Write("\nEnter your question: ");

        string? question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question) || question.ToLower() == "exit")
        {
            break; // exit loop if input is empty or 'exit'
        }

        // Retrieve top-N relevant chunks from vector store
        var topChunks = await vectorStore.GetChunksAsync(question, topN: 5);

        if (topChunks.Count == 0)
        {
            Console.WriteLine("No relevant information found in the documents.");
            continue;
        }

        // Build context string from retrieved top chunks to proivide to LLM
        string context = "";
        foreach(var chunk in topChunks)
        {
            if (chunk.Metadata != null && chunk.Metadata.ContainsKey("SourceFile") && chunk.Metadata.ContainsKey("ChunkIndex"))
            {
                context += $"[Source: {chunk.Metadata["SourceFile"]} | ChunkIndex: {chunk.Metadata["ChunkIndex"]}]";
            }

            context += chunk.Text + "\n----\n";
        }

        // Construct prompt for the LLM
        string prompt = $"""
        Using the following context from the documents, answer the question below. Do not make any assumptions or make up information.

        Context:
        {context}

        Question: {question}\n
        """;

        // Call the LLM service to get an answer
        try
        {
            string answer = await llmService.GetResponse(prompt);
            Console.WriteLine("\nAnswer:");
            Console.WriteLine(answer);
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nError retrieving answer from LLM:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Please wait a bit or try again later.");
        }

    }

}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}


// Console.WriteLine("Press any key to exit...");
// Console.ReadKey();


