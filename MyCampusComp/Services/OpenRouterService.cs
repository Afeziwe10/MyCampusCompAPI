using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MyCampusComp.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenRouterService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetResponseAsync(string message)
        {
            // Get API key from configuration/environment variables
            var apiKey = _configuration["OpenRouter:ApiKey"];

            // Default model
            var model = _configuration["OpenRouter:Model"]
                        ?? "openai/gpt-4o";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenRouter API key is missing. " +
                    "Set the OPENROUTER_API_KEY environment variable."
                );
            }

            const string systemPrompt = """
                You are Campus Assistant, an AI assistant inside the MyCampusComp
                student mobile application.

                Your main purpose is to help college and university students.

                You can help students with:

                - Studying and understanding difficult topics
                - Explaining programming concepts
                - Assignments and academic guidance
                - Study planning
                - Time management
                - Campus-related questions
                - General student support
                - Explaining technical concepts in simple language

                Always communicate in a friendly, helpful and respectful way.

                Keep explanations clear and easy to understand.

                When explaining programming, provide simple examples and explain
                the code instead of only giving the answer.

                Do not pretend to know specific campus information unless it has
                been provided to you.

                If you do not know something, clearly say that you do not have
                that information.

                Do not make up campus announcements, timetables, deadlines,
                lecturer information or student information.

                Only answer academic and campus-related questions that are
                relevant to students.

                Your name is Campus Assistant.
                """;

            var requestBody = new
            {
                model = model,

                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },

                    new
                    {
                        role = "user",
                        content = message
                    }
                },

                max_tokens = 2000,
                temperature = 0.7
            };
            
            
            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions"
            );

            // OpenRouter authentication
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            // Optional OpenRouter metadata
            request.Headers.TryAddWithoutValidation(
                "HTTP-Referer",
                _configuration["OpenRouter:Referer"]
                ?? "https://mycampuscomp-api2026-bhcha3cwgtdmckbg.brazilsouth-01.azurewebsites.net"
            );

            request.Headers.TryAddWithoutValidation(
                "X-Title",
                "MyCampusComp"
            );

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenRouter returned {(int)response.StatusCode}: {responseBody}"
                );
            }

            using var document =
                JsonDocument.Parse(responseBody);

            var root = document.RootElement;

            var answer = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException(
                    "OpenRouter returned an empty response."
                );
            }

            return answer;
        }
    }
}