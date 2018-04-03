using System;

namespace Kontur.ImageTransformer.Transformations
{
    public class TransformationAttribute : Attribute
    {
        public readonly string Name;

        public TransformationAttribute(string name)
        {
            Name = name;
        }
    }
}