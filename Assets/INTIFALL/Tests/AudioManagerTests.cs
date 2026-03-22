using NUnit.Framework;
using INTIFALL.Audio;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class AudioManagerTests
    {
        private AudioManager _audio;
        private GameObject _go;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("AudioManager");
            _audio = _go.AddComponent<AudioManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void MasterVolume_InitiallyOne()
        {
            Assert.AreEqual(1f, _audio.MasterVolume);
        }

        [Test]
        public void SFXVolume_InitiallyOne()
        {
            Assert.AreEqual(1f, _audio.SFXVolume);
        }

        [Test]
        public void MusicVolume_InitiallyHalf()
        {
            Assert.AreEqual(0.5f, _audio.MusicVolume);
        }

        [Test]
        public void AmbientVolume_InitiallyThreeTenths()
        {
            Assert.AreEqual(0.3f, _audio.AmbientVolume);
        }

        [Test]
        public void SetMasterVolume_ClampsValue()
        {
            _audio.SetMasterVolume(1.5f);
            Assert.AreEqual(1f, _audio.MasterVolume);
        }

        [Test]
        public void SetSFXVolume_ClampsValue()
        {
            _audio.SetSFXVolume(-0.5f);
            Assert.AreEqual(0f, _audio.SFXVolume);
        }
    }
}