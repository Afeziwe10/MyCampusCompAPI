using Google.GenAI;
using Google.GenAI.Types;

namespace MyCampusComp.Services
{
    public class OpenAIService
    {
        private readonly Client _client;

        public OpenAIService(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(apiKey: apiKey);
        }

        public async Task<string> GetResponseAsync(string message)
        {
            string systemPrompt = """
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

                Only answer academic and campus-related questions that are relevant to students.

                Your name is Campus Assistant.
                """;

            string fullPrompt =
                systemPrompt +
                "\n\nStudent's question:\n" +
                message;

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.6-flash",
                contents: fullPrompt
            );

            return response.Candidates[0].Content.Parts[0].Text;
        }
    }
}