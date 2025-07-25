using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

using static Common.JsonConverters.FormKeyConverter;

namespace Common
{
    /// <summary>
    ///     A record ID for easy passing around when ID could be EditorID, FormID, FormKey, or
    ///     ModKey. Use the <see cref="Type" /> property to determine which ID type is stored,
    ///     before calling the property ID it references as calling properties for incorrect types
    ///     will throw exceptions.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct RecordID : IEquatable<IMajorRecord>
    {
        /// <summary>
        ///     Initializes a new instance where IDType is either <see cref="IDType.EditorID" /> or
        ///     <see cref="IDType.Invalid" />. Use mainly if you need to include a string when
        ///     <see cref="IDType.Invalid" />.
        /// </summary>
        /// <param name="type">
        ///     The type of the record ID. Must be <see cref="IDType.EditorID" /> or
        ///     <see cref="IDType.Invalid" />.
        /// </param>
        /// <param name="id">The identifier string associated with the record ID.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="type" /> is not <see cref="IDType.EditorID" /> or
        ///     <see cref="IDType.Invalid" />.
        /// </exception>
        public RecordID (IDType type, string id)
        {
            Type = type;
            RawID = type switch
            {
                IDType.EditorID or IDType.Invalid => id,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid RecordID type."),
            };
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.EditorID" />.
        /// </summary>
        /// <param name="id">The EditorID</param>
        public RecordID (string id)
        {
            Type = IDType.EditorID;
            RawID = id;
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.FormKey" />.
        /// </summary>
        /// <param name="id">The FormKey</param>
        public RecordID (FormKey id)
        {
            Type = IDType.FormKey;
            RawID = id;
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.FormID" />.
        /// </summary>
        /// <param name="id">The FormID</param>
        public RecordID (FormID id)
        {
            Type = IDType.FormID;
            RawID = id;
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.ModKey" />.
        /// </summary>
        /// <param name="id">The ModKey</param>
        public RecordID (ModKey id)
        {
            Type = IDType.ModKey;
            RawID = id;
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="RecordID" /> with an invalid type.
        /// </summary>
        public RecordID ()
        {
            Type = IDType.Invalid;
            RawID = null;
        }

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.EditorID" />, returns the EditorID
        ///     string else throws an InvalidOperationException.
        /// </summary>
        public string EditorID => Type == IDType.EditorID && RawID is string id ? id : throw new InvalidOperationException("Cannot access EditorID on a non-EditorID RecordID.");

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.FormID" />, returns the FormID else
        ///     throws an InvalidOperationException.
        /// </summary>
        public FormID FormID => Type == IDType.FormID && RawID is FormID id ? id : throw new InvalidOperationException("Cannot access FormID on a non-FormID RecordID.");

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.FormKey" />, returns the FormKey else
        ///     throws an InvalidOperationException.
        /// </summary>
        public FormKey FormKey => Type == IDType.FormKey && RawID is FormKey id ? id : throw new InvalidOperationException("Cannot access FormKey on a non-FormKey RecordID.");

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.Invalid" />, returns the string if
        ///     provided via <see cref="RecordID.RecordID(IDType, string)" /> or null else throws an
        ///     InvalidOperationException for other ID types.
        /// </summary>
        public string? Invalid => Type == IDType.Invalid ? RawID as string : throw new InvalidOperationException("Cannot access Invalid on a non-Invalid RecordID.");

        /// <summary>
        ///     If RecordID was created from null. Type would be Invalid.
        /// </summary>
        public bool IsNull => RawID is null;

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.ModKey" />, returns the ModKey else
        ///     throws an InvalidOperationException.
        /// </summary>
        public ModKey ModKey => Type == IDType.ModKey && RawID is ModKey id ? id : throw new InvalidOperationException("Cannot access ModKey on a non-ModID RecordID.");

        /// <summary>
        ///     The ID which could be any of the valid types.
        /// </summary>
        public object? RawID { get; }

        /// <summary>
        ///     Informs you to which ID type is pointed to by this RecordID. Use before calling any
        ///     of the other properties to avoid exceptions.
        /// </summary>
        public IDType Type { get; }

        private string DebuggerDisplay
            => Type switch
            {
                IDType.EditorID => $"EditorID: {EditorID}",
                IDType.FormID => $"FormID: 0x{FormID}",
                IDType.FormKey => $"FormKey: {FormKey}",
                IDType.ModKey => $"ModKey: {ModKey}",
                IDType.Invalid => $"Invalid: {Invalid ?? "null"}",
                _ => "Unknown RecordID type.",
            };

        /// <summary>
        ///     Implicitly converts a RecordID to its IDType.
        /// </summary>
        public static implicit operator IDType (RecordID id) => id.Type;

        /// <summary>
        ///     Implicitly converts a FormKey to a RecordID.
        /// </summary>
        public static implicit operator RecordID (FormKey id) => new(id);

        /// <summary>
        ///     Implicitly converts a FormID to a RecordID.
        /// </summary>
        public static implicit operator RecordID (FormID id) => new(id);

        /// <summary>
        ///     Implicitly converts a ModKey to a RecordID.
        /// </summary>
        public static implicit operator RecordID (ModKey id) => new(id);

        /// <summary>
        ///     Implicitly converts a RecordID to its string.
        /// </summary>
        public static implicit operator string (RecordID id) => id.ToString();

        /// <summary>
        ///     Checks if two RecordIDs are not equal based on their Type and ID.
        /// </summary>
        public static bool operator != (RecordID left, RecordID right) => !(left == right);

        /// <summary>
        ///     Checks if two RecordIDs are equal based on their Type and ID.
        /// </summary>
        public static bool operator == (RecordID left, RecordID right) => left.Equals(right);

        /// <inheritdoc />
        public override bool Equals ([NotNullWhen(true)] object? obj) => obj is RecordID other && Equals(other);

        /// <inheritdoc cref="object.Equals(object?)" />
        public bool Equals (RecordID obj)
            => Type == obj.Type
            && Equals(RawID, obj.RawID);

        /// <summary>
        ///     Checks if the current RecordID is equal to another IMajorRecord.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        ///     Returns false if RecordID Type is FormID or Invalid. Else will compare EditorID,
        ///     FormKey or ModKey value with IMajorRecord based on RecordID.Type
        /// </returns>
        public bool Equals (IMajorRecord? other)
            => other is not null && Type switch
            {
                IDType.EditorID => other.EditorID?.Equals(EditorID, StringComparison.OrdinalIgnoreCase) ?? false,
                IDType.FormID => false,
                IDType.FormKey => other.FormKey == FormKey,
                IDType.ModKey => other.FormKey.ModKey == ModKey,
                IDType.Invalid => false,
                _ => false,
            };

        /// <inheritdoc />
        public override int GetHashCode () => RawID?.GetHashCode() ?? 0;

        /// <summary>
        ///     Returns a string representation of the RecordID based on its type. Type itself is
        ///     not included in the string.
        /// </summary>
        public string ToString (Format formKeyFormat)
            => Type switch
            {
                IDType.EditorID => EditorID,
                IDType.FormID => $"0x{FormID.Raw:X8}",
                IDType.FormKey => FormKey.ToString(formKeyFormat),
                IDType.ModKey => ModKey.ToString(),
                IDType.Invalid => Invalid ?? string.Empty,
                _ => throw new InvalidOperationException("Unknown RecordID type."),
            };

        /// <summary>
        ///     Returns a string representation of the RecordID based on its type. Type itself is
        ///     not included in the string.
        /// </summary>
        public override string ToString ()
            => ToString(Format.Default);
    }
}