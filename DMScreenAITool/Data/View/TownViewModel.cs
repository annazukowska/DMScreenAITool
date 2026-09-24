using DMScreenAITool.Data.Model;
using DMScreenAITool.Enums;

namespace DMScreenAITool.Data.View
{
    public class TownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Biome Biome { get; set; }
        public string? CountryName { get; set; }
        public string TownSize { get; set; }
        public int LociationsNumber { get; set; }
        public bool ShoulGenerateQuests { get; set; }
        public bool ShoulGenerateNPCs { get; set; }
        public string Prompt { get; set; }
    }
}
