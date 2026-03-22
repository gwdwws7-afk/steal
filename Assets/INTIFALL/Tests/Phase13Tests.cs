using NUnit.Framework;
using INTIFALL.Core;
using INTIFALL.UI;
using UnityEngine;

namespace INTIFALL.Tests
{
    public class Phase13Tests
    {
        [Test]
        public void SaveLoadManager_Defaults()
        {
            var go = new GameObject("SaveLoadManager");
            var manager = go.AddComponent<SaveLoadManager>();
            Assert.IsFalse(manager.HasSaveData);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PauseMenuUI_Defaults()
        {
            var go = new GameObject("PauseMenuUI");
            var pause = go.AddComponent<PauseMenuUI>();
            Assert.IsFalse(pause.IsPaused);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MainMenuUI_DoesNotCrash()
        {
            var go = new GameObject("MainMenuUI");
            var menu = go.AddComponent<MainMenuUI>();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameOverUI_DoesNotCrash()
        {
            var go = new GameObject("GameOverUI");
            var ui = go.AddComponent<GameOverUI>();
            Object.DestroyImmediate(go);
        }
    }
}