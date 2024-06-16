using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        DotNetEnv.Env.Load();

        // Read the API key from environment variables
        string apiKey = Environment.GetEnvironmentVariable("ApiKey");

        // Check if the API key is loaded
        Console.WriteLine(apiKey);

        string apiUrl = "https://api.openai.com/v1/chat/completions";

        Console.ReadKey();
        // Read the file path from the user
        Console.WriteLine("Enter the path to the .txt file:");
        string filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return;
        }

        // Read the contents of the file
        string fileContents = await File.ReadAllTextAsync(filePath);

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        // Send the file contents to OpenAI and get the summary
        string summary = await GetSummary(fileContents, client, apiUrl);

        // Display the summary
        Console.WriteLine("Summary:");
        Console.WriteLine(summary);
    }

    static async Task<string> GetSummary(string content, HttpClient client, string apiUrl)
    {
        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = "Please summarize the following text:\n\n" + content }
            }
        };

        var jsonRequest = System.Text.Json.JsonSerializer.Serialize(request);
        var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseContent);

            // Extracting the summary from the JSON response
            string summary = jsonResponse["choices"][0]["message"]["content"].ToString();
            return summary;
        }
        else
        {
            Console.WriteLine($"Error performing request. Status code: {response.StatusCode}");
            return null;
        }
    }
}
