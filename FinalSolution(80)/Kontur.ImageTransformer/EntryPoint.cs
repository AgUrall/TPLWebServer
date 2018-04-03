using System;
using System.Collections.Generic;
using System.Drawing;
using Kontur.ImageTransformer.Transformations;

namespace Kontur.ImageTransformer
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            using (var server =
                new ImageHttpServer((uint) (4 * Environment.ProcessorCount), ProcessImage, new Size(1000, 1000)))
            {
                server.Start(80, "process");
                Console.ReadKey();
            }
        }

        private static Image ProcessImage(Image sourse, string filterName, Rectangle sourceArea)
        {
            try
            {
                var transformation = TransformationDispatcher.ChooseTransformation(filterName);
                var resImg = transformation.Apply(sourse, sourceArea);
                return resImg;
            }
            catch (ArgumentException exception)
            {
                throw new AggregateException(exception);
            }
            catch (KeyNotFoundException exception)
            {
                throw new ArgumentException("Illlegal filter name", nameof(filterName), exception);
            }
        }
    }
}