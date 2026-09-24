using DMScreenAITool.Enums;
using System.Text.Json.Serialization;

namespace DMScreenAITool.Data.Model
{
    public class NPC
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Race Race { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Class Class { get; set; }
        public string Description { get; set; }
        public string Personality { get; set; }
        public string Secret { get; set; }
        public string CombatStyle { get; set; }
        public string PlotHook { get; set; }
        public Town Town { get; set; }

    }
}
