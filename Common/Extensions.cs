using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using DynamicData;

using Newtonsoft.Json.Linq;

namespace Common
{
    /// <summary>
    ///     My standard method extensions.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        ///     Dictionary of types to friendly display name, used by GetClassName, when type.Name
        ///     isn't friendly.
        /// </summary>
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
        ///     Adds dictionary keys to hash code, so two separate dictionaries that have the same
        ///     keys will have the same hash code.
        /// </summary>
        public static void AddDictionary<TKey> (this HashCode hashCode, IDictionary<TKey, JToken> dictionary)
        {
            foreach (var pair in dictionary)
                hashCode.Add(pair.Key);
        }

        /// <summary>
        ///     Adds entries from other list that don't already exist in source list. Supports where
        ///     multiple entries can exist, making sure source contains at least as many as other
        /// </summary>
        /// <returns>Number of entries actually added to source.</returns>
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
                int add = s is not null ? o.Count - s.Count : o.Count;

                for (int i = add; i > 0; i--)
                {
                    source.Add(o.Value);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Creates new array with second array appended to first array.
        /// </summary>
        public static T[] AddRange<T> (this T[] array, IEnumerable<T> values) where T : class
        {
            var list = array.ToList();
            list.AddRange(values);
            return [.. list];
        }

        /// <summary>
        ///     Will explode generic types out, until max depth or generic type has more than 1
        ///     argument
        /// </summary>
        /// <param name="type">The type to explode</param>
        /// <param name="maxDepth">
        ///     To limit how deep it goes. Once it reaches this depth it will return remaining
        ///     unexploded as final entry in array. Value of 0 = no limit. Value of 1 will just
        ///     return input type as only array member.
        /// </param>
        /// <returns>
        ///     Array of types. If input type not generic then result will be array with single
        ///     value of the input type.
        /// </returns>
        /// <example>
        ///     typeof(List&lt;string&lt;).Explode() == [typeof(List&lt;&lt;), typeof(string)]
        /// </example>
        public static Type[] Explode (this Type type, int maxDepth = 0) => doExplode(type, maxDepth, 1);

        /// <summary>
        ///     Prints class name in readable format even if generic type
        /// </summary>
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
        /// <param name="type">The type to check.</param>
        /// <param name="index">Underlying Type Index</param>
        public static Type? GetIfUnderlyingType (this Type type, int index = 0) => type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericArguments().Length > index ? type.GetGenericArguments()[index] : null;

        /// <summary>
        ///     Checks if type is Nullable
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if implements Nullable&lt;&gt;</returns>
        public static bool IsNullable (this Type type) => type.GetIfGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        ///     Returns enumerable of lines in string, splitting on new line characters.
        /// </summary>
        public static IEnumerable<string> Lines (this string source)
        {
            ArgumentNullException.ThrowIfNull(source);

            using var reader = new StringReader(source);

            while (reader.ReadLine() is { } line)
                yield return line;
        }

        /// <summary>
        ///     If type is Nullable will return the inner type, else just returns the type
        ///     unchanged.
        /// </summary>
        /// <param name="type">Possible nullable type</param>
        /// <returns>Non nullable type</returns>
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
        public static bool SafeAny<TSource> ([NotNullWhen(true)] this IEnumerable<TSource>? source, Func<TSource, bool> predicate) => source != null && source.Any(predicate);

        /// <summary>
        ///     Pascal / Camel case to normal words
        /// </summary>
        /// <param name="input"></param>
        /// <returns>New string with spaces added between words. Case not changed.</returns>
        public static string SeparateWords (this string input) => InsertSpaces().Replace(input, "$1 ");

        /// <summary>
        ///     Clones a dictionary. This is a shallow clone, meaning that the keys and values are
        ///     not cloned.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue> (this Dictionary<TKey, TValue> dictionary)
                    where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            Dictionary<TKey, TValue> clone = new(dictionary.Count, dictionary.Comparer);
            foreach (var pair in dictionary)
                clone.Add(pair.Key, pair.Value);

            return clone;
        }

        /// <summary>
        ///     Adds entries from other dictionary to this one. If the key already exists, it will
        ///     not add it or throw an exception. This is useful for merging dictionaries. Failure
        ///     to add an entry will not stop it from adding the rest of the entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="addTo">Dictionary to add the entries to.</param>
        /// <param name="addFrom">Dictionary to read the entries to add from.</param>
        /// <returns>
        ///     Count of number of entries added. If this equals size of addFrom.Count then all
        ///     entries were successfully added. If addFrom is null will be 0 same as if addFrom
        ///     empty or all entries failed to add. -1 returned only if addTo is null.
        /// </returns>
        public static int TryAdd<TKey, TValue> (this IDictionary<TKey, TValue> addTo, IDictionary<TKey, TValue>? addFrom)
                    where TKey : notnull
        {
            if (addTo is null)
                return -1;

            if (addFrom == null)
                return 0;

            int count = 0;

            foreach (var pair in addFrom)
            {
                if (addTo.TryAdd(pair.Key, pair.Value))
                    count++;
            }

            return count;
        }

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

        private static Type[] doExplode (Type type, int maxDepth, int depth)
        {
            if (maxDepth == depth || !type.IsGenericType || type.IsGenericTypeDefinition || type.GetGenericArguments().Length != 1)
                return [type];

            List<Type> list = [type.GetGenericTypeDefinition()];
            list.AddRange(doExplode(type.GetGenericArguments()[0], maxDepth, ++depth));

            return [.. list];
        }

        [GeneratedRegex("([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))")]
        private static partial Regex InsertSpaces ();
    }
}