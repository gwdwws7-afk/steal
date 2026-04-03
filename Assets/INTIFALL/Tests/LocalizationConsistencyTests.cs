using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LocalizationConsistencyTests
    {
        private static readonly Regex CjkRegex = new("[\u4e00-\u9fff]", RegexOptions.Compiled);

        [Test]
        public void RuntimeScripts_DoNotContainMixedCjkLiterals()
        {
            string runtimeDir = Path.Combine(Application.dataPath, "INTIFALL/Scripts/Runtime");
            Assert.IsTrue(Directory.Exists(runtimeDir), $"Runtime directory not found: {runtimeDir}");

            string[] files = Directory.GetFiles(runtimeDir, "*.cs", SearchOption.AllDirectories);
            Assert.Greater(files.Length, 0, "No runtime scripts found.");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                Assert.IsFalse(CjkRegex.IsMatch(content), $"Found CJK text in runtime script: {file}");
            }
        }
    }
}
