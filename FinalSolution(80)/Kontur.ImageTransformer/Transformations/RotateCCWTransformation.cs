using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer.Transformations
{
    [Transformation("rotate-ccw")]
    public class RotateCcwTransformation : Transformation
    {
        protected override Rectangle RotateArea(Rectangle sourceArea, Size imageSize)
        {
            var result = new Rectangle
            {
                X = imageSize.Width - sourceArea.Y - sourceArea.Height,
                Y = sourceArea.X,
                Height = sourceArea.Width,
                Width = sourceArea.Height
            };
            return result;
        }

        protected override Bitmap Execute(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var newWidth = height;
            var newHeight = width;

            var resultBitmap = new Bitmap(height, width);

            var originalData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, height, width),
                ImageLockMode.ReadWrite,
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
                        var destinationX = y;
                        var destinationY = newWidth * newHeight;
                        for (var x = 0; x < width; ++x)
                        {
                            var sourcePosition = x + yOffset;
                            destinationY -= newWidth;
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
                bitmap.UnlockBits(originalData);
                resultBitmap.UnlockBits(resultData);
            }

            return resultBitmap;
        }
    }
}