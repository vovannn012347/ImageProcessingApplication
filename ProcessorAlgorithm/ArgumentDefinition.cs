using System;

namespace ProcessorAlgorithm
{
    [Serializable]
    public enum ArgumentType
    {
        String,
        Image,
        Integer,
        Double
    }

    [Serializable]
    public class ArgumentDefinition
    {
        public ArgumentType Type { get; set; }

        public string Name { get; set; }
    }
}