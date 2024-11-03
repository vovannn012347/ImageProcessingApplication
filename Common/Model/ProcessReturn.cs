using System;

namespace Common.Model
{
    public class ProcessReturn
    {
        public string ProcessId { get; set; }
        public string CodeName { get; set; }
        public string FriendlyName { get; set; }

        public ArgumentDefinition[] InputParams { get; set; }
        public ArgumentDefinition[] OutputParams { get; set; }
    }
}