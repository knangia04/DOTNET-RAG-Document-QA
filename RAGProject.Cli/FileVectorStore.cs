using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace RAGProject.Cli;

/*
    A simple file-based vector store for storing and retrieving text chunks with embeddings.
    Stores chunks + embeedings and retrieves top-N similar chunks based on cosine similarity.
*/

public class FileVectorStore
{
    private readonly IEmbeddingGenerator _embeddingGenerator; // Embedding generator service
    private readonly string _storageDirectory; // Directory to store chunk files, saved as JSON

    // Constructor. Pass in embedding generator (GEMINI) and directory path
    public FileVectorStore(
        IEmbeddingGenerator embeddingGenerator,
        string storageDirectory)
    {
        _embeddingGenerator = embeddingGenerator;
        _storageDirectory = storageDirectory;

        Directory.CreateDirectory(_storageDirectory); // ensures that directory exists
    }

    // Ingestion step
    public async Task AddChunkAsync(
        string chunkText,
        Dictionary<string, string> metadata)
    {
        var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(chunkText); // calls Gemini embedding API

        var chunk = new StoredChunk
        {
            Text = chunkText,
            Embedding = embedding,
            Metadata = metadata
        }; // creates a chunk object

        // Serialize chunk to JSON and save to file
    
        var path = Path.Combine(_storageDirectory, $"{chunk.Id}.json");

        var json = JsonSerializer.Serialize(
            chunk,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(path, json); // writes chunka and embedding to file
    }

    // Retrieval/simliarity search step
    public async Task<IReadOnlyList<ChunkResult>> GetChunksAsync(
        string query,
        int topN = 5,
        Func<Dictionary<string, string>, bool>? metadataFilter = null)
    {
        var queryEmbedding =
            await _embeddingGenerator.GenerateEmbeddingAsync(query); // get embedding for query text

        var results = new List<ChunkResult>();

        /*
            Iterate over stored chunk files, load chunks back into memory, optional metadata filtering, 
            compute cosine similarity between query embedding and chunk embedding, collect results with scores.
            Return top-N most similar chunks.
        */

        foreach (var file in Directory.EnumerateFiles(_storageDirectory, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var chunk = JsonSerializer.Deserialize<StoredChunk>(json)!;

            if (metadataFilter != null &&
                !metadataFilter(chunk.Metadata))
                continue;

            var score = VectorMath.CosineSimilarity(
                queryEmbedding,
                chunk.Embedding);

            results.Add(new ChunkResult
            {
                ChunkId = chunk.Id,
                Text = chunk.Text,
                Metadata = chunk.Metadata,
                Score = score
            });
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .ToList();
    }
}

/*
    Cosine similarity function between two vectors.
*/
internal static class VectorMath
{
    public static double CosineSimilarity(List<double> a, List<double> b)
    {
        if (a.Count != b.Count)
            throw new InvalidOperationException("Embedding dimension mismatch");

        double dot = 0f;
        double magA = 0f;
        double magB = 0f;

        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / ((float)Math.Sqrt(magA) * (float)Math.Sqrt(magB));
    }
}

/*
    Interface for embedding generator service. Any class implementing this interface 
    must provide a method to generate embeddings for a given text.
    Makes it easy to swap out different embedding generation implementations.
*/
public interface IEmbeddingGenerator
{
    // generate embedding vector for the input text.
    Task<List<double>> GenerateEmbeddingAsync(string text);
}

// represents a chunk of text stores in the vector store
internal class StoredChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public List<double> Embedding { get; set; } = new List<double>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

// represents a retrieved chunk from similarity search
public class ChunkResult
{
    public string ChunkId { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public Dictionary<string, string> Metadata { get; init; } = new();
    public double Score { get; init; }
}

// Gemini embedding generator implementation of IEmbeddingGenerator
public class GeminiEmbeddingGenerator : IEmbeddingGenerator
{
    // Reference to LLMService to call Gemini embedding API
    LLMService _llmService;

    // constructor which takes LLMService as a parameter and assigns it to a private field
    public GeminiEmbeddingGenerator(LLMService llmService)
    {
        _llmService = llmService;
    }

    // generates embedding for a given text by calling GeminiAPI via LLMService
    public async Task<List<double>> GenerateEmbeddingAsync(string text)
    {
        var embeddings = await _llmService.GetEmbeddings(text);

        return embeddings;
    }
}