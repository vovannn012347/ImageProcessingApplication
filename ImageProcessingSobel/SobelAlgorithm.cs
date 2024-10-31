using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProcessorAlgorithm;

using TorchSharp;

using static TorchSharp.torch;
using static TorchSharp.torchvision;
using static TorchSharp.torchvision.io;

namespace ImageProcessingSobel
{
    public class SobelAlgorithm : ProcessAlgorithm
    {
        static Imager imageLoader = new SkiaImager();

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

            var device = DeviceType.CPU;
            TorchSharp.torch.InitializeDeviceType(TorchSharp.DeviceType.CPU);

            var image = LoadImageAsTensor(input_image_path.Value).to(device);

            var sobelX = torch.tensor(new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }).reshape(1, 1, 3, 3).to(device);
            var sobelY = torch.tensor(new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }).reshape(1, 1, 3, 3).to(device);

            var edgeX = nn.functional.conv2d(image.unsqueeze(0), sobelX);
            var edgeY = nn.functional.conv2d(image.unsqueeze(0), sobelY);

            var edges = torch.sqrt(edgeX.pow(2) + edgeY.pow(2)).squeeze();

            var resultpath = Path.Combine(tempStoragePath, Guid.NewGuid().ToString() + Path.GetExtension(input_image_path.Value));
            // Display or save the resulting edge-detected image
            DisplayOrSaveTensorAsImage(edges, resultpath);

            return new Argument[]
            {
                new Argument
                {
                    Name = "output_img",
                    Value = Path.GetFileName(resultpath)
                }
            };
        }

        static Tensor LoadImageAsTensor(string filePath)
        {
            // Load an image and convert it to grayscale tensor (values normalized between 0 and 1)
            using (var img = torchvision.io.read_image(filePath, ImageReadMode.GRAY, imageLoader))
            {
                return img.to(torch.float32).div(255);
            }
        }

        static void DisplayOrSaveTensorAsImage(Tensor tensor, string filePath)
        {
            // Save tensor as image for visualization (convert back to byte range 0-255)
            var img = tensor.mul(255).clamp(0, 255).to(torch.uint8);
            torchvision.io.write_png(img, filePath, imageLoader);
        }
    }
}
