using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

using Loqui;

using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace Common
{
    /// <summary>
    ///     Common methods for classes to use with Synthesis
    /// </summary>
    public static partial class SynthCommon
    {
        /// <summary>
        ///     Adds 0 padding to String representation of a form key. Doesn't actually validate the
        ///     rest of the string.
        /// </summary>
        /// <param name="input">
        ///     String representation of a form key that may not be padded
        /// </param>
        /// <returns>
        ///     String representation of the form key with 0 padding added if required.
        /// </returns>
        public static string FixFormKey (string input) => RegexFormKey().Replace(input, m => m.Value.PadLeft(6, '0'));

        /// <summary>
        ///     Returns the static registration for the given type if it exists.
        /// </summary>
        /// <param name="type">ILoquiObject type that has StaticRegistration property.</param>
        /// <returns>StaticRegistration value if property exists on type, else null.</returns>
        public static ILoquiRegistration? GetStaticRegistration (Type type) => TryGetStaticRegistration(type, out var reg) ? reg : null;

        /// <summary>
        ///     Is this record context the master
        /// </summary>
        public static bool IsMaster (this IModContext<IMajorRecordGetter> context) => context.ModKey.Equals(context.Record.FormKey.ModKey);

        /// <summary>
        ///     Checks that string meets the requirements for a valid EditorID
        /// </summary>
        public static bool IsValidEditorID (this string? editorID, bool allowSomeBadChars = false, char[]? allowedPrefixes = null)
        {
            if (string.IsNullOrEmpty(editorID))
                return false;

            if (allowedPrefixes != null && allowedPrefixes.Contains(editorID[0]))
                editorID = editorID[1..];

            return allowSomeBadChars ? EditorIDAcceptable().IsMatch(editorID) : EditorIDValid().IsMatch(editorID);
        }

        /// <inheritdoc cref="TryConvertToBethesdaID(string, char[], out FormID, out FormKey, out string, out char?)" />
        [Obsolete("Use TryConvertToBethesdaID with RecordID instead.")]
        public static IDType TryConvertToBethesdaID (string input, out FormKey formKey, out string editorID) => TryConvertToBethesdaID(input, null, out formKey, out editorID, out _);

        /// <inheritdoc cref="TryConvertToBethesdaID(string, char[], out FormID, out FormKey, out string, out char?)" />
        [Obsolete("Use TryConvertToBethesdaID with RecordID instead.")]
        public static IDType TryConvertToBethesdaID (string input, char[]? allowedPrefixes, out FormKey formKey, out string editorID, out char? prefix) => TryConvertToBethesdaID(input, allowedPrefixes, out _, out formKey, out editorID, out prefix);

        /// <inheritdoc cref="TryConvertToBethesdaID(string, char[], out FormID, out FormKey, out string, out char?)" />
        [Obsolete("Use TryConvertToBethesdaID with RecordID instead.")]
        public static IDType TryConvertToBethesdaID (string input, out FormID formID, out FormKey formKey, out string editorID) => TryConvertToBethesdaID(input, null, out formID, out formKey, out editorID, out _);

        /// <inheritdoc cref="TryConvertToBethesdaID(string, char[], out RecordID, out char?)" />
        public static IDType TryConvertToBethesdaID (string input, out RecordID id) => TryConvertToBethesdaID(input, null, out id, out _);

        /// <summary>
        ///     Attempts to convert a string to a RecordID.
        /// </summary>
        /// <param name="input">Input string that is either FormKey or EditorID</param>
        /// <param name="allowedPrefixes">Allowed prefixes for input</param>
        /// <param name="id">
        ///     Record ID. Use <see cref="RecordID.Type" /> to see what type of ID.
        /// </param>
        /// <param name="prefix">Prefix if input had one</param>
        /// <returns><see cref="RecordID.Type" /> for easy switch statements.</returns>
        public static IDType TryConvertToBethesdaID (string input, char[]? allowedPrefixes, out RecordID id, out char? prefix)
        {
            if (allowedPrefixes != null && allowedPrefixes.Contains(input[0]))
            {
                prefix = input[0];
                input = input[1..];
            }
            else
            {
                prefix = null;
            }

            if (FormID.TryFactory(input, out var formID))
            {
                id = formID;
                return id.Type;
            }

            if (FormKey.TryFactory(FixFormKey(input), out var formKey))
            {
                id = formKey;
                return id.Type;
            }

            if (ModKey.TryFromNameAndExtension(input, out var modKey))
            {
                id = modKey;
                return id.Type;
            }

            if (input.IsValidEditorID(true))
            {
                id = new RecordID(input);
                return id.Type;
            }

            id = new RecordID(IDType.Invalid, input);
            return IDType.Invalid;
        }

        /// <summary>
        ///     Attempts to convert a string to a FormKey or EditorID
        /// </summary>
        /// <param name="input">Input string that is either FormKey or EditorID</param>
        /// <param name="allowedPrefixes">Allowed prefixes for input</param>
        /// <param name="formID">FormID if return value is IDType.FormID</param>
        /// <param name="formKey">FormKey if return value is IDType.FormKey</param>
        /// <param name="editorID">
        ///     Null if return value != IDType.EditorID, else the EditorID as a string
        /// </param>
        /// <param name="prefix">Prefix if input had one</param>
        /// <returns>
        ///     IDType value depending on if input was FormID, FormKey or EditorID. IDType.Invalid
        ///     if nothing valid found.
        /// </returns>
        [Obsolete("Use TryConvertToBethesdaID with RecordID instead.")]
        public static IDType TryConvertToBethesdaID (string input, char[]? allowedPrefixes, out FormID formID, out FormKey formKey, out string editorID, out char? prefix)
        {
            switch (TryConvertToBethesdaID(input, allowedPrefixes, out var id, out prefix))
            {
                case IDType.FormID:
                    formID = id.FormID;
                    formKey = default;
                    editorID = null!;
                    return IDType.FormID;

                case IDType.FormKey:
                    formID = FormID.Null;
                    formKey = id.FormKey;
                    editorID = null!;
                    return IDType.FormKey;

                case IDType.ModKey:
                    formID = FormID.Null;
                    formKey = new FormKey(id.ModKey, 0xFFFFFF);
                    editorID = null!;
                    return IDType.ModKey;

                case IDType.EditorID:
                    formID = FormID.Null;
                    formKey = FormKey.Null;
                    editorID = id.EditorID;
                    return IDType.EditorID;

                default:
                    formID = FormID.Null;
                    formKey = FormKey.Null;
                    editorID = null!;
                    return IDType.Invalid;
            }
        }

        /// <summary>
        ///     Attempt to return the record identified by either FormKey or EditorID
        /// </summary>
        /// <typeparam name="T">Record Type</typeparam>
        /// <param name="id">String of FormKey or EditorID</param>
        /// <param name="linkCache">Used to search for record</param>
        /// <param name="record">The record if found</param>
        /// <returns>true if found else false</returns>
        public static bool TryGetRecord<T> (string id, ILinkCache<IMod, IModGetter> linkCache, [NotNullWhen(true)] out T? record)
            where T : class, IMajorRecordQueryableGetter
        {
            record = null;

            return TryConvertToBethesdaID(id, out var recordID) switch
            {
                IDType.FormKey => linkCache.TryResolve<T>(recordID.FormKey, out record),
                IDType.EditorID => linkCache.TryResolve<T>(recordID.EditorID, out record),
                _ => false,
            };
        }

        /// <summary>
        ///     Attempt to return the winning record context identified by either FormKey or
        ///     EditorID
        /// </summary>
        /// <typeparam name="TMajor">Major Record Type</typeparam>
        /// <typeparam name="TMajorGetter">Major Record Getter Type</typeparam>
        /// <param name="id">String of FormKey or EditorID</param>
        /// <param name="linkCache">Used to search for record</param>
        /// <param name="record">The record if found</param>
        /// <returns>true if found else false</returns>
        public static bool TryGetRecordContext<TMajor, TMajorGetter> (string id, ILinkCache<IMod, IModGetter> linkCache, [NotNullWhen(true)] out IModContext<IMod, IModGetter, TMajor, TMajorGetter>? record)
            where TMajor : class, IMajorRecordQueryable, TMajorGetter
            where TMajorGetter : class, IMajorRecordQueryableGetter
        {
            record = null;

            return TryConvertToBethesdaID(id, out var recordID) switch
            {
                IDType.FormKey => linkCache.TryResolveContext(recordID.FormKey, out record),
                IDType.EditorID => linkCache.TryResolveContext(recordID.EditorID, out record),
                _ => false,
            };
        }

        /// <summary>
        ///     Returns the static registration for the given type if it exists.
        /// </summary>
        /// <param name="type">ILoquiObject type that has StaticRegistration property.</param>
        /// <param name="registration">StaticRegistration value it found</param>
        /// <returns>
        ///     true if found, false if invalid type without StaticRegistration property
        /// </returns>
        public static bool TryGetStaticRegistration (Type type, [NotNullWhen(true)] out ILoquiRegistration? registration)
        {
            registration = null;
            if (type.IsGenericTypeDefinition)
                return false;

            var sr = type.GetProperty("StaticRegistration", BindingFlags.Public | BindingFlags.Static);
            registration = sr?.GetValue(null) as ILoquiRegistration;
            return registration is not null;
        }

        /// <summary>
        ///     Going by https://en.uesp.net/wiki/Skyrim_Mod:SkyEdit/User_Interface/Common_Fields
        ///     should not include space or underscores, but seems to be used sometimes.
        /// </summary>
        [GeneratedRegex(@"^[a-zA-Z0-9_ ]+$")]
        private static partial Regex EditorIDAcceptable ();

        [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
        private static partial Regex EditorIDValid ();

        [GeneratedRegex(@"^[0-9A-Fa-f]{1,6}")]
        private static partial Regex RegexFormKey ();
    }
}