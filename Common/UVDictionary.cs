using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Common
{
    /// <summary>
    ///     A unique value dictionary that allows for reverse lookups.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class UVDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _byKey;
        private readonly Dictionary<TValue, TKey> _byValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UVDictionary{TKey, TValue}" /> class
        ///     with default comparers.
        /// </summary>
        public UVDictionary ()
        {
            _byKey = [];
            _byValue = [];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UVDictionary{TKey, TValue}" /> class
        ///     with specified key and value comparers.
        /// </summary>
        /// <param name="keyComparer"></param>
        /// <param name="valueComparer"></param>
        public UVDictionary (IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _byKey = new(keyComparer);
            _byValue = new(valueComparer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UVDictionary{TKey, TValue}" /> class
        ///     with existing dictionaries.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        private UVDictionary (Dictionary<TKey, TValue> keys, Dictionary<TValue, TKey> values)
        {
            _byKey = keys;
            _byValue = values;
        }

        /// <inheritdoc />
        public int Count => _byKey.Count;

        /// <inheritdoc />
        public bool IsFixedSize => false;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        bool ICollection.IsSynchronized => false;

        /// <inheritdoc />
        public ICollection<TKey> Keys => _byKey.Keys;

        /// <inheritdoc />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)_byKey).Keys;

        /// <inheritdoc />
        ICollection IDictionary.Keys => _byKey.Keys;

        /// <inheritdoc />
        object ICollection.SyncRoot => throw new NotImplementedException();

        /// <inheritdoc />
        public ICollection<TValue> Values => _byValue.Keys;

        /// <inheritdoc />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)_byKey).Values;

        /// <inheritdoc />
        ICollection IDictionary.Values => _byValue.Values;

        /// <inheritdoc />
        object? IDictionary.this[object key]
        {
            get => (key is TKey _key) ? _byKey[_key] : null;
            set
            {
                if (key is TKey _key && value is TValue _value)
                    UpdateOrAdd(_key, _value);
                else
                    throw new InvalidCastException("Either key and/or value of incorrect type");
            }
        }

        /// <inheritdoc />
        public TValue this[TKey key] { get => _byKey[key]; set => UpdateOrAdd(key, value); }

        /// <inheritdoc />
        public void Add (TKey key, TValue value)
        {
            if (_byKey.TryAdd(key, value))
            {
                if (_byValue.TryAdd(value, key))
                    return;

                _ = _byKey.Remove(key);
            }

            throw new ArgumentException("An item with the same key or value already exists in the dictionary.");
        }

        /// <inheritdoc />
        public void Add (KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <inheritdoc />
        void IDictionary.Add (object key, object? value)
        {
            if (key is TKey _key && value is TValue _value)
                Add(_key, _value);
            else
                throw new InvalidCastException("Either key and/or value of incorrect type");
        }

        /// <inheritdoc />
        public void Clear ()
        {
            _byKey.Clear();
            _byValue.Clear();
        }

        /// <inheritdoc />
        public bool Contains (KeyValuePair<TKey, TValue> item) => _byKey.Contains(item);

        /// <inheritdoc />
        bool IDictionary.Contains (object key) => key is TKey _key && _byKey.ContainsKey(_key);

        /// <inheritdoc />
        public bool ContainsKey (TKey key) => _byKey.ContainsKey(key);

        /// <summary>
        ///     Checks if the dictionary contains a specific value.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <returns>True if found.</returns>
        public bool ContainsValue (TValue value) => _byValue.ContainsKey(value);

        /// <inheritdoc />
        public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        /// <inheritdoc />
        public void CopyTo (Array array, int index) => throw new NotImplementedException();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () => _byKey.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator () => ((IDictionary)_byKey).GetEnumerator();

        /// <summary>
        ///     Returns the key for a provided value.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <returns>Key of value</returns>
        /// <exception cref="KeyNotFoundException">Value not found.</exception>
        public TKey GetKey (TValue value) => _byValue.TryGetValue(value, out var key)
                ? key
                : throw new KeyNotFoundException($"The value '{value}' was not found in the dictionary.");

        /// <summary>
        ///     Returns the value for a provided key.
        /// </summary>
        /// <param name="key">Key to find.</param>
        /// <returns>Value of key.</returns>
        /// <exception cref="KeyNotFoundException">Key not found.</exception>
        public TValue GetValue (TKey key) => _byKey.TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"The key '{key}' was not found in the dictionary.");

        /// <inheritdoc />
        public bool Remove (TKey key)
        {
            if (_byKey.TryGetValue(key, out var value))
            {
                _ = _byKey.Remove(key);
                _ = _byValue.Remove(value);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Remove (KeyValuePair<TKey, TValue> item)
        {
            if (_byKey.TryGetValue(item.Key, out var value) && ReferenceEquals(value, item.Value))
            {
                _ = _byKey.Remove(item.Key);
                _ = _byValue.Remove(value);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        void IDictionary.Remove (object key)
        {
            if (key is TKey _key)
                _ = Remove(_key);
        }

        /// <summary>
        ///     Swaps the key and value around. This is useful for reverse lookups when casting to
        ///     standard Dictionary interfaces.
        ///
        ///     NOTE: This doesn't create a clone. Modifying original will modify swapped
        ///     dictionary.
        /// </summary>
        /// <returns>UVDictionary with keys and values swapped.</returns>
        public UVDictionary<TValue, TKey> Swap () => new(_byValue, _byKey);

        /// <summary>
        ///     Attempts to add a new key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">Unique key of entry to add.</param>
        /// <param name="value">Unique value of entry to add.</param>
        /// <returns>
        ///     True if value was successfully added. False could mean either key or value provided
        ///     were not unique.
        /// </returns>
        public bool TryAdd (TKey key, TValue value)
        {
            if (_byKey.TryAdd(key, value))
            {
                if (_byValue.TryAdd(value, key))
                    return true;
                _ = _byKey.Remove(key);
            }

            return false;
        }

        /// <summary>
        ///     Attempts to get the key for a provided value.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <param name="key">Key of value found. Null if value not found.</param>
        /// <returns>True if value exists.</returns>
        public bool TryGetKey (TValue value, [NotNullWhen(true)] out TKey? key) => _byValue.TryGetValue(value, out key) && key is not null;

        /// <summary>
        ///     Attempts to get the value for a provided key.
        /// </summary>
        /// <param name="key">Key to find.</param>
        /// <param name="value">Value of key found. Null if key not found.</param>
        /// <returns>True if key found.</returns>
        public bool TryGetValue (TKey key, [NotNullWhen(true)] out TValue? value) => _byKey.TryGetValue(key, out value) && value is not null;

        /// <summary>
        ///     Updates an existing key-value pair or adds a new one. If the value already exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateOrAdd (TKey key, TValue value)
        {
            bool keyFound = _byKey.TryGetValue(key, out var existingKeyValue);
            bool valueFound = _byValue.TryGetValue(value, out var existingValueKey);

            if (keyFound && valueFound && key.Equals(existingValueKey) && value.Equals(existingKeyValue))
                return;

            if (valueFound && existingValueKey is not null)
                _ = Remove(existingValueKey);

            if (keyFound)
            {
                if (existingKeyValue is not null)
                    _ = _byValue.Remove(existingKeyValue);

                _byKey[key] = value;
                _byValue.Add(value, key);
            }
            else
            {
                _byKey.Add(key, value);
                _byValue.Add(value, key);
            }
        }
    }
}