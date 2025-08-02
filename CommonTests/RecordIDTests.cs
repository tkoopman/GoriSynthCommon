using Common;

using Mutagen.Bethesda.Plugins;

namespace CommonTests
{
    public class RecordIDTests
    {
        [Fact]
        public void Test_EqualsOptions ()
        {
            var d = default(RecordID.EqualsOptions);
            Assert.Equal((RecordID.EqualsOptions)0, d);
        }

        [Fact]
        public void Test_RecordID_EditorID ()
        {
            var recordID = new RecordID("TestRecord");
            string str = recordID;
            Assert.Equal(IDType.Name, recordID.Type);
            Assert.Equal("TestRecord", str);
        }

        [Fact]
        public void Test_RecordID_FormID ()
        {
            var recordID = new RecordID(new FormID(0x12345678));
            string str = recordID;
            Assert.Equal(IDType.FormID, recordID.Type);
            Assert.Equal("0x12345678", str);
        }

        [Fact]
        public void Test_RecordID_FormKey ()
        {
            var recordID = new RecordID(new FormKey("Skyrim.esm", 0x123456));
            string str = recordID;
            Assert.Equal(IDType.FormKey, recordID.Type);
            Assert.Equal("123456:Skyrim.esm", str);
        }

        [Fact]
        public void Test_RecordID_Invalid ()
        {
            var recordID = new RecordID();
            string str = recordID;
            Assert.Equal(IDType.Invalid, recordID.Type);
            Assert.Equal(string.Empty, str);
        }

        [Fact]
        public void Test_RecordID_ModKey ()
        {
            var recordID = new RecordID(new ModKey("Skyrim", ModType.Master));
            string str = recordID;
            Assert.Equal(IDType.ModKey, recordID.Type);
            Assert.Equal("Skyrim.esm", str);
        }
    }
}