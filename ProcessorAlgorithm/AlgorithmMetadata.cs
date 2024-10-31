using System;

namespace ProcessorAlgorithm
{

    [Serializable]
    public class AlgorithmMetadata
    {
        public ArgumentDefinition[] Input { get; set; }

        public ArgumentDefinition[] Output { get; set; }
    }
}