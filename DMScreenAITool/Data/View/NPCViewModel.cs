using DMScreenAITool.Enums;

namespace DMScreenAITool.Data.View
{
    public class NPCViewModel
    {
        public string Name { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Tone Tone { get; set; }
        public string Prompt { get; set; }
    }
}
