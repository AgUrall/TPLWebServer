using System;
using System.Drawing;

namespace Kontur.ImageTransformer.ImageUtilitys
{
    internal static class ImageUtilitys
    {
        private static Rectangle ReCalculateCoords(Rectangle area,
            Size imgSize)
        {
            var start = new Point(area.X, area.Y);
            var end = new Point(area.X + area.Width,
                area.Y + area.Height);
            if (end.X < start.X)
            {
                var tmp = end.X;
                end.X = start.X;
                start.X = tmp;
            }

            if (end.Y < start.Y)
            {
                var tmp = end.Y;
                end.Y = start.Y;
                start.Y = tmp;
            }

            area = Rectangle.FromLTRB(start.X, start.Y, end.X, end.Y);
            var result = Rectangle.Intersect(area, new Rectangle(new Point(0, 0), imgSize));
            return result;
        }

        public static Bitmap GetSubimage(this Bitmap sourceBmp, Rectangle area)
        {
            var intersection = ReCalculateCoords(area, sourceBmp.Size);

            if (intersection.Width == 0 || intersection.Height == 0)
                throw new ArgumentOutOfRangeException(nameof(intersection), intersection, "Zero width (height)");

            var format = sourceBmp.PixelFormat;
            var resultBmp = sourceBmp.Clone(intersection, format);
            return resultBmp;
        }
    }
}