# DOTNET-RAG-Document-QA
**Author:** Krish Nangia

A .NET 8 console application implementing a RAG pipeline for document-based question answering.  
This code ingests plain text documents, generate embeddings for semantic search, and query an LLM to get answers grounded strictly in the provided documents.

---

## Overview

This application is designed to demonstrate a simple RAG workflow:

1. **Document Ingestion** – Read and chunk text documents.
2. **Vector Embedding** – Generate embeddings for each chunk using Gemini.
3. **Vector Store** – Store chunk embeddings with metadata for retrieval.
4. **Query Handling** – Accept user questions via the console.
5. **Similarity Search** – Retrieve the most relevant chunks using cosine similarity.
6. **LLM Answer Generation** – Send retrieved context + user question to LLM to produce a grounded answer.

---

## Step-by-Step Flow

### 1. Program Startup
- Loads `FileVectorStore` from the configured `VECTOR_STORE_LOCATION`.
- If the folder does not exist, initializes a new store.

### 2. **Document Ingestion**  
   - Scans `DOCUMENTS_FOLDER_PATH` for `.txt` files.  
   - Checks whether files were previously ingested by comparing existing vector store chunks.  
   - **Already ingested files are skipped** to avoid duplication.  
   - Currently, ingesting all documents in the `Stories` directory from scratch takes **around 6 minutes**.

### 3. Chunking Documents
- Reads each file’s content.
- Uses `SemanticSlicer` to split the document into smaller chunks (e.g., 50–100 words per chunk).

### 4. Embedding Generation
- For each chunk, calls `GeminiEmbeddingGenerator` to produce a vector embedding.
- Stores metadata for each chunk:
  - `SourceFile` – the original filename
  - `ChunkIndex` – position of the chunk within the file

### 5. Vector Store Update
- Adds each chunk and its embedding to `FileVectorStore`.
- Implements a delay (`delayMs`) between API calls to avoid rate limiting.

### 6. User Query Handling
- Waits for console input from the user.
- Accepts natural language questions.
- Type `exit` to quit.

### 7. Similarity Search
- Computes similarity (cosine similarity) between query embedding and stored chunk embeddings.
- Retrieves the top N most relevant chunks (default: 5).

### 8. Context Construction
- Combines the retrieved chunks into a single context string.
- Optionally includes metadata such as `SourceFile` and `ChunkIndex` for debugging.

### 9. LLM Query & Answer Generation
- Sends the context and question as a prompt to the LLM (`LLMService`).
- Receives an answer grounded strictly in the provided context.

### 10. Output Answer
- Prints the answer to the console.
- Loops back to accept more queries until the user exits.

---

## Features
- Incremental document ingestion.
- Chunk-based vector embeddings for precise semantic retrieval.
- Top-N similarity search for relevant context.
- LLM-based answer generation grounded in retrieved chunks.
- Simple console interface with safe exit handling.

---

## Approach & Assumptions
- Unique filenames are used for ingestion detection. If a file is modified but the name is unchanged, the system will not re-ingest it.
- The vector store preserves all previous embeddings; old chunks remain even if files are updated.
- Works only with plain text documents. PDFs or Word documents require preprocessing into `.txt`.
- Delays (`delayMs`) are used to avoid hitting rate limits.
- Metadata such as `SourceFile` and `ChunkIndex` is optional but useful for debugging and context reference.

---

## Setup & Installation

```bash
git clone https://github.com/knangia04/DOTNET-RAG-Document-QA.git
cd DOTNET-RAG-Document-QA
dotnet restore
dotnet build
```
- Place your documents in the `Documents` folder and set  `GlobalSettings.DOCUMENTS_FOLDER_PATH` in the `GlobalSettings.cs`.
- Set `GlobalSettings.VECTOR_STORE_LOCATION` for storing chunk embeddings.

---

## Usage

Run the application:

```bash
dotnet run
```
- The program will ingest new documents and generate embeddings.
- After ingestion, you can enter questions interactively.
- Type `exit` to quit the program safely.
