using System;
using System.Drawing;
using Kontur.ImageTransformer.ImageUtilitys;

namespace Kontur.ImageTransformer.Transformations
{
    public abstract class Transformation
    {
        protected abstract Bitmap Execute(Bitmap bitmap);

        protected abstract Rectangle RotateArea(Rectangle sourceArea, Size imageSize);

        public Image Apply(Image source, Rectangle sourceArea)
        {
            using (var bmp = new Bitmap(source))
            {
                var rotatedArea = RotateArea(sourceArea, source.Size);

                try
                {
                    using (var subBmp = bmp.GetSubimage(rotatedArea))
                    {
                        return Execute(subBmp);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }
    }
}