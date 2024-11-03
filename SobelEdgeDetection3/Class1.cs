using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Accord.Imaging.Filters;

using ProcessorAlgorithm;
using System.Drawing;
using Common.Model;

namespace ImageProcessingSobel3
{
    public class SobelAlgorithm : ProcessAlgorithm
    {

        public override ArgumentDefinition[] GetInputArguments()
        {
            return new ArgumentDefinition[] {
                new ArgumentDefinition{
                    Name = "input_img",
                    Type = ArgumentType.Image
                }
            };
        }

        public override ArgumentDefinition[] GetOutputArguments()
        {
            return new ArgumentDefinition[] {
                new ArgumentDefinition{
                    Name = "output_img",
                    Type = ArgumentType.Image
                }
            };
        }

        public override async Task<Argument[]> Process(string tempStoragePath, Argument[] args)
        {
            var input_image_path = args.FirstOrDefault(a => a.Name == "input_img");
            if (input_image_path == null) throw new ArgumentException("argument not found");

            var outputImagePath = Path.Combine(tempStoragePath, Guid.NewGuid().ToString() + Path.GetExtension(input_image_path.Value));
            
            // Load the image
            Bitmap image = (Bitmap)Bitmap.FromFile(input_image_path.Value);

            var grayscaleFilter = new Grayscale(0.299, 0.587, 0.114);
            Bitmap grayscaleImage = grayscaleFilter.Apply(image);
            // Create Sobel filter
            var sobel = new SobelEdgeDetector();

            // Apply filter
            Bitmap result = sobel.Apply(grayscaleImage);

            // Save the result
            result.Save(outputImagePath);

            return new Argument[]
            {
                new Argument
                {
                    Name = "output_img",
                    Value = Path.GetFileName(outputImagePath)
                }
            };
        }
    }
}
