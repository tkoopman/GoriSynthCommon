using Common;

using Mutagen.Bethesda.Plugins;

namespace CommonTests
{
    public class SynthCommonTests
    {
        [Theory]
        [ClassData(typeof(Data_TryConvertToBethesdaID))]
        public void Test_TryConvertToBethesdaID (string input, char[]? allowedPrefixes, RecordID expected, char? ePrefix)
        {
            _ = Common.SynthCommon.TryConvertToBethesdaID(input, allowedPrefixes, convertFormID, out var recordID, out char? prefix);
            Assert.Equal(expected, recordID);
            Assert.Equal(ePrefix, prefix);
        }

        private FormKey convertFormID (FormID formID)
        {
            uint modID = formID.MasterIndex(MasterStyle.Full);

            string? modName = modID switch
            {
                0x00u => "Skyrim",
                0x01u => "Update",
                0x02u => "Dawnguard",
                0x03u => "HearthFires",
                0x04u => "Dragonborn",
                _ => null,
            };

            if (modName is null)
                return FormKey.Null;

            return new FormKey(new ModKey(modName, ModType.Master), formID.Id(MasterStyle.Full));
        }

        public class Data_TryConvertToBethesdaID : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[]
                {   // convertFormID returns FormKey.Null so just keeps as FormID
                    "0A000001", null, new RecordID(new FormID(0xA000001)), null
                };
                yield return new object?[]
                {   // When providing a FormID must include all leading 0's else won't be detected as FormID but technically could be EditorID
                    "0x1", null, new RecordID(IDType.EditorID, "0x1"), null
                };
                yield return new object?[]
                {   // Will be converted from FormID to FormKey by convertFormID
                    "0x0000000F", null, new RecordID(new FormKey("Skyrim.esm", 0xF)), null
                };
                yield return new object?[]
                {
                    "000101:Skyrim.esm", null, new RecordID(new FormKey("Skyrim.esm", 0x000101)), null
                };
                yield return new object?[]
                {
                    "101:Skyrim.esm", null, new RecordID(new FormKey("Skyrim.esm", 0x000101)), null
                };
                yield return new object?[]
                {
                    "0x000101~Skyrim.esm", null, new RecordID(new FormKey("Skyrim.esm", 0x000101)), null
                };
                yield return new object?[]
                {
                    "0x101~Skyrim.esm", null, new RecordID(new FormKey("Skyrim.esm", 0x000101)), null
                };
                yield return new object?[]
                {
                    "0x000101:Skyrim.esm", null, new RecordID(new FormKey("Skyrim.esm", 0x000101)), null
                };
                yield return new object?[]
                {   // FormKeys can not include the Mod ID in the FormID (6 HEX digits max)
                    "00000001:Skyrim.esm", null, new RecordID(IDType.Invalid, "00000001:Skyrim.esm"), null
                };
                yield return new object?[]
                {
                    "Skyrim.esm", null, new RecordID(new ModKey("Skyrim", ModType.Master)), null
                };
                yield return new object?[]
                {   // Not a valid file extension, so not a valid ModKey, but also not EditorID as contains period (.)
                    "Skyrim.esa", null, new RecordID(IDType.Invalid, "Skyrim.esa"), null
                };
                yield return new object?[]
                {
                    "AEditorID", null, new RecordID(IDType.EditorID, "AEditorID"), null
                };
                yield return new object?[]
                {
                    "AEditorID", (char[])['!','-','+','^','*'], new RecordID(IDType.EditorID, "AEditorID"), null
                };
                yield return new object?[]
                {
                    "-AEditorID", (char[])['!','-','+','^','*'], new RecordID(IDType.EditorID, "AEditorID"), '-'
                };
                yield return new object?[]
                {
                    "*AEditorID", (char[])['!','-','+','^','*'], new RecordID(IDType.EditorID, "AEditorID"), '*'
                };
                yield return new object?[]
                {   // Prefixes are typically not part of the allowed characters for EditorIDs
                    "-AEditorID", null, new RecordID(IDType.Invalid, "-AEditorID"), null
                };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}