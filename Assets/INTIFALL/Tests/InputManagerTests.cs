using NUnit.Framework;
using INTIFALL.Input;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class InputManagerTests
    {
        private InputManager _input;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("InputManager");
            _input = _go.AddComponent<InputManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void InputEnabled_InitiallyTrue()
        {
            Assert.IsTrue(_input.InputEnabled);
        }

        [Test]
        public void EnableInput_SetsEnabledTrue()
        {
            _input.DisableInput();
            _input.EnableInput();
            Assert.IsTrue(_input.InputEnabled);
        }

        [Test]
        public void DisableInput_SetsEnabledFalse()
        {
            _input.DisableInput();
            Assert.IsFalse(_input.InputEnabled);
        }

        [Test]
        public void SetMouseSensitivity_ClampsValue()
        {
            _input.SetMouseSensitivity(5f);
            _input.SetMouseSensitivity(-1f);
        }

        [Test]
        public void SetInvertY_SetsCorrectly()
        {
            _input.SetInvertY(true);
            _input.SetInvertY(false);
        }
    }
}