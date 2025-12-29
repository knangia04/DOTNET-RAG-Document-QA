using Google.GenAI;
using Google.GenAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGProject.Cli
{
    // LLMService is basically a wrapper around the Gemini API client.
    // Centralizes all LLM-related calls in one place. That is, generation + embeddings.
    // This is so the rest of the app does not talk directly to Gemini. 
    public class LLMService
    {
        // Async method to send prompt to Gemini and return generated response text    
        public async Task<string> GetResponse(string request)
        {
            var client = new Client(apiKey: GlobalSettings.API_KEY); // create Gemini client with API key from GlobalSettings
            var response = await client.Models.GenerateContentAsync(
              model: GlobalSettings.LLM_MODEL, contents: request
            ); // call Gemini's text generation endpoint with model and request
            var resultText = response.Candidates?[0]?.Content?.Parts?[0]?.Text ?? ""; // extract first text response
            return resultText;
        }

        // Async method to convert text into numerical embeddings using Gemini API
        public async Task<List<double>> GetEmbeddings(string text)
        {
            var client = new Client(apiKey: GlobalSettings.API_KEY); // create Gemini client with API key from GlobalSettings
            var response = await client.Models.EmbedContentAsync(
              model: GlobalSettings.EMBED_MODEL, contents: text); // call Gemini's embedding endpoint with model and text
            var result = response.Embeddings?.FirstOrDefault(); // extract first embedding result
            return result.Values;
        }

    }
}
