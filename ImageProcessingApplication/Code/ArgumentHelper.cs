using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

using ProcessorAlgorithm;

namespace ImageProcessingApplication.Code
{
    public static class AlgorithmDefinitionExtensions
    {
        public static bool NeedFileSave(this ArgumentDefinition[] objects)
        {
            return objects.Any(o => o.Type == ArgumentType.Image);
        }
    }
}