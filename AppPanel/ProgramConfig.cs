using System.Collections.Generic;

namespace AppPanel
{
    public class ProgramConfig
    {
        public string? Path { get; set; }
        public List<string> Params { get; set; }
        public string? Icon { get; set; }
        public string? Name { get; set; }
        public bool Multi { get; set; }

        public ProgramConfig()
        {
            Params = new List<string>();
            Multi = false;
        }
    }
}