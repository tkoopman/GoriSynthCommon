using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

using Noggog;

namespace Common
{
    /// <summary>
    ///     Builds a searching index of RecordIDs mapped to data, that supports wildcard string
    ///     lookups
    /// </summary>
    /// <typeparam name="T">Data type to store linked to RecordID</typeparam>
    /// <param name="comparer">RecordID comparer</param>
    public class IndexedRecordIDs<T> (IEqualityComparer<T>? comparer = null) where T : notnull
    {
        private readonly IEqualityComparer<T>? _comparer = comparer;

        private readonly int[] _minLength =
        [
            int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            int.MaxValue, int.MaxValue,
        ];

        private readonly Dictionary<string, List<int>> matchesExact = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<RecordID, List<int>> matchesKey = [];
        private readonly List<StartsWithKey>[] matchesWildcardBuckets = new List<StartsWithKey>[27];
        private readonly List<(RecordID recordID, T value)> rawData = [];
        private int _absoluteMin = int.MaxValue;
        private bool _wildcardsSorted = false;

        /// <summary>
        ///     Index contains at least 1 exact string match entry.
        /// </summary>
        public bool HasExactMatchEntries { get; private set; }

        /// <summary>
        ///     Index contains at least 1 FormKey / ModKey match entry.
        /// </summary>
        public bool HasKeyMatchEntries { get; private set; }

        /// <summary>
        ///     Index contains at least 1 string match entry. Could be exact or wildcard.
        /// </summary>
        public bool HasStringMatchEntries => HasExactMatchEntries || HasWildcardEntries;

        /// <summary>
        ///     Index contains at least 1 wildcard string match entry.
        /// </summary>
        public bool HasWildcardEntries { get; private set; }

        /// <summary>
        ///     Add recordID to index linked to item.
        /// </summary>
        /// <param name="recordID">RecordID to add</param>
        /// <param name="item">Item to return when this ID is looked up</param>
        /// <exception cref="ArgumentException">
        ///     RecordID provided is of an unsupported type.
        /// </exception>
        public void Add (RecordID recordID, T item)
        {
            int rawIndex = rawData.Count;
            rawData.Add((recordID, item));

            switch (recordID.Type)
            {
                case IDType.Name:
                    if (recordID.IsWildcard)
                    {
                        _wildcardsSorted = false; // Set first so getBucketIndex knows read write operation
                        string id = recordID.Name;
                        int index = getBucketIndex(id[0]);
                        matchesWildcardBuckets[index].Add(new(id, rawIndex));

                        if (id.Length < _absoluteMin)
                            _absoluteMin = id.Length;

                        if (id.Length < _minLength[index])
                            _minLength[index] = id.Length;

                        HasWildcardEntries = true;
                        return;
                    }

                    matchesExact.AddValue(recordID, rawIndex);
                    HasExactMatchEntries = true;
                    break;

                case IDType.FormKey:
                case IDType.ModKey:
                    matchesKey.AddValue(recordID, rawIndex);
                    HasKeyMatchEntries = true;
                    break;

                case IDType.Invalid:
                case IDType.FormID:
                default:
                    throw new ArgumentException("FormID and Invalid record IDs not supported");
            }
        }

        /// <summary>
        ///     Lookup a record and find all matching entries. Will return tuple of value linked to
        ///     record ID(s) and list of record IDs that matched it, as can have multiple pointing
        ///     to same data.
        /// </summary>
        /// <param name="record">Record to search for.</param>
        /// <param name="equalsOptions">
        ///     What properties are allowed to be used to match against.
        /// </param>
        /// <param name="linkCache">
        ///     LinkCache for current load order so we can lookup keywords to confirm valid and
        ///     name.
        /// </param>
        /// <returns>
        ///     List of matches as tuple of value linked to record ID(s) and list of record IDs that
        ///     matched it.
        /// </returns>
        public IEnumerable<(T value, IEnumerable<RecordID> recordIDs)> FindAll (IMajorRecordGetter record, RecordID.EqualsOptions equalsOptions, ILinkCache linkCache)
        {
            if (rawData.Count == 0)
                yield break;

            if (!_wildcardsSorted)
                SortIndex();

            Dictionary<T, HashSet<RecordID>> results = new(_comparer);
            IEnumerable<IKeywordCommonGetter>? keywords = null;

            if (equalsOptions.HasFlag(RecordID.EqualsOptions.Keywords) &&
                record is IKeywordedGetter keyworded &&
                keyworded.Keywords is not null &&
                keyworded.Keywords.Count > 0)
            {
                keywords = keyworded.Keywords.Select(k => k.TryResolve(linkCache)).Where(k => k is not null)!;
            }

            if (HasKeyMatchEntries)
            {
                if (equalsOptions.HasFlag(RecordID.EqualsOptions.FormKey))
                {
                    foreach (var (recordID, value) in InternalFindAll(record.FormKey, RecordID.Field.FormKey))
                        _ = results.AddValue(value, recordID);
                }

                if (equalsOptions.HasFlag(RecordID.EqualsOptions.ModKey))
                {
                    foreach (var (recordID, value) in InternalFindAll(record.FormKey.ModKey, RecordID.Field.ModKey))
                        _ = results.AddValue(value, recordID);
                }

                if (keywords is not null)
                {
                    foreach (var k in keywords)
                    {
                        foreach (var (recordID, value) in InternalFindAll(k.FormKey, RecordID.Field.Keywords))
                            _ = results.AddValue(value, recordID);
                    }
                }
            }

            if (HasStringMatchEntries)
            {
                if (!record.EditorID.IsNullOrWhitespace() && equalsOptions.HasFlag(RecordID.EqualsOptions.EditorID))
                {
                    foreach (var (recordID, value) in InternalFindAll(record.EditorID, RecordID.Field.EditorID))
                        _ = results.AddValue(value, recordID);
                }

                if (equalsOptions.HasFlag(RecordID.EqualsOptions.Name) && record is INamedGetter named)
                {
                    foreach (var (recordID, value) in InternalFindAll(named.Name, RecordID.Field.Name))
                        _ = results.AddValue(value, recordID);
                }

                if (keywords is not null)
                {
                    foreach (var k in keywords)
                    {
                        foreach (var (recordID, value) in InternalFindAll(k.EditorID, RecordID.Field.Keywords))
                            _ = results.AddValue(value, recordID);
                    }
                }
            }

            foreach (var r in results)
                yield return (r.Key, r.Value);
        }

        /// <summary>
        ///     FindAll but returns results as found with no grouping of same T values, if found via
        ///     multiple RecordIDs.
        ///
        ///     NOTE: <see cref="SortIndex" /> MUST of been called prior to calling this method.
        /// </summary>
        public IEnumerable<(RecordID recordID, T value)> InternalFindAll (string? name, RecordID.Field field)
        {
            if (name is not null && name.Length > 0)
            {
                if (HasExactMatchEntries && matchesExact.TryGetValue(name, out var list))
                {
                    foreach (int item in list)
                        yield return rawData[item];
                }

                if (HasWildcardEntries)
                {
                    // As index is by string starts with value, we loop removing first char each
                    // time until we smaller than smallest indexed string.
                    while (name.Length >= _absoluteMin)
                    {
                        int index = getBucketIndex(name[0]);
                        int minLen = _minLength[index];
                        if (name.Length >= minLen)
                        {
                            string minStr = name[..minLen];
                            var bucket = matchesWildcardBuckets[index];

                            // Perform search for the smallest starting characters in bucket. This
                            // will either return index of exact match or ~next entry in list found
                            index = bucket.BinarySearch(new(minStr));

                            if (index < 0)
                            {
                                // As this was not an exact match only need to search forward from
                                // this point
                                index = ~index;
                            }
                            else
                            {
                                // As indexed strings do not need to be unique we must search
                                // backwards from returned index, to see if extra values, as binary
                                // search is not guaranteed to return first matching entry in list.
                                for (int i = index - 1; i >= 0; i--)
                                {
                                    var b = bucket[i];
                                    var data = rawData[b.RawIndex];

                                    if (b.CompareTo(minStr) < 0)
                                        break; // If entry comes before minStr than no need to continue

                                    if (!data.recordID.LimitTo.fieldEnabled(field))
                                        continue;

                                    if (b.Check(name))
                                        yield return data;
                                }
                            }

                            // Now we also search forward until we find entry that would of come
                            // after string being looked up, as any possible matches would of
                            // already of been found by then.
                            for (int i = index; i < bucket.Count; i++)
                            {
                                var b = bucket[i];
                                var data = rawData[b.RawIndex];

                                if (b.CompareTo(name) > 0)
                                    break; // Once name would of come before b in list no need to keep searching

                                if (!data.recordID.LimitTo.fieldEnabled(field))
                                    continue;

                                if (b.Check(name))
                                    yield return data;
                            }
                        }

                        // Remove first character and loop around to check again
                        name = name[1..];
                    }
                }
            }
        }

        /// <summary>
        ///     FindAll but returns results as found with no grouping of same T values, if found via
        ///     multiple RecordIDs.
        /// </summary>
        public IEnumerable<(RecordID recordID, T value)> InternalFindAll (FormKey formKey, RecordID.Field field)
        {
            if (matchesKey.TryGetValue(formKey, out var list))
            {
                foreach (int item in list)
                {
                    var data = rawData[item];

                    if (data.recordID.LimitTo.fieldEnabled(field))
                        yield return data;
                }
            }
        }

        /// <summary>
        ///     FindAll but returns results as found with no grouping of same T values, if found via
        ///     multiple RecordIDs.
        /// </summary>
        public IEnumerable<(RecordID recordID, T value)> InternalFindAll (ModKey modKey, RecordID.Field field)
        {
            if (matchesKey.TryGetValue(modKey, out var list))
            {
                foreach (int item in list)
                {
                    var data = rawData[item];

                    if (data.recordID.LimitTo.fieldEnabled(field))
                        yield return data;
                }
            }
        }

        /// <summary>
        ///     FindAll but returns results as found with grouping of same T values, if found via
        ///     multiple RecordIDs.
        ///
        ///     NOTE: <see cref="SortIndex" /> MUST of been called prior to calling this method.
        /// </summary>
        public Dictionary<T, HashSet<RecordID>> InternalFindAllGrouped (string name, RecordID.Field field)
        {
            var result = new Dictionary<T, HashSet<RecordID>>(comparer: _comparer);

            InternalFindAll(name, field).ForEach(i => _ = result.AddValue(i.value, i.recordID));

            return result;
        }

        /// <summary>
        ///     Used for seeing how index is being used once filled with data.
        /// </summary>
        /// <param name="output">
        ///     Text writer to send stats to. Will use Console.Out if null.
        /// </param>
        public void PrintStats (TextWriter? output = null)
        {
            output ??= Console.Out;
            output.WriteLine($"Key Indexed: {matchesKey.Count}");
            output.WriteLine($"Exact String Indexed: {matchesExact.Count}");

            if (!HasWildcardEntries)
            {
                output.WriteLine("Contains String Indexes: empty");
                return;
            }

            output.WriteLine("Contains String Indexes:");
            output.WriteLine($"Absolute Min: {_absoluteMin}");
            output.WriteLine("Index | Count | Min");
            foreach (char c in "#ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                int index = getBucketIndex(c);
                output.WriteLine($"{c} | {matchesWildcardBuckets[index]?.Count} | {_minLength[index]}");
            }
        }

        /// <summary>
        ///     Sorts the index, as for performance of adding bulk entries it does not auto sort on
        ///     adding entries. No need to manually call when performing FindAll as it will auto
        ///     call this if required.
        /// </summary>
        /// <remarks>
        ///     Using any of the InternalFindAll methods DOES NOT automatically call this.
        /// </remarks>
        public void SortIndex ()
        {
            _wildcardsSorted = true;
            foreach (var wildcard in matchesWildcardBuckets)
                wildcard?.Sort();
        }

        /// <summary>
        ///     Returns bucket index # to use based on character.
        /// </summary>
        /// <param name="c">
        ///     This should be the first character of the string you need bucket for
        /// </param>
        /// <returns>Index to <see cref="matchesWildcardBuckets" /> to use.</returns>
        private int getBucketIndex (char c)
        {
            c = char.ToUpperInvariant(c);
            int index = (int)c - 64; // This will make A-Z = 1-26
            index = index is < 0 or > 26 ? 0 : index; // Any other char will use bucket 0

            // If sorted then we are doing readonly operations so no need to create list
            if (!_wildcardsSorted)
                matchesWildcardBuckets[index] ??= [];

            return index;
        }

        /// <summary>
        ///     Used for storing string and linked data value in indexers.
        /// </summary>
        private readonly struct StartsWithKey : IComparable<StartsWithKey>, IComparable<string>
        {
            /// <summary>
            ///     Create new key for storing in index.
            /// </summary>
            /// <param name="str">String to add and index.</param>
            /// <param name="rawIndex">Index to RecordID and item linked to this string</param>
            public StartsWithKey (string str, int rawIndex)
            {
                StartsWith = str;
                RawIndex = rawIndex;
            }

            /// <summary>
            ///     Create Key to search index with. Should only be used for lookups and not storing
            ///     data.
            /// </summary>
            internal StartsWithKey (string str)
            {
                StartsWith = str;
                RawIndex = -1;
            }

            /// <summary>
            ///     Index to linked RecordID and Data in <see cref="rawData" />
            /// </summary>
            public readonly int RawIndex { get; }

            /// <summary>
            ///     The string that must be found at the start of a string to match this entry.
            /// </summary>
            public readonly string StartsWith { get; }

            /// <summary>
            ///     Check if provided string starts with stored value
            /// </summary>
            public readonly bool Check (ReadOnlySpan<char> other) => other.StartsWith(StartsWith, StringComparison.OrdinalIgnoreCase);

            /// <inheritdoc />
            public int CompareTo (string? obj) => string.Compare(StartsWith, obj, StringComparison.OrdinalIgnoreCase);

            /// <inheritdoc />
            public int CompareTo (StartsWithKey obj) => string.Compare(StartsWith, obj.StartsWith, StringComparison.OrdinalIgnoreCase);

            /// <inheritdoc />
            public int CompareTo (object? obj) => obj switch
            {
                string str => CompareTo(str),
                StartsWithKey key => CompareTo(key),
                _ => 0,
            };
        }
    }
}