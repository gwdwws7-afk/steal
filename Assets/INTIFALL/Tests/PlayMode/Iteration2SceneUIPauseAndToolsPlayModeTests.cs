using System.Collections;
using INTIFALL.Level;
using INTIFALL.System;
using INTIFALL.Tools;
using INTIFALL.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class Iteration2SceneUIPauseAndToolsPlayModeTests
    {
        private const string SmokeToolName = "SmokePlayModeTool";

        private static readonly string[] CoreSceneNames =
        {
            "Level01_Qhapaq_Passage",
            "Level02_Temple_Complex",
            "Level03_Underground_Labs",
            "Level04_Qhipu_Core",
            "Level05_General_Taki_Villa"
        };

        [UnityTest]
        public IEnumerator CoreScenes_PauseHudAndToolLoop_WorksForSmoke()
        {
            for (int i = 0; i < CoreSceneNames.Length; i++)
            {
                string sceneName = CoreSceneNames[i];
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                var flow = Object.FindFirstObjectByType<LevelFlowManager>();
                if (flow != null)
                    Object.Destroy(flow.gameObject);

                var pauseMenu = Object.FindFirstObjectByType<PauseMenuUI>();
                Assert.IsNotNull(pauseMenu, $"PauseMenuUI missing in {sceneName}");

                var hudManager = Object.FindFirstObjectByType<HUDManager>();
                Assert.IsNotNull(hudManager, $"HUDManager missing in {sceneName}");

                GameObject player = TryFindPlayer();
                Assert.IsNotNull(player, $"Player missing in {sceneName}");

                var toolManager = player.GetComponent<ToolManager>();
                if (toolManager == null)
                    toolManager = Object.FindFirstObjectByType<ToolManager>();
                Assert.IsNotNull(toolManager, $"ToolManager missing in {sceneName}");

                int pausedEvents = 0;
                int resumedEvents = 0;
                int equippedEvents = 0;
                int usedEvents = 0;

                global::System.Action<PauseMenuUI.GamePausedEvent> onPaused = _ => pausedEvents++;
                global::System.Action<PauseMenuUI.GameResumedEvent> onResumed = _ => resumedEvents++;
                global::System.Action<ToolEquippedEvent> onEquipped = evt =>
                {
                    if (evt.toolName == SmokeToolName)
                        equippedEvents++;
                };
                global::System.Action<ToolUsedEvent> onUsed = evt =>
                {
                    if (evt.toolName == SmokeToolName)
                        usedEvents++;
                };

                EventBus.Subscribe(onPaused);
                EventBus.Subscribe(onResumed);
                EventBus.Subscribe(onEquipped);
                EventBus.Subscribe(onUsed);

                GameObject mockToolPrefab = null;
                ToolData toolData = null;

                try
                {
                    Time.timeScale = 1f;

                    bool wasVisible = hudManager.IsVisible;
                    hudManager.ToggleHUD();
                    Assert.AreNotEqual(wasVisible, hudManager.IsVisible, $"HUD toggle failed in {sceneName}");

                    hudManager.ToggleHUD();
                    Assert.AreEqual(wasVisible, hudManager.IsVisible, $"HUD toggle restore failed in {sceneName}");

                    pauseMenu.Pause();
                    Assert.IsTrue(pauseMenu.IsPaused, $"Pause state not set in {sceneName}");
                    Assert.AreEqual(0f, Time.timeScale, $"Time scale not paused in {sceneName}");

                    pauseMenu.Resume();
                    Assert.IsFalse(pauseMenu.IsPaused, $"Pause state not cleared in {sceneName}");
                    Assert.AreEqual(1f, Time.timeScale, $"Time scale not resumed in {sceneName}");

                    Assert.GreaterOrEqual(pausedEvents, 1, $"No pause event received in {sceneName}");
                    Assert.GreaterOrEqual(resumedEvents, 1, $"No resume event received in {sceneName}");

                    mockToolPrefab = CreateMockToolPrefab();
                    toolData = CreateMockToolData(mockToolPrefab);
                    int slotIndex = FindBestSlot(toolManager);

                    toolManager.EquipTool(slotIndex, toolData);
                    yield return null;

                    toolManager.SelectTool(slotIndex);
                    Assert.AreEqual(slotIndex, toolManager.ActiveToolIndex, $"Tool slot not selected in {sceneName}");
                    Assert.IsNotNull(toolManager.ActiveTool, $"Active tool missing in {sceneName}");

                    int ammoBeforeUse = toolManager.ActiveTool.CurrentAmmo;
                    toolManager.UseActiveTool();
                    yield return null;

                    Assert.GreaterOrEqual(equippedEvents, 1, $"No tool equipped event for smoke tool in {sceneName}");
                    Assert.GreaterOrEqual(usedEvents, 1, $"No tool used event for smoke tool in {sceneName}");
                    Assert.Less(toolManager.ActiveTool.CurrentAmmo, ammoBeforeUse, $"Tool ammo did not decrease in {sceneName}");
                }
                finally
                {
                    Time.timeScale = 1f;

                    EventBus.Unsubscribe(onPaused);
                    EventBus.Unsubscribe(onResumed);
                    EventBus.Unsubscribe(onEquipped);
                    EventBus.Unsubscribe(onUsed);

                    if (toolData != null)
                        Object.Destroy(toolData);
                    if (mockToolPrefab != null)
                        Object.Destroy(mockToolPrefab);
                }
            }
        }

        private static GameObject CreateMockToolPrefab()
        {
            GameObject prefab = new GameObject("SmokePlayModeToolPrefab");
            var tool = prefab.AddComponent<SceneSmokeMockTool>();
            tool.toolName = SmokeToolName;
            tool.category = EToolCategory.AttentionShift;
            tool.defaultSlot = EToolSlot.Slot1;
            tool.maxAmmo = 2;
            tool.ammo = 1;
            tool.cooldown = 0.5f;
            return prefab;
        }

        private static ToolData CreateMockToolData(GameObject prefab)
        {
            var data = ScriptableObject.CreateInstance<ToolData>();
            data.toolName = SmokeToolName;
            data.category = EToolCategory.AttentionShift;
            data.defaultSlot = EToolSlot.Slot1;
            data.maxAmmo = 2;
            data.runtimePrefab = prefab;
            return data;
        }

        private static int FindBestSlot(ToolManager manager)
        {
            ToolBase[] tools = manager.EquippedTools;
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == null)
                    return i;
            }
            return 0;
        }

        private static GameObject TryFindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }

    public class SceneSmokeMockTool : ToolBase
    {
        protected override void OnToolUsed()
        {
        }
    }
}
