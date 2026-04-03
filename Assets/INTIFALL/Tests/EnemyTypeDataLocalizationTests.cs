using INTIFALL.AI;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class EnemyTypeDataLocalizationTests
    {
        [Test]
        public void DefaultData_UsesEnglishAsPrimaryDisplayName()
        {
            EnemyTypeData data = EnemyTypeData.GetDefaultData(EEnemyType.Normal);

            Assert.AreEqual("Guard", data.displayName);
            Assert.AreEqual("Guard", data.displayNameEnglish);
            Assert.AreEqual("普通士兵", data.displayNameChinese);
            Assert.IsFalse(string.IsNullOrWhiteSpace(data.localizationKey));

            Object.DestroyImmediate(data);
        }

        [Test]
        public void GetDisplayName_ResolvesByLanguage()
        {
            EnemyTypeData data = EnemyTypeData.GetDefaultData(EEnemyType.Heavy);

            Assert.AreEqual("Heavy Guard", data.GetDisplayName(SystemLanguage.English));
            Assert.AreEqual("重型兵", data.GetDisplayName(SystemLanguage.ChineseSimplified));

            Object.DestroyImmediate(data);
        }

        [Test]
        public void DefaultData_AllEnemyTypes_HaveLocalizationKeys()
        {
            foreach (EEnemyType type in global::System.Enum.GetValues(typeof(EEnemyType)))
            {
                EnemyTypeData data = EnemyTypeData.GetDefaultData(type);
                Assert.IsFalse(string.IsNullOrWhiteSpace(data.localizationKey), $"Missing localizationKey for {type}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(data.displayNameEnglish), $"Missing displayNameEnglish for {type}");
                Object.DestroyImmediate(data);
            }
        }
    }
}
