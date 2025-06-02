using System.Collections;

namespace CommonTests
{
    internal class TranslationMaskConverter_TestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator ()
        {
            yield return new object[]
            {
                "true",
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(true)
            };

            yield return new object[]
            {
                "false",
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(false)
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": true
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(true)
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": false
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(false)
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": true,
                    "OnOveralL": true
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(true)
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": false,
                    "OnOveralL": false
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(false, false)
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": true,
                    "OnOveralL": false,
                    "ObjectEffect": false,
                    "FormVersion": false,
                    "ObjectBounds": false,
                    "Model": {
                    "DefaultOn": true,
                    "OnOverall": false
                    }
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(true, false)
                {
                    ObjectEffect = false,
                    FormVersion = false,
                    ObjectBounds = new Mutagen.Bethesda.Skyrim.ObjectBounds.TranslationMask(false),
                    Model = new Mutagen.Bethesda.Skyrim.Model.TranslationMask(true, false)
                }
            };

            yield return new object[]
            {
                """
                {
                    "defaulton": true,
                    "OnOveralL": false,
                    "ObjectEffect": false,
                    "FormVersion": false,
                    "ObjectBounds": true,
                    "Model": {
                    "DefaultOn": true
                    }
                }
                """,
                new Mutagen.Bethesda.Skyrim.Weapon.TranslationMask(true, false)
                {
                    ObjectEffect = false,
                    FormVersion = false,
                    ObjectBounds = new Mutagen.Bethesda.Skyrim.ObjectBounds.TranslationMask(true),
                    Model = new Mutagen.Bethesda.Skyrim.Model.TranslationMask(true, true)
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}