using System;
using System.Threading.Tasks;
using static VoiceToText_Repo.Ulity.ChatGptAPI;

class Program
{
    static async Task Main(string[] args)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string apiKey = "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";

        bool continueChatting = true;

        while (continueChatting)
        {
            Console.WriteLine("Enter your prompt:");
            string userPrompt = Console.ReadLine();

            ChatGPTCall gptCall = new ChatGPTCall(apiUrl, apiKey);
            string response = await gptCall.GenerateResponse(userPrompt);

            Console.WriteLine("AI Response: " + response);

            Console.WriteLine("Do you want to continue chatting? (y/n)");
            string input = Console.ReadLine();

            continueChatting = input.ToLower() == "y";
        }
    }
}
