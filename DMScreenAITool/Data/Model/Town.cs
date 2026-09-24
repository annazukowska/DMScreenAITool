using DMScreenAITool.Enums;
using System.Text.Json.Serialization;

namespace DMScreenAITool.Data.Model
{
    public class Town
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Biome Biome { get; set; }
        public string TownSize { get; set; }
        public string? CountryName { get; set; }
        public string PlotHook { get; set; }
        public string Secret { get; set; }
        public string? AdditionalDescription { get; set; }
        public List<NotableLocation> NotableLocations { get; set; }
    }
}
