using System.Reflection;
using NUnit.Framework;
using INTIFALL.Level;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class LevelLoaderTests
    {
        private LevelLoader _loader;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("LevelLoader");
            _loader = _go.AddComponent<LevelLoader>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void EnemiesSpawned_InitiallyZero()
        {
            Assert.AreEqual(0, _loader.EnemiesSpawned);
        }

        [Test]
        public void IntelSpawned_InitiallyZero()
        {
            Assert.AreEqual(0, _loader.IntelSpawned);
        }

        [Test]
        public void GetLevelData_ReturnsData()
        {
            var data = _loader.GetLevelData();
        }

        [Test]
        public void GetEnemySpawnData_ReturnsData()
        {
            var data = _loader.GetEnemySpawnData();
        }

        [Test]
        public void GetIntelSpawnData_ReturnsData()
        {
            var data = _loader.GetIntelSpawnData();
        }

        [Test]
        public void ShouldUsePlaceholderFallback_DefaultEditorMode_ReturnsTrue()
        {
            bool allowed = InvokePrivateBoolMethod("ShouldUsePlaceholderFallback");
            Assert.IsTrue(allowed);
        }

        [Test]
        public void ShouldUsePlaceholderFallback_ForcedStrictWithoutOverride_ReturnsFalse()
        {
            SetPrivateField("forceStrictRuntimeValidationInEditor", true);
            SetPrivateField("allowPlaceholderFallbackInStrictRuntime", false);

            bool allowed = InvokePrivateBoolMethod("ShouldUsePlaceholderFallback");
            Assert.IsFalse(allowed);
        }

        [Test]
        public void ShouldUsePlaceholderFallback_ForcedStrictWithOverride_ReturnsTrue()
        {
            SetPrivateField("forceStrictRuntimeValidationInEditor", true);
            SetPrivateField("allowPlaceholderFallbackInStrictRuntime", true);

            bool allowed = InvokePrivateBoolMethod("ShouldUsePlaceholderFallback");
            Assert.IsTrue(allowed);
        }

        private bool InvokePrivateBoolMethod(string methodName)
        {
            MethodInfo method = typeof(LevelLoader).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing private method '{methodName}'.");
            return (bool)method.Invoke(_loader, null);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            FieldInfo field = typeof(LevelLoader).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}'.");
            field.SetValue(_loader, value);
        }
    }
}
