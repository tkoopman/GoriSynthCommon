using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using DynamicData;

using Newtonsoft.Json.Linq;

namespace Common
{
    public static partial class Extensions
    {
        private static readonly Dictionary<Type, string> Aliases = new()
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(void), "void" },
            { typeof(byte?), "byte?" },
            { typeof(sbyte?), "sbyte?" },
            { typeof(short?), "short?" },
            { typeof(ushort?), "ushort?" },
            { typeof(int?), "int?" },
            { typeof(uint?), "uint?" },
            { typeof(long?), "long?" },
            { typeof(ulong?), "ulong?" },
            { typeof(float?), "float?" },
            { typeof(double?), "double?" },
            { typeof(decimal?), "decimal?" },
            { typeof(bool?), "bool?" },
            { typeof(char?), "char?" }
        };

        /// <summary>
        ///     Adds dictionary to hashCode
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="hashCode"></param>
        /// <param name="dictionary"></param>
        public static void AddDictionary<TKey> (this HashCode hashCode, IDictionary<TKey, JToken> dictionary)
        {
            foreach (var pair in dictionary)
            {
                hashCode.Add(pair.Key);
                hashCode.Add(pair.Value.ToString());
            }
        }

        /// <summary>
        ///     Adds entries from other list that don't already exist in source list. Supports where
        ///     multiple entries can exist, making sure source contains at least as many as other
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns>Count of number of entries actually added to source.</returns>
        public static int AddMissing<T> (this IList<T> source, IEnumerable<T>? other) where T : class
        {
            if (!other.SafeAny())
                return 0;

            if (!source.Any())
            {
                source.Add(other);
                return source.Count;
            }

            var sourceGrouped = source.GroupBy(i => i).Select(g => new { Value = g.Key, Count = g.Count() });
            var otherGrouped = other.GroupBy(i => i).Select(g => new { Value = g.Key, Count = g.Count() });

            int count = 0;
            foreach (var o in otherGrouped)
            {
                var s = sourceGrouped.FirstOrDefault(i => o.Value.Equals(i.Value));
                int add = s != null ? o.Count - s.Count : o.Count;

                for (int i = add; i > 0; i--)
                {
                    source.Add(o.Value);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Prints class name in readable format even if generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>String of class name.</returns>
        public static string GetClassName (this Type? type)
        {
            if (type is null)
                return "null";

            if (type.IsGenericType)
            {
                var genericTypes = type.GenericTypeArguments;
                return $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(",", genericTypes.Select(GetClassName))}>";
            }

            return Aliases.TryGetValue(type, out string? privativeName) ? privativeName : type.Name;
        }

        /// <summary>
        ///     If type is generic, returns generic type definition, else returns input type
        /// </summary>
        public static Type GetIfGenericTypeDefinition (this Type type) => type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition() : type;

        /// <summary>
        ///     If type is generic and underlying type exists at index, returns that underlying
        ///     type, else returns null
        /// </summary>
        /// <param name="index">Underlying Type Index</param>
        public static Type? GetIfUnderlyingType (this Type type, int index = 0) => type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericArguments().Length > index ? type.GetGenericArguments()[index] : null;

        /// <summary>
        ///     Checks if type is Nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable (this Type type) => type.GetIfGenericTypeDefinition() == typeof(Nullable<>);

        public static Type RemoveNullable (this Type type) => type.GetIfGenericTypeDefinition() == typeof(Nullable<>) ? type.GetIfUnderlyingType() ?? throw new Exception("WTF - This not meant to happen") : type;

        /// <summary>
        ///     Same as IEnumerable.Any() but will return false instead of throwing
        ///     ArgumentNullException if null.
        /// </summary>
        /// <returns>False if null else result of.Any()</returns>
        public static bool SafeAny<TSource> ([NotNullWhen(true)] this IEnumerable<TSource>? source) => source != null && source.Any();

        /// <summary>
        ///     Same as IEnumerable.Any(predicate) but will return false instead of throwing
        ///     ArgumentNullException if null.
        /// </summary>
        /// <returns>False if null else result of .Any()</returns>
        public static bool SafeAny<TSource> (this IEnumerable<TSource>? source, Func<TSource, bool> predicate) => source != null && source.Any(predicate);

        /// <summary>
        ///     Camel case to normal words
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SeparateWords (this string input) => InsertSpaces().Replace(input, "$1 ");

        /// <summary>
        ///     Returns entries that are in source but not in other list. Handles duplicates. If 3
        ///     in source and 1 in other list, result will have 2 copies in it. Entries that are in
        ///     the other list but not in source are irrelevant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">List of items to confirm if they exist in other list</param>
        /// <param name="other">List of items to check against</param>
        /// <returns>Items in source list that are not in other list.</returns>
        public static IEnumerable<T> WhereNotIn<T> (this IEnumerable<T>? source, IEnumerable<T>? other) where T : class
        {
            if (!source.SafeAny())
                return [];

            if (!other.SafeAny())
                return [.. source]; // Create new array to prevent errors if source modified

            var list = new List<T>(source);

            foreach (var ni in other)
                _ = list.Remove(ni);

            return list;
        }

        [GeneratedRegex("([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))")]
        private static partial Regex InsertSpaces ();
    }
}