using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class DataLayerLocalizationConsistencyTests
    {
        [Test]
        public void ScriptableObjectDefinitions_DoNotContainReplacementCharacters()
        {
            string dir = Path.Combine(Application.dataPath, "INTIFALL/ScriptableObjects");
            Assert.IsTrue(Directory.Exists(dir), $"Directory not found: {dir}");

            string[] files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
            Assert.Greater(files.Length, 0, "No ScriptableObject definition files found.");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                Assert.IsFalse(content.Contains('\uFFFD'), $"Found replacement character in {file}");
            }
        }
    }
}
