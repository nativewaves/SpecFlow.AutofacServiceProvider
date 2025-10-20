using System;
using System.Linq;

namespace NativeWaves.Reqnroll.AutofacServiceProvider
{
    public static class SystemTypeExtension
    {
        /// <summary> Prints a pretty name of a generic type. </summary>
        /// <param name="genericType">Type to name prettily.</param>
        /// <returns>A string containing all generic types in the name.</returns>
        public static string ToGenericTypeName(this Type genericType)
        {
            return $"{NameFromType(genericType)}";

            string NameFromType(Type type)
            {
                return type == null
                    ? string.Empty
                    : type.IsGenericType
                         ? $"{CleanGenericName(type.Name)}<{string.Join(",", type.GenericTypeArguments.Select(gt => NameFromType(gt)))}>"
                         : CleanGenericName(type.Name);
            }

            string CleanGenericName(string name)
            {
                var readTo = name.IndexOf('`');
                if (readTo < 0) { readTo = name.Length; }
                return name.Substring(0, readTo);
            }
        }
    }
}
