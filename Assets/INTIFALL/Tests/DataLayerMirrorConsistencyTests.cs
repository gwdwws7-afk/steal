using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class DataLayerMirrorConsistencyTests
    {
        [Test]
        public void RuntimeMirrorAssets_ResourcesAndScriptableObjects_AreInSync()
        {
            AssertMirrorDirectory("Levels");
            AssertMirrorDirectory("Spawns");
        }

        private static void AssertMirrorDirectory(string category)
        {
            string resourcesDir = Path.Combine(Application.dataPath, "Resources", "INTIFALL", category);
            string mirrorDir = Path.Combine(Application.dataPath, "INTIFALL", "ScriptableObjects", category);

            Assert.IsTrue(Directory.Exists(resourcesDir), $"Resources dir missing: {resourcesDir}");
            Assert.IsTrue(Directory.Exists(mirrorDir), $"Mirror dir missing: {mirrorDir}");

            Dictionary<string, string> resourceFiles = GetAssetFileMap(resourcesDir);
            Dictionary<string, string> mirrorFiles = GetAssetFileMap(mirrorDir);

            Assert.AreEqual(resourceFiles.Count, mirrorFiles.Count,
                $"Asset count mismatch in category '{category}'.");

            foreach (KeyValuePair<string, string> entry in resourceFiles)
            {
                Assert.IsTrue(mirrorFiles.TryGetValue(entry.Key, out string mirrorPath),
                    $"Missing mirrored asset in '{category}': {entry.Key}");

                string resourceContent = NormalizeYaml(File.ReadAllText(entry.Value));
                string mirrorContent = NormalizeYaml(File.ReadAllText(mirrorPath));
                Assert.AreEqual(resourceContent, mirrorContent,
                    $"Mirrored asset drift detected in '{category}/{entry.Key}'.");
            }

            foreach (string mirrorOnlyName in mirrorFiles.Keys.Except(resourceFiles.Keys))
            {
                Assert.Fail($"Unexpected mirrored asset in '{category}': {mirrorOnlyName}");
            }
        }

        private static Dictionary<string, string> GetAssetFileMap(string directory)
        {
            return Directory
                .GetFiles(directory, "*.asset", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path)
                .ToDictionary(path => Path.GetFileName(path), path => path);
        }

        private static string NormalizeYaml(string content)
        {
            return content.Replace("\r\n", "\n").Trim();
        }
    }
}
