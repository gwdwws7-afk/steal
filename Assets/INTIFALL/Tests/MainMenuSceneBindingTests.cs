using System;
using INTIFALL.Core;
using INTIFALL.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace INTIFALL.Tests
{
    public class MainMenuSceneBindingTests
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [Test]
        public void MainMenuScene_MainMenuUIRequiredFields_AreBound()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
            Assert.IsNotNull(sceneAsset, $"MainMenu scene missing at '{MainMenuScenePath}'.");

            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid());

            MainMenuUI menuUI = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenu scene should contain MainMenuUI.");

            SerializedObject serialized = new SerializedObject(menuUI);
            AssertObjectReferenceBound(serialized, "mainPanel");
            AssertObjectReferenceBound(serialized, "levelSelectPanel");
            AssertObjectReferenceBound(serialized, "settingsPanel");

            AssertObjectReferenceBound(serialized, "newGameButton");
            AssertObjectReferenceBound(serialized, "continueButton");
            AssertObjectReferenceBound(serialized, "levelSelectButton");
            AssertObjectReferenceBound(serialized, "settingsButton");
            AssertObjectReferenceBound(serialized, "quitButton");

            AssertArrayBound(serialized, "levelButtons", 5);
            AssertArrayBound(serialized, "levelLockTexts", 5);

            AssertObjectReferenceBound(serialized, "masterVolumeSlider");
            AssertObjectReferenceBound(serialized, "sfxVolumeSlider");
            AssertObjectReferenceBound(serialized, "musicVolumeSlider");
            AssertObjectReferenceBound(serialized, "invertYToggle");
            AssertObjectReferenceBound(serialized, "sensitivitySlider");

            AssertArrayBound(serialized, "saveSlotButtons", SaveLoadManager.MaxSaveSlots);
            AssertArrayBound(serialized, "saveSlotStatusTexts", SaveLoadManager.MaxSaveSlots);
            AssertObjectReferenceBound(serialized, "activeSlotText");
            AssertObjectReferenceBound(serialized, "activeSlotMissionSnapshotText");
            AssertObjectReferenceBound(serialized, "slotActionFeedbackText");
            AssertObjectReferenceBound(serialized, "restoreBackupButton");
            AssertObjectReferenceBound(serialized, "deleteSlotButton");

            AssertObjectReferenceBound(serialized, "saveLoadManager");
            AssertObjectReferenceBound(serialized, "levelFlowManager");
        }

        [Test]
        public void BuildSettings_ContainsEnabledMainMenuScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Assert.Greater(scenes.Length, 0, "Build settings scene list should not be empty.");

            bool found = false;
            bool enabled = false;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!string.Equals(scenes[i].path, MainMenuScenePath, StringComparison.OrdinalIgnoreCase))
                    continue;

                found = true;
                enabled = scenes[i].enabled;
                break;
            }

            Assert.IsTrue(found, $"Build settings should include '{MainMenuScenePath}'.");
            Assert.IsTrue(enabled, "MainMenu scene should be enabled in build settings.");
        }

        [Test]
        public void MainMenuScene_MissionSnapshotText_UsesReadableBaselineStyle()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid());

            MainMenuUI menuUI = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenu scene should contain MainMenuUI.");

            SerializedObject serialized = new SerializedObject(menuUI);
            SerializedProperty snapshotProperty = serialized.FindProperty("activeSlotMissionSnapshotText");
            Assert.IsNotNull(snapshotProperty, "Missing serialized property 'activeSlotMissionSnapshotText'.");
            Assert.IsNotNull(snapshotProperty.objectReferenceValue, "'activeSlotMissionSnapshotText' should be bound.");

            Text snapshotText = snapshotProperty.objectReferenceValue as Text;
            Assert.IsNotNull(snapshotText, "'activeSlotMissionSnapshotText' should reference a Text component.");

            RectTransform snapshotRect = snapshotText.GetComponent<RectTransform>();
            Assert.IsNotNull(snapshotRect, "Mission snapshot text should have RectTransform.");
            Assert.AreEqual(980f, snapshotRect.sizeDelta.x, 0.01f, "Snapshot text width should preserve baseline.");
            Assert.AreEqual(136f, snapshotRect.sizeDelta.y, 0.01f, "Snapshot text height should preserve baseline.");
            Assert.AreEqual(236f, snapshotRect.anchoredPosition.y, 0.01f, "Snapshot text vertical layout drifted from baseline.");

            Assert.AreEqual(19, snapshotText.fontSize, "Snapshot text font size should preserve baseline readability.");
            Assert.AreEqual(TextAnchor.UpperLeft, snapshotText.alignment, "Snapshot text should be left/top aligned for multiline scan.");
            Assert.AreEqual(HorizontalWrapMode.Wrap, snapshotText.horizontalOverflow, "Snapshot text should wrap horizontally.");
            Assert.AreEqual(VerticalWrapMode.Overflow, snapshotText.verticalOverflow, "Snapshot text should allow vertical overflow.");
            Assert.AreEqual(1.15f, snapshotText.lineSpacing, 0.001f, "Snapshot text line spacing should preserve readability baseline.");
            Assert.IsFalse(snapshotText.raycastTarget, "Snapshot text should not block UI clicks.");
        }

        private static void AssertObjectReferenceBound(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.IsNotNull(property, $"Missing serialized property '{propertyName}'.");
            Assert.AreEqual(SerializedPropertyType.ObjectReference, property.propertyType, $"Property '{propertyName}' is not an object reference.");
            Assert.IsNotNull(property.objectReferenceValue, $"Property '{propertyName}' should be bound.");
        }

        private static void AssertArrayBound(SerializedObject serialized, string propertyName, int expectedLength)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.IsNotNull(property, $"Missing serialized array '{propertyName}'.");
            Assert.IsTrue(property.isArray, $"Property '{propertyName}' should be an array.");
            Assert.AreEqual(expectedLength, property.arraySize, $"Array '{propertyName}' size mismatch.");

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                Assert.IsNotNull(element.objectReferenceValue, $"Array '{propertyName}' element {i} should be bound.");
            }
        }
    }
}
