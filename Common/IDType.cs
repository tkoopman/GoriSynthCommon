namespace Common
{
    /// <summary>
    ///     Used by
    ///     <see cref="SynthCommon.TryConvertToBethesdaID(string, out Mutagen.Bethesda.Plugins.FormKey, out string)" />
    ///     to determine the type of ID detected.
    /// </summary>
    public enum IDType
    {
        /// <summary>
        ///     Value provided was not a valid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     FormID Only, no ModID provided.
        /// </summary>
        FormID = 1 << 0,

        /// <summary>
        ///     Mod ID Only, no FormID provided.
        /// </summary>
        ModID = 1 << 1,

        /// <summary>
        ///     Value provided was a valid FormKey.
        /// </summary>
        FormKey = FormID | ModID,

        /// <summary>
        ///     Value provided was a valid EditorID.
        /// </summary>
        EditorID = 1 << 2,
    }
}