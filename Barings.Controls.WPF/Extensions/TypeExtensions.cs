using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Barings.Controls.WPF.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Indicates whether or not a particular <see cref="Type"/> is formattable or not
        /// </summary>
        public static bool IsFormattable(this Type type)
        {
            return type.IsNumeric() || type == typeof(DateTime) || type == typeof(DateTime?);
        }

        /// <summary>
        /// Indicates whether or not a particular <see cref="Type"/> is numeric or not
        /// </summary>
        public static bool IsNumeric(this Type type)
        {
            return NumericTypes.Contains(type) || NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        public static HashSet<Type> FormattableTypes()
        {
            var hash = NumericTypes;
            hash.Add(typeof(DateTime));
            return hash;
        }

        /// <summary>
        /// A collection of numeric <see cref="Type"/>s
        /// </summary>
        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(decimal),
            typeof(float),
            typeof(long)
        };

        public static object JsonCloneObject<T>(this T source, Type t)
        {
            var json = JsonConvert.SerializeObject(source);
            var a = GetEnumerableType(source.GetType());

            if (source is IEnumerable)
            {
                var s = source as IEnumerable;
                var iRet = Activator.CreateInstance(source.GetType()) as IList;
                if (iRet == null) return null;
                foreach (var item in s)
                {
                    var iJson = item.CloneJson(t);
                    iRet.Add(iJson);
                }
                return iRet;
            }
            var ret = JsonConvert.DeserializeObject(json, a);
            return ret;
        }

        public static object CloneJson(this object source, Type t)
        {
            return ReferenceEquals(source, null) ? null : JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source), t);
        }
        private static Type GetEnumerableType(Type type)
        {
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType
                    && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return null;
        }
        public static object StreamClone(this object source)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, source); //serialize to stream
                stream.Position = 0;
                return binaryFormatter.Deserialize(stream);
            }
        }
    }
}
