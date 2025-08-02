using Common;

using Mutagen.Bethesda.Plugins;

using Reloaded.Memory.Extensions;

using Xunit.Abstractions;

namespace CommonTests
{
    public class IndexedRecordIDsTests (ITestOutputHelper output)
    {
        public static readonly char[] ValidChars = [.. "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789 abcdefghijklmnopqrstuvwxyz"];

        public static readonly List<string> Words =
        [
            "cats", "dogs", "iron", "fence", "sword", "shirt", "boots", "head", "skyrim", "fallout"
        ];

        private readonly ITestOutputHelper Output = output;

        [Fact]
        public void Test_ModKeyIndex ()
        {
            var index = new IndexedRecordIDs<string>();
            index.Add(new RecordID(new ModKey("Skyrim", ModType.Master), RecordID.EqualsOptions.ModKey), "Skyrim");
            index.Add(new RecordID(new ModKey("Dawnguard", ModType.Master), RecordID.EqualsOptions.ModKey), "Dawnguard");
            index.Add(new RecordID(new ModKey("Dragonborn", ModType.Master), RecordID.EqualsOptions.ModKey), "Dragonborn");
            index.Add(new RecordID(new ModKey("BSHeartland", ModType.Master), RecordID.EqualsOptions.ModKey), "BSHeartland");
            index.Add(new RecordID(new ModKey("ForgottenCity", ModType.Master), RecordID.EqualsOptions.ModKey), "ForgottenCity");

            var r = index.InternalFindAll(new FormKey(new ModKey("Dragonborn", ModType.Master), 0x1).ModKey, RecordID.Field.ModKey);
            Assert.NotEmpty(r);
            Assert.Equal("Dragonborn", r.First().value);
        }

        [Fact]
        public void Test_WildIndex ()
        {
            List<string> tryFind = [];
            var index = new IndexedRecordIDs<string>();
            var rnd = new Random();

            for (int w = 0; w < Words.Count; w++)
            {
                bool addTwice = w % 2 == 0;
                string word = Words[w];

                tryFind.Add(word);

                if (addTwice)
                    tryFind.Add(word);

                for (int i = 0; i < 100000; i++)
                {
                    int len = rnd.Next(4, 16);
                    char[] chars = new char[len];
                    for (int l = 0; l < len; l++)
                        chars[l] = ValidChars[rnd.Next(ValidChars.Length)];

                    string str = string.Join(null, chars);
                    tryFind.Add($"{str}{word}");
                    tryFind.Add($"{word}{str}");
                }
            }

            // Now we have list generated let us actually load into index
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            foreach (string str in tryFind)
            {
                index.Add(new RecordID(IDType.Name, str, true, RecordID.EqualsOptions.EditorID), $"{str}{count++}");
            }

            watch.Stop();
            Output.WriteLine($"Load Rules: {watch.Elapsed}");
            watch.Restart();

            // Sort index
            index.SortIndex();

            watch.Stop();
            Output.WriteLine($"Sort Index: {watch.Elapsed}");

            index.PrintStats();

            List<string> random = [];

            for (int i = 0; i < 100; i++)
            {
                int len = rnd.Next(4, 16);
                char[] chars = new char[len];
                for (int l = 0; l < len; l++)
                    chars[l] = ValidChars[rnd.Next(ValidChars.Length)];

                string str = string.Join(null, chars);
                if (!Words.Any(w => str.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    random.Add(str);
            }

            watch.Restart();
            for (int w = 0; w < Words.Count; w++)
            {
                bool addTwice = w % 2 == 0;
                int expect = addTwice ? 2 : 1;

                count = index.InternalFindAllGrouped(Words[w], RecordID.Field.EditorID).Count;
                Assert.Equal(expect, count);

                for (int i = 0; i < 1000; i++)
                {
                    string r = random[rnd.Next(random.Count)];
                    string wordA = $"{Words[w]}{r}";
                    string wordB = $"{r}{Words[w]}";

                    switch (i % 3)
                    {
                        case 0:
                            break;

                        case 1:
                            wordA = wordA.ToLowerInvariantFast();
                            wordB = wordB.ToLowerInvariantFast();
                            break;

                        case 2:
                            wordA = wordA.ToUpperInvariantFast();
                            wordB = wordB.ToUpperInvariantFast();
                            break;
                    }

                    count = index.InternalFindAllGrouped($"{Words[w]}{r}", RecordID.Field.Name).Count;
                    Assert.Equal(0, count);
                    count = index.InternalFindAllGrouped($"{r}{Words[w]}", RecordID.Field.EditorID).Count;
                    Assert.True(expect <= count);
                }
            }

            watch.Stop();
            Output.WriteLine($"{Words.Count * 2}k Searches {watch.Elapsed}");
        }
    }
}