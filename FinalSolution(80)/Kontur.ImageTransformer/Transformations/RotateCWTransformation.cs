using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer.Transformations
{
    [Transformation("rotate-cw")]
    public class RotateCwTransformation : Transformation
    {
        protected override Rectangle RotateArea(Rectangle sourceArea, Size imageSize)
        {
            var result = new Rectangle
            {
                X = sourceArea.Y,
                Y = imageSize.Height - sourceArea.X - sourceArea.Width,
                Height = sourceArea.Width,
                Width = sourceArea.Height
            };
            return result;
        }

        protected override Bitmap Execute(Bitmap bmp)
        {
            var width = bmp.Width;
            var height = bmp.Height;
            var newWidth = height;
            var newHeight = width;

            var newWidthMinusOne = height - 1;

            var resultBitmap = new Bitmap(newWidth, newHeight);

            var originalData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, height, width),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    var originalPointer = (int*) originalData.Scan0.ToPointer();
                    var rotatedPointer = (int*) resultData.Scan0.ToPointer();

                    var yOffset = -width;
                    for (var y = 0; y < height; ++y)
                    {
                        yOffset += width;
                        var destinationX = newWidthMinusOne - y;
                        var destinationY = -newWidth;
                        for (var x = 0; x < width; ++x)
                        {
                            var sourcePosition = x + yOffset;
                            destinationY += newWidth;
                            var destinationPosition =
                                destinationX + destinationY;
                            rotatedPointer[destinationPosition] =
                                originalPointer[sourcePosition];
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(originalData);
                resultBitmap.UnlockBits(resultData);
            }

            return resultBitmap;
        }
    }
}