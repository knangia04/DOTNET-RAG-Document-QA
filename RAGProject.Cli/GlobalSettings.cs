using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGProject.Cli
{
    internal class GlobalSettings
    {
        //change the Datasource to the full path of file in the Stories folder (entire story is in Misc directory)
        public const string ShakespeareWorks_DataSourceLocation = "";
        //change the Datasource to the full path of file in the Stories folder 
        public const string AesopFables_DataSourceLocation = "";
        //change the API key to your own key
        public const string API_KEY = "";
        //change the vector store location to your desired location where you want to store the files with chunk and vector embeddings
        public const string VECTOR_STORE_LOCATION = "";
        // documents folder path
        public const string DOCUMENTS_FOLDER_PATH = "";
        //Keep these the same
        public const string LLM_MODEL = "gemini-2.5-flash";
        public const string EMBED_MODEL = "gemini-embedding-001";
    }
}
