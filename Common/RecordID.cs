using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

using Noggog;

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
    public readonly struct RecordID : IEquatable<IMajorRecordGetter>, IEquatable<RecordID>, IEquatable<ModKey>, IEquatable<string>
    {
        private readonly int hashcode;

        /// <summary>
        ///     Initializes a new instance where IDType is either <see cref="IDType.Name" /> or
        ///     <see cref="IDType.Invalid" />. Use mainly if you need to include a string when
        ///     <see cref="IDType.Invalid" />.
        /// </summary>
        /// <param name="type">
        ///     The type of the record ID. Must be <see cref="IDType.Name" /> or
        ///     <see cref="IDType.Invalid" />.
        /// </param>
        /// <param name="id">The identifier string associated with the record ID.</param>
        /// <param name="isWildcard">EditorID entered was wildcard</param>
        /// <param name="limitTo">
        ///     Limit this record id to only matching against listed entries.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="type" /> is not <see cref="IDType.Name" /> or
        ///     <see cref="IDType.Invalid" />.
        /// </exception>
        public RecordID (IDType type, string? id, bool isWildcard = false, EqualsOptions limitTo = EqualsOptions.AllString)
        {
            RawID = type switch
            {
                IDType.Name => id is not null ? id : throw new ArgumentNullException(nameof(id), "ID cannot be null when type is Name."),
                IDType.Invalid => id,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid RecordID type."),
            };

            Type = type;
            IsWildcard = isWildcard;
            LimitTo = limitTo & EqualsOptions.AllString; // Remove any non valid options
            hashcode = id?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.Name" />.
        /// </summary>
        /// <param name="id">The EditorID</param>
        /// <param name="isWildcard">EditorID entered was wildcard</param>
        /// <param name="limitTo">
        ///     Limit this record ID to only matching if current field is included in EqualOptions
        /// </param>
        public RecordID (string id, bool isWildcard = false, EqualsOptions limitTo = EqualsOptions.AllString) : this(IDType.Name, id, isWildcard, limitTo) { }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.FormKey" />.
        /// </summary>
        /// <param name="id">The FormKey</param>
        /// <param name="limitTo">
        ///     Limit this record ID to only matching if current field is included in EqualOptions
        /// </param>
        public RecordID (FormKey id, EqualsOptions limitTo = EqualsOptions.AllFormKey)
        {
            RawID = id;
            Type = IDType.FormKey;
            LimitTo = limitTo & EqualsOptions.AllFormKey; // Remove any non valid options
            hashcode = id.GetHashCode();
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.FormID" />.
        /// </summary>
        /// <param name="id">The FormID</param>
        /// <param name="limitTo">
        ///     Limit this record ID to only matching if current field is included in EqualOptions
        /// </param>
        public RecordID (FormID id, EqualsOptions limitTo = EqualsOptions.AllFormKey)
        {
            RawID = id;
            Type = IDType.FormID;
            LimitTo = limitTo & EqualsOptions.AllFormKey; // Remove any non valid options
            hashcode = id.GetHashCode();
        }

        /// <summary>
        ///     Initializes a new instance where IDType is <see cref="IDType.ModKey" />.
        /// </summary>
        /// <param name="id">The ModKey</param>
        /// <param name="limitTo">
        ///     Limit this record ID to only matching if current field is included in EqualOptions
        /// </param>
        public RecordID (ModKey id, EqualsOptions limitTo = EqualsOptions.ModKey)
        {
            RawID = id;
            Type = IDType.ModKey;
            LimitTo = limitTo & EqualsOptions.AllModKey; // Remove any non valid options
            hashcode = id.GetHashCode();
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="RecordID" /> with an invalid type.
        /// </summary>
        public RecordID ()
        {
            RawID = null;
            Type = IDType.Invalid;
            hashcode = 0;
            LimitTo = EqualsOptions.None;
        }

        /// <summary>
        ///     Determines what record properties should be checked when using
        ///     <see cref="Equals(IMajorRecordGetter?, EqualsOptions)" />
        /// </summary>
        [Flags]
        public enum EqualsOptions
        {
            /// <summary>
            ///     Will never return true as not checking anything
            /// </summary>
            None = 0,

            /// <summary>
            ///     Check Record's FormKey (FormKey)
            /// </summary>
            FormKey = 1 << 0,

            /// <summary>
            ///     Check Record's EditorID (String)
            /// </summary>
            EditorID = 1 << 1,

            /// <summary>
            ///     Check if Record comes from mod. (ModKey)
            /// </summary>
            ModKey = 1 << 2,

            /// <summary>
            ///     The basic checks done if you just call
            ///     <see cref="Equals(IMajorRecordGetter?)" />
            /// </summary>
            BasicChecks = FormKey | EditorID | ModKey,

            /// <summary>
            ///     Check Name if record is INamedRecord. (String)
            /// </summary>
            Name = 1 << 3,

            /// <summary>
            ///     Check if Keywords contains this RecordID. Checks keyword's FormKey and EditorID
            ///     for matches. (FormKey | String)
            /// </summary>
            Keywords = 1 << 4,

            /// <summary>
            ///     Match against all possible options
            /// </summary>
            Default = BasicChecks | Name | Keywords,

            /// <summary>
            ///     All possible options
            /// </summary>
            All = Default,

            /// <summary>
            ///     All valid options for FormKeys
            /// </summary>
            AllFormKey = FormKey | Keywords,

            /// <summary>
            ///     All valid options for ModKeys
            /// </summary>
            AllModKey = ModKey,

            /// <summary>
            ///     All valid options for Strings
            /// </summary>
            AllString = EditorID | Name | Keywords,
        }

        /// <summary>
        ///     For identifying the current field being checked.
        /// </summary>
        public enum Field
        {
            /// <summary>
            ///     Search for nothing. Unless otherwise stated using this will mean you will not
            ///     return any results.
            /// </summary>
            None = EqualsOptions.None,

            /// <summary>
            ///     Current field being checked is FormKey
            /// </summary>
            FormKey = EqualsOptions.FormKey,

            /// <summary>
            ///     Current field being checked is ModKey (Source mod for record)
            /// </summary>
            ModKey = EqualsOptions.ModKey,

            /// <summary>
            ///     Current field being checked is the EditorID
            /// </summary>
            EditorID = EqualsOptions.EditorID,

            /// <summary>
            ///     Current field being checked is the Name
            /// </summary>
            Name = EqualsOptions.Name,

            /// <summary>
            ///     Current field being checked is the Keywords linked to the main record being
            ///     searched
            /// </summary>
            Keywords = EqualsOptions.Keywords,
        }

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
        ///     provided via <see cref="RecordID.RecordID(IDType, string?, bool, EqualsOptions)" />
        ///     or null else throws an InvalidOperationException for other ID types.
        /// </summary>
        public string? Invalid => Type == IDType.Invalid ? RawID as string : throw new InvalidOperationException("Cannot access Invalid on a non-Invalid RecordID.");

        /// <summary>
        ///     If RecordID was created from null. Type would be Invalid.
        /// </summary>
        public bool IsNull => RawID is null;

        /// <summary>
        ///     Input was string starting with * so when checking EditorID or Names should use
        ///     contains not equals.
        /// </summary>
        public bool IsWildcard { get; }

        /// <summary>
        ///     Restricts the RecordID to only matching against these values.
        /// </summary>
        public EqualsOptions LimitTo { get; }

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.ModKey" />, returns the ModKey else
        ///     throws an InvalidOperationException.
        /// </summary>
        public ModKey ModKey => Type == IDType.ModKey && RawID is ModKey id ? id : throw new InvalidOperationException("Cannot access ModKey on a non-ModID RecordID.");

        /// <summary>
        ///     If <see cref="Type" /> is <see cref="IDType.Name" />, returns the EditorID string
        ///     else throws an InvalidOperationException.
        /// </summary>
        public string Name => Type == IDType.Name && RawID is string id ? id : throw new InvalidOperationException("Cannot access EditorID on a non-name RecordID.");

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
                IDType.Name => $"EditorID: {Name}",
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
        public override bool Equals ([NotNullWhen(true)] object? obj)
            => obj switch
            {
                RecordID other => Equals(other),
                FormKey formKey => Equals(formKey),
                ModKey modKey => Equals(modKey),
                string str => Equals(str),
                _ => false,
            };

        /// <summary>
        ///     Compares this RecordID to another
        /// </summary>
        public bool Equals (RecordID other)
            => Type == other.Type
            && LimitTo == other.LimitTo
            && IsWildcard == other.IsWildcard
            && RawID is string str && other.RawID is string otherStr ? str.Equals(otherStr, StringComparison.OrdinalIgnoreCase) : Equals(RawID, other.RawID);

        /// <summary>
        ///     Checks if supplied FormKey matches this RecordID
        /// </summary>
        /// <param name="value">FormKey to check</param>
        /// <param name="field">Current field value is from.</param>
        /// <returns>
        ///     True if supplied value matches this RecordID and field is included in
        ///     <see cref="RecordID.LimitTo" />
        /// </returns>
        public bool Equals (FormKey value, Field field)
            => LimitTo.fieldEnabled(field)
            && Equals(value);

        /// <summary>
        ///     Checks if supplied FormKey matches this RecordID
        /// </summary>
        /// <param name="value">FormKey to check</param>
        /// <returns>
        ///     True if supplied value matches this RecordID and field is included in
        ///     <see cref="RecordID.LimitTo" />
        /// </returns>
        public bool Equals (FormKey value)
            => (
                Type == IDType.FormKey
                && FormKey == value
            ) || (
                Type == IDType.ModKey
                && ModKey == value.ModKey
            );

        /// <summary>
        ///     Checks if supplied ModKey matches this RecordID
        /// </summary>
        /// <param name="value">FormKey to check</param>
        /// <param name="field">Current field value is from.</param>
        /// <returns>
        ///     True if supplied value matches this RecordID and field is included in
        ///     <see cref="RecordID.LimitTo" />
        /// </returns>
        public bool Equals (ModKey value, Field field)
            => Equals(value)
            && LimitTo.fieldEnabled(field);

        /// <inheritdoc />
        public bool Equals (ModKey value)
            => Type == IDType.ModKey
            && ModKey == value;

        /// <summary>
        ///     Checks if supplied string matches this RecordID
        /// </summary>
        /// <param name="value">String to check</param>
        /// <param name="field">Current field value is from.</param>
        /// <returns>
        ///     True if supplied value matches this RecordID and field is included in
        ///     <see cref="RecordID.LimitTo" />
        /// </returns>
        public bool Equals (string? value, Field field)
            => LimitTo.fieldEnabled(field)
            && Equals(value);

        /// <inheritdoc />
        public bool Equals (string? value)
            => value is not null
            && RawID is string str
            && (IsWildcard ? value.Contains(str, StringComparison.OrdinalIgnoreCase) : str.Equals(value, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc cref="Equals(IMajorRecordGetter?, EqualsOptions, ILinkCache?, Field)" />
        public bool Equals (IMajorRecordGetter? record) => Equals(record, EqualsOptions.BasicChecks, null, Field.None);

        /// <inheritdoc cref="Equals(IMajorRecordGetter?, EqualsOptions, ILinkCache?, Field)" />
        public bool Equals (IMajorRecordGetter? record, EqualsOptions equalsOptions) => Equals(record, equalsOptions, null, Field.None);

        /// <summary>
        ///     Checks if record is a match for this RecordID.
        /// </summary>
        /// <param name="record">Record to check if it matches</param>
        /// <param name="equalsOptions">
        ///     Details what to try and match against in the record.
        /// </param>
        /// <param name="linkCache">
        ///     If <see cref="EqualsOptions" /> includes <see cref="EqualsOptions.Keywords" /> link
        ///     cache must be provided if you want to be able to match against keyword names and
        ///     that keyword actually exists.
        /// </param>
        /// <param name="field">Current field if relevant else <see cref="Field.None" /></param>
        /// <returns>
        ///     Will compare EditorID, Name, FormKey, ModKey and Keyword values against this
        ///     RecordID and return true if matched. Always false if <see cref="RecordID.Type" /> is
        ///     <see cref="IDType.FormID" /> or <see cref="IDType.Invalid" />
        /// </returns>
        public bool Equals (IMajorRecordGetter? record, EqualsOptions equalsOptions, ILinkCache? linkCache, Field field)
        {
            if (record is null || equalsOptions == EqualsOptions.None)
                return false;

            bool isSubProperty = field != Field.None;

            if (isSubProperty && !LimitTo.fieldEnabled(field))
                return false;

            // The basic checks
            bool result = Type switch
            {
                IDType.Name // Check EditorID
                => equalsOptions.HasFlag(EqualsOptions.EditorID)
                && !record.EditorID.IsNullOrWhitespace()
                && Equals(record.EditorID, isSubProperty ? field : Field.EditorID),
                IDType.FormID => false,
                IDType.FormKey => equalsOptions.HasFlag(EqualsOptions.FormKey) && Equals(record.FormKey, isSubProperty ? field : Field.FormKey),
                IDType.ModKey => equalsOptions.HasFlag(EqualsOptions.ModKey) && Equals(record.FormKey.ModKey, isSubProperty ? field : (Field)EqualsOptions.ModKey),
                IDType.Invalid => false,
                _ => false,
            };

            if (result)
                return true;

            // Check record name
            if (equalsOptions.HasFlag(EqualsOptions.Name) &&
                Type is IDType.Name or IDType.Invalid &&
                RawID is string &&
                record is INamed namedRecord &&
                namedRecord.Name is not null &&
                Equals(namedRecord.Name, isSubProperty ? field : Field.Name))
            {
                return true;
            }

            // Check Keywords
            if (equalsOptions.HasFlag(EqualsOptions.Keywords) &&
                Type is IDType.FormKey or IDType.Name &&
                record is IKeywordedGetter keywordedRecord &&
                keywordedRecord.Keywords is not null)
            {
                if (LimitTo.fieldEnabled(Field.Keywords))
                {
                    foreach (var key in keywordedRecord.Keywords)
                    {
                        if (linkCache is null)
                        {
                            // Well I guess if no linkCache we can
                            if (Type == IDType.FormKey && key.FormKey == FormKey)
                                return true;

                            continue;
                        }

                        var keyword = key.TryResolve(linkCache);
                        if (keyword is null)
                        {
                            // Matched keyword FormKey but record not found - no need to check any
                            // more keywords
                            if (Type == IDType.FormKey && key.FormKey == FormKey)
                                break;

                            continue;
                        }

                        if (Equals(keyword, EqualsOptions.FormKey | EqualsOptions.EditorID, null, Field.Keywords))
                            return true;
                    }
                }
            }

            // Nothing else to check so failed to match
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode () => hashcode;

        /// <summary>
        ///     Returns a string representation of the RecordID based on its type. Type itself is
        ///     not included in the string.
        /// </summary>
        public string ToString (Format formKeyFormat)
            => Type switch
            {
                IDType.Name => IsWildcard ? $"*{Name}" : Name,
                IDType.FormID => $"0x{FormID.Raw:X8}",
                IDType.FormKey => FormKey.ToString(formKeyFormat),
                IDType.ModKey => ModKey.ToString(),
                IDType.Invalid => IsWildcard ? $"*{Invalid}" : Invalid ?? string.Empty,
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