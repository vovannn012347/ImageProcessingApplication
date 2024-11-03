using System;

namespace Common.Model
{
    public enum ArgumentType
    {
        String,
        Image,
        Integer,
        Double
    }

    public class ArgumentDefinition
    {
        public ArgumentType Type { get; set; }

        public string Name { get; set; }
    }
}