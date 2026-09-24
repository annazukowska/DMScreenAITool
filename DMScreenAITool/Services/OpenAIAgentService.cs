using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DMScreenAITool.Data.Model;
using DMScreenAITool.Data.View;
using DMScreenAITool.Enums;
using OpenAI.Chat;

namespace DMScreenAITool.Services
{
    public class OpenAIAgentService
    {
        private readonly ChatClient _chatClient;

        public OpenAIAgentService(string model, string apiKey)
        {
            _chatClient = new ChatClient(model, apiKey);
        }

        public async Task<NPC> GenerateNPCAsync(NPCViewModel npcView, TypeOfPrompt typeOfPrompt)
        {
            var options = PrepateOptions(typeOfPrompt);
            var message = PrepareNPCMessage(npcView);

            var response = await _chatClient.CompleteChatAsync(message, options);
            var npcResponse = System.Text.Json.JsonSerializer.Deserialize<NPC>(response.Value.Content[0].Text,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                });

            return npcResponse;
        }

        public async Task<Town> GenerateTownAsync(TownViewModel townView, TypeOfPrompt typeOfPrompt)
        {
            var options = PrepateOptions(typeOfPrompt);
            var message = PrepareTownMessage(townView);

            var response = await _chatClient.CompleteChatAsync(message, options);
            var townResponse = System.Text.Json.JsonSerializer.Deserialize<Town>(response.Value.Content[0].Text,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true
                });

            return townResponse;
        }

        public async Task<string> GenerateNPCNameAsync(Race race, Class npcClass, string tone = "Fantasy")
        {
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 50
            };

            var systemPrompt = $"""
                You are a fantasy name generator. Generate authentic D&D character names.
                Respond with ONLY the name, nothing else.
                """;

            var userPrompt = $"""
                Generate a {tone.ToLower()} name for a {race} {npcClass}.
                Make it sound authentic to D&D fantasy setting.
                Include both first and last name.
                """;

            var message = new ChatMessage[]
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage(userPrompt)
            };

            var response = await _chatClient.CompleteChatAsync(message, options);
            return response.Value.Content[0].Text.Trim();
        }

        public async Task<string> GenerateTownNameAsync(Biome biome, string townSize)
        {
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 30 // Very short for just a name
            };

            var systemPrompt = $"""
                You are a fantasy settlement name generator. Generate authentic D&D town names.
                Respond with ONLY the town name, nothing else.
                """;

            var userPrompt = $"""
                Generate a fantasy name for a {townSize.ToLower()} in a {biome.ToString().ToLower()} biome.
                Make it sound like a real D&D settlement.
                Single name only, no description.
                """;

            var message = new ChatMessage[]
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage(userPrompt)
            };

            var response = await _chatClient.CompleteChatAsync(message, options);
            return response.Value.Content[0].Text.Trim();
        }

        private ChatCompletionOptions PrepateOptions(TypeOfPrompt typeOfPrompt)
        {
            var options = new ChatCompletionOptions();

            switch (typeOfPrompt)
            {
                case TypeOfPrompt.NPCGenerator:
                    var raceValues = Enum.GetNames(typeof(Race));
                    var classValues = Enum.GetNames(typeof(Class));

                    options.MaxOutputTokenCount = Configuration.OpenAIAgentSettings.MIN_OUTPUT_TOKEN_COUNT;
                    var npcSchema = $@"{{
                      ""type"": ""object"",
                      ""properties"": {{
                        ""name"": {{ ""type"": ""string"" }},
                        ""race"": {{ ""type"": ""string"", ""enum"": [{string.Join(",", raceValues.Select(r => $"\"{r}\""))}] }},
                        ""npcClass"": {{ ""type"": ""string"", ""enum"": [{string.Join(",", classValues.Select(c => $"\"{c}\""))}] }},
                        ""description"": {{ ""type"": ""string"" }},
                        ""personality"": {{ ""type"": ""string"" }},
                        ""secret"": {{ ""type"": ""string"" }},
                        ""combatStyle"": {{ ""type"": ""string"" }},
                        ""plotHook"": {{ ""type"": ""string"" }}
                      }},
                      ""required"": [""name"",""race"",""npcClass"",""description"",""personality"",""secret"",""combatStyle"",""plotHook""],
                      ""additionalProperties"": false
                    }}";
                    options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "npc",
                        jsonSchema: BinaryData.FromString(npcSchema),
                        jsonSchemaIsStrict: true);
                    break;

                case TypeOfPrompt.TownGenerator:
                    var biomeValues = Enum.GetNames(typeof(Biome));

                    options.MaxOutputTokenCount = Configuration.OpenAIAgentSettings.MAX_OUTPUT_TOKEN_COUNT;
                    var townSchema = $@"{{
                      ""type"": ""object"",
                      ""properties"": {{
                        ""name"": {{ ""type"": ""string"" }},
                        ""biome"": {{ ""type"": ""string"", ""enum"": [{string.Join(",", biomeValues.Select(b => $"\"{b}\""))}] }},
                        ""townSize"": {{ ""type"": ""string"" }},
                        ""plotHook"": {{ ""type"": ""string"" }},
                        ""secret"": {{ ""type"": ""string"" }},
                        ""additionalDescription"": {{ ""type"": ""string"" }},
                        ""notableLocations"": {{
                          ""type"": ""array"",
                          ""items"": {{
                            ""type"": ""object"",
                            ""properties"": {{
                              ""name"": {{ ""type"": ""string"" }},
                              ""description"": {{ ""type"": ""string"" }}
                            }},
                            ""required"": [""name"", ""description""],
                            ""additionalProperties"": false
                          }}
                        }}
                      }},
                      ""required"": [""name"", ""biome"", ""townSize"", ""plotHook"", ""secret"", ""additionalDescription"", ""notableLocations""],
                      ""additionalProperties"": false
                    }}";
                    options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "town",
                        jsonSchema: BinaryData.FromString(townSchema),
                        jsonSchemaIsStrict: true);
                    break;

                default:
                    break;
            }

            return options;
        }

        private ChatMessage[] PrepareNPCMessage(NPCViewModel npcView)
        {
            var systemprompt = $"""
                You are an expert AI Dungeon Master assistant specializing in creating memorable D&D NPCs.
    
                Generate rich, three-dimensional characters that feel authentic to a fantasy medieval setting.
                Use {npcView.Tone} tone throughout all content.
    
                Guidelines:
                - Create NPCs with clear motivations, flaws, and personality quirks
                - Ensure descriptions are vivid but concise (2-4 sentences each)
                - Make secrets meaningful and plot-relevant
                - Design combat styles that match the character's class, personality and D&D playstyle
                - Create plot hooks that naturally involve the party
                - Use appropriate fantasy names and terminology
                - Consider how race and class influence personality and background
    
                Always fill all required JSON fields with engaging, usable content.
                """;

            var userPrompt = $"""
                Create a D&D NPC.

                Name: {npcView.Name}
                Race: {npcView.Race}
                Class: {npcView.Class}
                Additional info from DM: {npcView.Prompt}

                Include:
                - Personal info
                - Short description
                - Personality
                - Secret
                - Combat style
                - Plot hook
                """;

            var message = new ChatMessage[]
            {
                ChatMessage.CreateSystemMessage(systemprompt),
                ChatMessage.CreateUserMessage(userPrompt)
            };

            return message;
        }

        private ChatMessage[] PrepareTownMessage(TownViewModel townView)
        {
            var systemprompt = $"""
                You are an AI Dungeon Master assistant specialized in creating immersive D&D settlements.
                You generate detailed, atmospheric towns with rich lore and interesting locations.
                Create content that feels authentic to a fantasy medieval setting.
                Always fill all required JSON fields with creative, engaging content.
                """;
            var userPrompt = "";

            var questInfo = townView.ShoulGenerateQuests ? "Include quest hooks for each location." : "";
            var npcInfo = townView.ShoulGenerateNPCs ? "Include notable NPCs for each location." : "";

            userPrompt = $"""
                Create a D&D town/settlement with the following specifications:

                Name: {(string.IsNullOrEmpty(townView.Name) ? "Generate an appropriate name" : townView.Name)}
                Biome: {townView.Biome}
                Size: {townView.TownSize}
                Country/Region: {(string.IsNullOrEmpty(townView.CountryName) ? "Not specified" : townView.CountryName)}
                Number of Notable Locations: {townView.LociationsNumber}
                Additional Description: {townView.Prompt}

                {questInfo}
                {npcInfo}

                Requirements:
                        
                - Create exactly {townView.LociationsNumber} notable locations
                - Each location should have a unique name and detailed description
                - Include a compelling town description that fits the biome and size
                - Provide an interesting secret about the town
                - Create a plot hook that could drive adventures
                - Make the town feel alive and authentic to D&D fantasy setting
                - Consider the town size when determining the scope and importance of locations

                Notable Locations should include a mix of:
                - Taverns/Inns
                - Shops/Markets
                - Religious sites
                - Government buildings
                - Unique landmarks
                - Places of interest for adventurers
                """;

            var message = new ChatMessage[]
            {
                ChatMessage.CreateSystemMessage(systemprompt),
                ChatMessage.CreateUserMessage(userPrompt)
            };

            return message;
        }
    }
}