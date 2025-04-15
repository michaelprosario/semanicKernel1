
namespace AppInfra
{
    public class ContentFragmentMaker
    {
        // https://www.restack.io/p/text-chunking-answer-strategies-in-csharp-cat-ai
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

        // Write method to remove all non-alphanumeric characters from a string
        public string RemoveNonAlphanumeric(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
        }

        // Write method to remove new lines from string
        public string RemoveNewLines(string input)
        {
            return input.Replace("\n", " ").Replace("\r", " ");
        }        

        public List<ContentItemFragment> ProcessFile(string filePath)
        {
            Console.WriteLine($"--- File: {Path.GetFileName(filePath)} ---");

            Guid contentItemId = Guid.NewGuid();

            var response = new List<ContentItemFragment>();

            try
            {
                string content = File.ReadAllText(filePath);

                content = RemoveNonAlphanumeric(content);
                content = RemoveNewLines(content);

                // Get the file name from the file path
                string fileName = Path.GetFileName(filePath);

                List<string> chunks = GetChunks(content, 1000, 200);


                // loop over fragments
                foreach (var chunk in chunks)
                {
                    // fill out the fragment
                    var fragment = new ContentItemFragment
                    {
                        Id = Guid.NewGuid(),
                        ContentItemId = contentItemId,
                        Content = chunk,
                        Source = fileName,
                        Embedding = null // Placeholder for embedding, if needed
                    };
                    response.Add(fragment);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }

            Console.WriteLine($"--- End of {Path.GetFileName(filePath)} ---\n");

            return response;
        }

    }

}