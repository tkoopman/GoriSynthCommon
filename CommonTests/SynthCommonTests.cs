using Common;

using Mutagen.Bethesda.Plugins;

namespace CommonTests
{
    public class SynthCommonTests
    {
        [Theory]
        [ClassData(typeof(Data_TryConvertToBethesdaID))]
        public void Test_TryConvertToBethesdaID (string input, char[]? allowedPrefixes, IDType eType, FormID eFormID, FormKey eFormKey, string? eEditorID, char? ePrefix)
        {
            var result = Common.SynthCommon.TryConvertToBethesdaID(input, allowedPrefixes, out var formID, out var formKey, out string? editorID, out char? prefix);
            Assert.Equal(eType, result);
            Assert.Equal(eFormID, formID);
            Assert.Equal(eFormKey, formKey);
            Assert.Equal(eEditorID, editorID);
            Assert.Equal(ePrefix, prefix);
        }

        public class Data_TryConvertToBethesdaID : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[]
                {
                    "00000001", null, IDType.FormID, new FormID(0x1), default, null, null
                };
                yield return new object?[]
                {   // When providing a FormID must include all leading 0's else won't be detected as FormID but technically could be EditorID
                    "0x1", null, IDType.EditorID, default, default, "0x1", null
                };
                yield return new object?[]
                {
                    "0x0000000F", null, IDType.FormID, new FormID(0xF), default, null, null
                };
                yield return new object?[]
                {
                    "000101:Skyrim.esm", null, IDType.FormKey, default, new FormKey("Skyrim.esm", 0x000101), null, null
                };
                yield return new object?[]
                {
                    "101:Skyrim.esm", null, IDType.FormKey, default, new FormKey("Skyrim.esm", 0x000101), null, null
                };
                yield return new object?[]
                {   // FormKeys do not support 0x prefix but as contains : also not valid EditorID
                    "0x000101:Skyrim.esm", null, IDType.Invalid, default, default, null, null
                };
                yield return new object?[]
                {   // FormKeys can not include the Mod ID in the FormID (6 HEX digits max)
                    "00000001:Skyrim.esm", null, IDType.Invalid, default, default, null, null
                };
                yield return new object?[]
                {
                    "Skyrim.esm", null, IDType.ModID, default, new FormKey("Skyrim.esm", 0xFFFFFF), null, null
                };
                yield return new object?[]
                {   // Not a valid file extension, so not a valid ModKey, but also not EditorID as contains period (.)
                    "Skyrim.esa", null, IDType.Invalid, default, default, null, null
                };
                yield return new object?[]
                {
                    "AEditorID", null, IDType.EditorID, default, default, "AEditorID", null
                };
                yield return new object?[]
                {
                    "AEditorID", (char[])['!','-','+','^','*'], IDType.EditorID, default, default, "AEditorID", null
                };
                yield return new object?[]
                {
                    "-AEditorID", (char[])['!','-','+','^','*'], IDType.EditorID, default, default, "AEditorID", '-'
                };
                yield return new object?[]
                {
                    "*AEditorID", (char[])['!','-','+','^','*'], IDType.EditorID, default, default, "AEditorID", '*'
                };
                yield return new object?[]
                {   // Prefixes are typically not part of the allowed characters for EditorIDs
                    "-AEditorID", null, IDType.Invalid, default, default, null, null
                };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}