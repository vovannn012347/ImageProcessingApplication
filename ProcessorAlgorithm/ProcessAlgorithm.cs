using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ProcessorAlgorithm
{
    [Serializable]
    public abstract class ProcessAlgorithm : MarshalByRefObject
    {
        public abstract Task<Argument[]> Process(string tempStoragePath, Argument[] args);
        public virtual ArgumentDefinition[] GetInputArguments()
        {
            return new ArgumentDefinition[] { };
        }

        public virtual ArgumentDefinition[] GetOutputArguments()
        {
            return new ArgumentDefinition[] { };
        }

        public string GetMetadataFile()
        {
            var input = GetInputArguments();
            var output = GetOutputArguments();

            return  JsonConvert.SerializeObject(new AlgorithmMetadata
            {
                Input = input,
                Output = output
            });
        }
    }
}
