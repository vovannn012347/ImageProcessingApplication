using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProcessorAlgorithm;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;

namespace ImageProcessingSobel2
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
            var resultpath = Path.Combine(tempStoragePath, Guid.NewGuid().ToString() + Path.GetExtension(input_image_path.Value));

            using (Image<Rgba32> image = Image.Load<Rgba32>(inputImagePath))
            {
                // Define the Sobel kernel for the X direction
                float[,] sobelX = new float[,]
                {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
                };

                // Define the Sobel kernel for the Y direction
                float[,] sobelY = new float[,]
                {
                { 1, 2, 1 },
                { 0, 0, 0 },
                { -1, -2, -1 }
                };

                // Create a new image to hold the result
                using (Image<Rgba32> sobelImageX = new Image<Rgba32>(image.Width, image.Height))
                using (Image<Rgba32> sobelImageY = new Image<Rgba32>(image.Width, image.Height))
                using (Image<Rgba32> finalImage = new Image<Rgba32>(image.Width, image.Height))
                {
                    // Apply the Sobel filter in the X direction
                    sobelImageX.Mutate(x => x.Convolution(sobelX));

                    // Apply the Sobel filter in the Y direction
                    sobelImageY.Mutate(x => x.Convolution(sobelY));

                    // Combine the X and Y results
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            // Get the pixel values from the Sobel X and Y images
                            var pixelX = sobelImageX[x, y];
                            var pixelY = sobelImageY[x, y];

                            // Calculate the magnitude of the gradient
                            byte magnitude = (byte)Math.Min(Math.Sqrt(pixelX.R * pixelX.R + pixelY.R * pixelY.R), 255);

                            // Set the pixel in the final image
                            finalImage[x, y] = new Rgba32(magnitude, magnitude, magnitude);
                        }
                    }

                    // Save the result
                    finalImage.Save(outputImagePath);
                }
            }

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
