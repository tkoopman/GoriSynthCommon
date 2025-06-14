namespace Common
{
    /// <summary>
    ///     Used by
    ///     <see cref="SynthCommon.TryConvertToSkyrimID(string, out Mutagen.Bethesda.Plugins.FormKey, out string)" />
    ///     to determine the type of ID detected."/&gt;
    /// </summary>
    public enum IDType
    {
        /// <summary>
        ///     Value provided was not a valid FormKey or EditorID.
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     Value provided was a valid FormKey.
        /// </summary>
        FormKey = 1,

        /// <summary>
        ///     Value provided was a valid EditorID.
        /// </summary>
        EditorID = 2,
    }
}