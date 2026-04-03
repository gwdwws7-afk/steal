using System.Reflection;
using INTIFALL.Input;
using NUnit.Framework;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class InputCompatTests
    {
        [Test]
        public void InputCompat_ReturnsSafeDefaults_WhenLegacyInputMarkedUnavailable()
        {
            FieldInfo legacyFlag = typeof(InputCompat).GetField(
                "_legacyInputAvailable",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(legacyFlag, "Could not access InputCompat legacy-input flag.");
            bool originalValue = (bool)legacyFlag.GetValue(null);

            try
            {
                legacyFlag.SetValue(null, false);

                Assert.DoesNotThrow(() =>
                {
                    bool key = InputCompat.GetKey(KeyCode.Space);
                    bool keyDown = InputCompat.GetKeyDown(KeyCode.Space);
                    bool keyUp = InputCompat.GetKeyUp(KeyCode.Space);
                    float axis = InputCompat.GetAxis("Horizontal");
                    float axisRaw = InputCompat.GetAxisRaw("Vertical");
                    Vector3 mouse = InputCompat.MousePosition;

                    Assert.IsFalse(key);
                    Assert.IsFalse(keyDown);
                    Assert.IsFalse(keyUp);
                    Assert.AreEqual(0f, axis);
                    Assert.AreEqual(0f, axisRaw);
                    Assert.AreEqual(Vector3.zero, mouse);
                });
            }
            finally
            {
                legacyFlag.SetValue(null, originalValue);
            }
        }
    }
}
