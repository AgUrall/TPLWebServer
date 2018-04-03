using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kontur.ImageTransformer.Transformations
{
    public static class TransformationDispatcher
    {
        private static readonly Dictionary<string, Transformation> TransformationTypes;

        static TransformationDispatcher()
        {
            TransformationTypes = typeof(Transformation).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Transformation)))
                .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0))
                .Select(t => new
                {
                    Item1 = t.GetConstructor(new Type[0]),
                    Item2 = (TransformationAttribute) t.GetCustomAttribute(typeof(TransformationAttribute), true)
                })
                .Where(p => p.Item2 != null)
                .ToDictionary(p => p.Item2.Name, p => (Transformation) p.Item1.Invoke(new object[0]));
        }

        public static Transformation ChooseTransformation(string transformationName)
        {
            return TransformationTypes[transformationName.ToLower()];
        }
    }
}