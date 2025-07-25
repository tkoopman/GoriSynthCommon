using System.Text;

using Mutagen.Bethesda.Plugins;

using static Common.JsonConverters.FormKeyConverter;

namespace Common
{
    /// <summary>
    ///     FormKey extensions
    /// </summary>
    public static class FormKeyExt
    {
        /// <summary>
        ///     Convert FormKey to a string representation using given format options.
        /// </summary>
        public static string ToString (this FormKey formKey, Format format = Format.Default)
        {
            var sb = new StringBuilder();
            if (format.HasFlag(Format.HexPrefix))
                _ = sb.Append("0x");

            _ = sb.Append(formKey.ID.ToString(format.HasFlag(Format.RemoveLeadingZeros) ? "X" : "X6"));
            _ = sb.Append(format.HasFlag(Format.SeparatorTilde) ? '~' : ':');
            _ = sb.Append(formKey.ModKey);

            return sb.ToString();
        }
    }
}