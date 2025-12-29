# .NET-RAG-Document-QA
A .NET 8 console application implementing a Retrieval-Augmented Generation (RAG) pipeline for document-based question answering. Users can ingest text documents, create embeddings, store them in a vector database, and query an LLM (Google Gemini) for answers grounded in the ingested content.

Table of Contents

Overview

System Flow

Step-by-Step Flow

Features

Assumptions

Prerequisites

Setup & Installation

Usage

Code Structure

Testing & Sample Queries

Limitations

Notes

Overview

This project demonstrates a .NET RAG system that:

Ingests .txt documents.

Chunks the documents into smaller pieces for embedding.

Generates embeddings via an LLM embedding service (Google Gemini).

Stores embeddings in a local JSON-based vector store (FileVectorStore).

Performs similarity search to find the most relevant chunks for a user query.

Sends relevant context to an LLM to generate answers.

System Flow

Here’s a high-level flow of the system:

+-------------------+
|  Start Program    |
+-------------------+
          |
          v
+-------------------+
| Load Vector Store |<----------------+
+-------------------+                 |
          |                            |
          v                            |
+-------------------+                  |
|  Load Documents   |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
| Check Already     |                  |
| Ingested Files    |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
|  Slice Documents  |                  |
|  into Chunks      |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
| Generate Embedding|                  |
| for Each Chunk    |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
|  Save Chunks to   |                  |
|  Vector Store     |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
| User Input Query  |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
| Search Vector     |                  |
| Store (Top N)     |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
|  Build Context    |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
|  Send to LLM      |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
|  Display Answer   |                  |
+-------------------+                  |
          |                            |
          v                            |
+-------------------+                  |
| Repeat Until Exit |------------------+
+-------------------+

Step-by-Step Flow

Program Startup

Loads FileVectorStore from the configured VECTOR_STORE_LOCATION.

Checks if the folder exists; otherwise initializes a new store.

Document Ingestion

Scans the DOCUMENTS_FOLDER_PATH for .txt files.

Checks if files were previously ingested by comparing existing vector store chunks.

Skips already ingested files to avoid duplication.

Chunking Documents

Reads file content.

Uses Slicer to break the document into smaller chunks (e.g., 50–100 words per chunk).

This ensures manageable embedding sizes for the LLM.

Embedding Generation

For each chunk, calls GeminiEmbeddingGenerator to generate a vector embedding.

Stores metadata for each chunk:

SourceFile

ChunkIndex

Vector Store Update

Adds each chunk and its embedding to FileVectorStore.

Delays between API calls (delayMs) to avoid rate limits.

User Query Handling

Waits for console input from the user.

Accepts natural language questions.

Type exit to quit.

Similarity Search

Computes similarity (cosine similarity) between query embedding and stored chunk embeddings.

Retrieves top N most relevant chunks.

Context Construction

Combines the top chunks into a single context string.

Can optionally include metadata like SourceFile and ChunkIndex for debugging.

LLM Query & Answer Generation

Sends the context + question as a prompt to the LLM (LLMService).

Receives an answer strictly based on the provided context.

Output Answer

Prints answer to console.

Loops back to accept more queries until user exits.

Features

Incremental document ingestion.

Chunk-based vector embeddings for improved semantic retrieval.

Top-N similarity search for precise context retrieval.

LLM-based answer generation grounded in retrieved chunks.

Console interface with simple exit handling.

Assumptions

Unique filenames are required for ingestion detection.

Updating a file will not trigger re-ingestion unless the filename changes.

Works only for plain text documents. PDFs or Word documents need preprocessing.

Free-tier Gemini API usage is limited (~20 requests/day).

SourceFile metadata is optional in output but useful for debugging.

Prerequisites

.NET 8 SDK

Google Cloud project with Gemini API enabled

Valid API key for Gemini

Console environment or IDE (Visual Studio, VS Code)

Setup & Installation
git clone https://github.com/your-username/DOTNET-RAG-Document-QA.git
cd DOTNET-RAG-Document-QA
dotnet restore
dotnet build


Place documents in the Documents folder (or configure GlobalSettings.DOCUMENTS_FOLDER_PATH).

Set GlobalSettings.VECTOR_STORE_LOCATION for chunk storage.

Usage
dotnet run


The program will ingest new documents.

After ingestion, you can enter questions.

Type exit to quit.

Example Queries

"What lesson does The Tortoise and the Hare teach?"

"Who helps the Lion in The Lion and the Mouse?"

"Summarize Sonnet 1."

"What is the main conflict in Antony and Cleopatra?"

"eixt" → handled safely (returns no answer).

Limitations

Free-tier API quotas may restrict LLM queries.

Updating a file does not refresh embeddings automatically.

Only text documents are supported.
