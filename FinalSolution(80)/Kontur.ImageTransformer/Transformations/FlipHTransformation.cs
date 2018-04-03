using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer.Transformations
{
    [Transformation("flip-h")]
    public class FlipHTransformation : Transformation
    {
        protected override Rectangle RotateArea(Rectangle sourceArea, Size imageSize)
        {
            var result = new Rectangle
            {
                X = imageSize.Width - sourceArea.X - sourceArea.Width,
                Y = sourceArea.Y,
                Height = sourceArea.Height,
                Width = sourceArea.Width
            };
            return result;
        }

        protected override Bitmap Execute(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            var resultBitmap = new Bitmap(width, height);

            var widthMinusOne = width - 1;

            var originalData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
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
                        var destinationY = yOffset;
                        for (var x = 0; x < width; ++x)
                        {
                            var destinationX = widthMinusOne - x;
                            var sourcePosition = x + yOffset;
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