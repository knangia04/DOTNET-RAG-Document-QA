using SemanticSlicer;
using SemanticSlicer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGProject.Cli
{
    public class Samples
    {
        public static async Task TestGenerateContent(LLMService llmService)
        {
            var gen_result = await llmService.GetResponse("Write a poem about AI in the style of Shakespeare");
            Console.WriteLine("Response:" + Environment.NewLine + gen_result);
        }

        public static async Task TestEmbedding(LLMService llmService)
        {
            var embed_result = await llmService.GetEmbeddings("Hello, world!");

            Console.WriteLine("Embeeding Length:" + embed_result.Count);
        }

        public static async Task TestVectorStore(LLMService llmService)
        {
            var vectorStore = new FileVectorStore(new GeminiEmbeddingGenerator(llmService), GlobalSettings.VECTOR_STORE_LOCATION);

            await vectorStore.AddChunkAsync(chunkText: "This is a sample chunk of text to be embedded and stored.",
                metadata: new Dictionary<string, string>
                {
            { "Source", "SampleDocument.txt" },
            { "ChunkIndex", "1" }
                });

            var matchResult = await vectorStore.GetChunksAsync(query: "sample text", topN: 5);
        }

        public static List<DocumentChunk> ChunkDocument(string dataSourceLocation)
        {
            string text = File.ReadAllText(dataSourceLocation);
            var slicer = new Slicer();
            var chunks = slicer.GetDocumentChunks(text);
            Console.WriteLine($"Total Chunks Created: {chunks.Count}");
            return chunks;
        }

    }
}
