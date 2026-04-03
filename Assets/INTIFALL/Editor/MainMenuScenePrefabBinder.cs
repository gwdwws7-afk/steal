using System;
using System.Collections.Generic;
using INTIFALL.Core;
using INTIFALL.Level;
using INTIFALL.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace INTIFALL.Editor
{
    public static class MainMenuScenePrefabBinder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string PrefabPath = "Assets/INTIFALL/Prefabs/UI/MainMenuRoot.prefab";

        private sealed class MainMenuBuildContext
        {
            public Scene Scene;
            public GameObject Root;
            public MainMenuUI MainMenuUI;
            public GameObject MainPanel;
            public GameObject LevelSelectPanel;
            public GameObject SettingsPanel;
            public Button NewGameButton;
            public Button ContinueButton;
            public Button LevelSelectButton;
            public Button SettingsButton;
            public Button QuitButton;
            public Button[] LevelButtons;
            public Text[] LevelLockTexts;
            public Slider MasterVolumeSlider;
            public Slider SfxVolumeSlider;
            public Slider MusicVolumeSlider;
            public Toggle InvertYToggle;
            public Slider SensitivitySlider;
            public Button[] SaveSlotButtons;
            public Text[] SaveSlotStatusTexts;
            public Text ActiveSlotText;
            public Text SlotActionFeedbackText;
            public Button RestoreBackupButton;
            public Button DeleteSlotButton;
            public SaveLoadManager SaveLoadManager;
            public LevelFlowManager LevelFlowManager;
        }

        [MenuItem("INTIFALL/Iteration10/Rebuild MainMenu Scene+Prefab Binding")]
        public static void RebuildMainMenuSceneAndPrefabMenu()
        {
            RebuildMainMenuSceneAndPrefabBatch();
        }

        // Command line entry:
        // Unity.exe -batchmode -quit -projectPath <path> -executeMethod INTIFALL.Editor.MainMenuScenePrefabBinder.RebuildMainMenuSceneAndPrefabBatch
        public static void RebuildMainMenuSceneAndPrefabBatch()
        {
            EnsureFolderPath("Assets/INTIFALL/Prefabs/UI");
            Scene scene = OpenOrCreateScene();

            RemoveRootByName(scene, "MainMenuRoot");
            EnsureEventSystem(scene);

            MainMenuBuildContext context = BuildMainMenu(scene);
            BindMainMenuSerializedFields(context);
            WireLevelButtons(context);
            SavePrefab(context);
            SaveScene(context.Scene);
            EnsureMainMenuInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MainMenuBinder] MainMenu scene/prefab binding completed.");
        }

        private static Scene OpenOrCreateScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Scene created = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(created, ScenePath);
            return created;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
        }

        private static MainMenuBuildContext BuildMainMenu(Scene scene)
        {
            Font defaultFont = GetDefaultFont();

            GameObject root = new GameObject(
                "MainMenuRoot",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainMenuUI));
            SceneManager.MoveGameObjectToScene(root, scene);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            StretchToParent(rootRect);

            GameObject background = CreatePanel(
                "Background",
                root.transform,
                new Color(0.04f, 0.06f, 0.09f, 0.97f));
            StretchToParent(background.GetComponent<RectTransform>());

            GameObject mainPanel = CreatePanel(
                "MainPanel",
                background.transform,
                new Color(0.1f, 0.13f, 0.18f, 0.8f));
            StretchToParent(mainPanel.GetComponent<RectTransform>());

            GameObject levelSelectPanel = CreatePanel(
                "LevelSelectPanel",
                background.transform,
                new Color(0.09f, 0.11f, 0.16f, 0.82f));
            StretchToParent(levelSelectPanel.GetComponent<RectTransform>());
            levelSelectPanel.SetActive(false);

            GameObject settingsPanel = CreatePanel(
                "SettingsPanel",
                background.transform,
                new Color(0.09f, 0.11f, 0.15f, 0.82f));
            StretchToParent(settingsPanel.GetComponent<RectTransform>());
            settingsPanel.SetActive(false);

            CreateText("Title", mainPanel.transform, defaultFont, "INTIFALL", 64, TextAnchor.MiddleCenter, new Vector2(0f, 420f), new Vector2(900f, 90f));
            Text activeSlotText = CreateText("ActiveSlotText", mainPanel.transform, defaultFont, "Active Slot: 1", 28, TextAnchor.MiddleCenter, new Vector2(0f, 340f), new Vector2(640f, 48f));

            const int slotCount = SaveLoadManager.MaxSaveSlots;
            var saveSlotButtons = new Button[slotCount];
            var saveSlotStatusTexts = new Text[slotCount];
            for (int slot = 0; slot < slotCount; slot++)
            {
                float y = 250f - slot * 76f;
                saveSlotButtons[slot] = CreateButton($"SlotButton{slot + 1}", mainPanel.transform, defaultFont, $"Slot {slot + 1}", new Vector2(-190f, y), new Vector2(340f, 52f));
                saveSlotStatusTexts[slot] = CreateText($"SlotStatusText{slot + 1}", mainPanel.transform, defaultFont, "EMPTY", 22, TextAnchor.MiddleLeft, new Vector2(220f, y), new Vector2(500f, 48f));
            }

            Button restoreBackupButton = CreateButton("RestoreBackupButton", mainPanel.transform, defaultFont, "Restore Backup", new Vector2(-175f, 20f), new Vector2(320f, 52f));
            Button deleteSlotButton = CreateButton("DeleteSlotButton", mainPanel.transform, defaultFont, "Delete Slot", new Vector2(175f, 20f), new Vector2(320f, 52f));
            Text slotActionFeedbackText = CreateText(
                "SlotActionFeedbackText",
                mainPanel.transform,
                defaultFont,
                string.Empty,
                22,
                TextAnchor.MiddleCenter,
                new Vector2(0f, -42f),
                new Vector2(980f, 46f));

            Button continueButton = CreateButton("ContinueButton", mainPanel.transform, defaultFont, "Continue", new Vector2(0f, -60f), new Vector2(420f, 58f));
            Button newGameButton = CreateButton("NewGameButton", mainPanel.transform, defaultFont, "New Game", new Vector2(0f, -132f), new Vector2(420f, 58f));
            Button levelSelectButton = CreateButton("LevelSelectButton", mainPanel.transform, defaultFont, "Level Select", new Vector2(0f, -204f), new Vector2(420f, 58f));
            Button settingsButton = CreateButton("SettingsButton", mainPanel.transform, defaultFont, "Settings", new Vector2(0f, -276f), new Vector2(420f, 58f));
            Button quitButton = CreateButton("QuitButton", mainPanel.transform, defaultFont, "Quit", new Vector2(0f, -348f), new Vector2(420f, 58f));

            CreateText("LevelSelectTitle", levelSelectPanel.transform, defaultFont, "Level Select", 52, TextAnchor.MiddleCenter, new Vector2(0f, 400f), new Vector2(700f, 80f));

            int levelCount = 5;
            var levelButtons = new Button[levelCount];
            var levelLockTexts = new Text[levelCount];
            for (int level = 0; level < levelCount; level++)
            {
                float y = 280f - level * 96f;
                levelButtons[level] = CreateButton($"LevelButton{level + 1}", levelSelectPanel.transform, defaultFont, $"Level {level + 1}", new Vector2(-180f, y), new Vector2(420f, 62f));
                levelLockTexts[level] = CreateText($"LevelLockText{level + 1}", levelSelectPanel.transform, defaultFont, string.Empty, 24, TextAnchor.MiddleLeft, new Vector2(260f, y), new Vector2(360f, 56f));
            }

            CreateText(
                "LevelSelectHint",
                levelSelectPanel.transform,
                defaultFont,
                "Select from Main Menu to return.",
                20,
                TextAnchor.MiddleCenter,
                new Vector2(0f, -380f),
                new Vector2(700f, 44f));

            CreateText("SettingsTitle", settingsPanel.transform, defaultFont, "Settings", 52, TextAnchor.MiddleCenter, new Vector2(0f, 400f), new Vector2(700f, 80f));
            Slider masterVolumeSlider = CreateSliderRow("MasterVolumeSlider", settingsPanel.transform, defaultFont, "Master Volume", new Vector2(0f, 260f));
            Slider sfxVolumeSlider = CreateSliderRow("SfxVolumeSlider", settingsPanel.transform, defaultFont, "SFX Volume", new Vector2(0f, 170f));
            Slider musicVolumeSlider = CreateSliderRow("MusicVolumeSlider", settingsPanel.transform, defaultFont, "Music Volume", new Vector2(0f, 80f));
            Toggle invertYToggle = CreateToggleRow("InvertYToggle", settingsPanel.transform, defaultFont, "Invert Y", new Vector2(0f, -20f));
            Slider sensitivitySlider = CreateSliderRow("SensitivitySlider", settingsPanel.transform, defaultFont, "Mouse Sensitivity", new Vector2(0f, -120f));

            (SaveLoadManager saveLoadManager, LevelFlowManager levelFlowManager) = EnsureManagers(scene);

            return new MainMenuBuildContext
            {
                Scene = scene,
                Root = root,
                MainMenuUI = root.GetComponent<MainMenuUI>(),
                MainPanel = mainPanel,
                LevelSelectPanel = levelSelectPanel,
                SettingsPanel = settingsPanel,
                NewGameButton = newGameButton,
                ContinueButton = continueButton,
                LevelSelectButton = levelSelectButton,
                SettingsButton = settingsButton,
                QuitButton = quitButton,
                LevelButtons = levelButtons,
                LevelLockTexts = levelLockTexts,
                MasterVolumeSlider = masterVolumeSlider,
                SfxVolumeSlider = sfxVolumeSlider,
                MusicVolumeSlider = musicVolumeSlider,
                InvertYToggle = invertYToggle,
                SensitivitySlider = sensitivitySlider,
                SaveSlotButtons = saveSlotButtons,
                SaveSlotStatusTexts = saveSlotStatusTexts,
                ActiveSlotText = activeSlotText,
                SlotActionFeedbackText = slotActionFeedbackText,
                RestoreBackupButton = restoreBackupButton,
                DeleteSlotButton = deleteSlotButton,
                SaveLoadManager = saveLoadManager,
                LevelFlowManager = levelFlowManager
            };
        }

        private static void BindMainMenuSerializedFields(MainMenuBuildContext context)
        {
            SerializedObject serializedMainMenu = new SerializedObject(context.MainMenuUI);
            serializedMainMenu.FindProperty("mainPanel").objectReferenceValue = context.MainPanel;
            serializedMainMenu.FindProperty("levelSelectPanel").objectReferenceValue = context.LevelSelectPanel;
            serializedMainMenu.FindProperty("settingsPanel").objectReferenceValue = context.SettingsPanel;

            serializedMainMenu.FindProperty("newGameButton").objectReferenceValue = context.NewGameButton;
            serializedMainMenu.FindProperty("continueButton").objectReferenceValue = context.ContinueButton;
            serializedMainMenu.FindProperty("levelSelectButton").objectReferenceValue = context.LevelSelectButton;
            serializedMainMenu.FindProperty("settingsButton").objectReferenceValue = context.SettingsButton;
            serializedMainMenu.FindProperty("quitButton").objectReferenceValue = context.QuitButton;

            AssignObjectArray(serializedMainMenu.FindProperty("levelButtons"), context.LevelButtons);
            AssignObjectArray(serializedMainMenu.FindProperty("levelLockTexts"), context.LevelLockTexts);

            serializedMainMenu.FindProperty("masterVolumeSlider").objectReferenceValue = context.MasterVolumeSlider;
            serializedMainMenu.FindProperty("sfxVolumeSlider").objectReferenceValue = context.SfxVolumeSlider;
            serializedMainMenu.FindProperty("musicVolumeSlider").objectReferenceValue = context.MusicVolumeSlider;
            serializedMainMenu.FindProperty("invertYToggle").objectReferenceValue = context.InvertYToggle;
            serializedMainMenu.FindProperty("sensitivitySlider").objectReferenceValue = context.SensitivitySlider;

            serializedMainMenu.FindProperty("defaultSaveSlot").intValue = 0;
            AssignObjectArray(serializedMainMenu.FindProperty("saveSlotButtons"), context.SaveSlotButtons);
            AssignObjectArray(serializedMainMenu.FindProperty("saveSlotStatusTexts"), context.SaveSlotStatusTexts);
            serializedMainMenu.FindProperty("activeSlotText").objectReferenceValue = context.ActiveSlotText;
            serializedMainMenu.FindProperty("slotActionFeedbackText").objectReferenceValue = context.SlotActionFeedbackText;
            serializedMainMenu.FindProperty("restoreBackupButton").objectReferenceValue = context.RestoreBackupButton;
            serializedMainMenu.FindProperty("deleteSlotButton").objectReferenceValue = context.DeleteSlotButton;
            serializedMainMenu.FindProperty("autoContinueFromFirstAvailableSlot").boolValue = true;

            serializedMainMenu.FindProperty("levelFlowManager").objectReferenceValue = context.LevelFlowManager;
            serializedMainMenu.FindProperty("saveLoadManager").objectReferenceValue = context.SaveLoadManager;
            serializedMainMenu.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(context.MainMenuUI);
        }

        private static void WireLevelButtons(MainMenuBuildContext context)
        {
            if (context.LevelButtons == null)
                return;

            for (int i = 0; i < context.LevelButtons.Length; i++)
            {
                Button levelButton = context.LevelButtons[i];
                if (levelButton == null)
                    continue;

                while (levelButton.onClick.GetPersistentEventCount() > 0)
                    UnityEventTools.RemovePersistentListener(levelButton.onClick, 0);

                UnityEventTools.AddIntPersistentListener(levelButton.onClick, context.MainMenuUI.OnLevelButtonClicked, i);
                EditorUtility.SetDirty(levelButton);
            }
        }

        private static void SavePrefab(MainMenuBuildContext context)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(context.Root, PrefabPath, InteractionMode.AutomatedAction);
        }

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void EnsureMainMenuInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int sceneIndex = scenes.FindIndex(scene => string.Equals(scene.path, ScenePath, StringComparison.OrdinalIgnoreCase));
            if (sceneIndex >= 0)
            {
                EditorBuildSettingsScene existing = scenes[sceneIndex];
                scenes[sceneIndex] = new EditorBuildSettingsScene(existing.path, true);
            }
            else
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AssignObjectArray<T>(SerializedProperty property, IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static (SaveLoadManager, LevelFlowManager) EnsureManagers(Scene scene)
        {
            SaveLoadManager saveLoadManager = Object.FindFirstObjectByType<SaveLoadManager>();
            LevelFlowManager levelFlowManager = Object.FindFirstObjectByType<LevelFlowManager>();

            GameObject managerRoot = GameObject.Find("MainMenuManagers");
            if (managerRoot == null)
            {
                managerRoot = new GameObject("MainMenuManagers");
                SceneManager.MoveGameObjectToScene(managerRoot, scene);
            }

            if (saveLoadManager == null)
                saveLoadManager = managerRoot.AddComponent<SaveLoadManager>();

            if (levelFlowManager == null)
                levelFlowManager = managerRoot.AddComponent<LevelFlowManager>();

            return (saveLoadManager, levelFlowManager);
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.26f, 0.37f, 0.95f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.88f, 0.92f, 1f, 1f);
            colors.highlightedColor = new Color(0.95f, 0.98f, 1f, 1f);
            colors.pressedColor = new Color(0.76f, 0.86f, 1f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 0.7f);
            button.colors = colors;

            Text text = CreateText("Label", buttonObject.transform, font, label, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            StretchToParent(text.GetComponent<RectTransform>());
            return button;
        }

        private static Slider CreateSliderRow(string name, Transform parent, Font font, string label, Vector2 anchoredPosition)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = anchoredPosition;
            containerRect.sizeDelta = new Vector2(700f, 72f);

            CreateText("Label", container.transform, font, label, 24, TextAnchor.MiddleLeft, new Vector2(-220f, 0f), new Vector2(280f, 48f));

            GameObject sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            sliderObject.transform.SetParent(container.transform, false);

            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(140f, 0f);
            sliderRect.sizeDelta = new Vector2(360f, 28f);

            Image background = sliderObject.GetComponent<Image>();
            background.color = new Color(0.13f, 0.17f, 0.22f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5f, 5f);
            fillAreaRect.offsetMax = new Vector2(-5f, -5f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.31f, 0.74f, 1f, 1f);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.wholeNumbers = false;
            return slider;
        }

        private static Toggle CreateToggleRow(string name, Transform parent, Font font, string label, Vector2 anchoredPosition)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = anchoredPosition;
            containerRect.sizeDelta = new Vector2(700f, 72f);

            CreateText("Label", container.transform, font, label, 24, TextAnchor.MiddleLeft, new Vector2(-220f, 0f), new Vector2(280f, 48f));

            GameObject toggleObject = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObject.transform.SetParent(container.transform, false);

            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
            toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(140f, 0f);
            toggleRect.sizeDelta = new Vector2(48f, 48f);

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(toggleObject.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            StretchToParent(backgroundRect);

            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = new Color(0.13f, 0.17f, 0.22f, 1f);

            GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;

            Image checkmarkImage = checkmark.GetComponent<Image>();
            checkmarkImage.color = new Color(0.31f, 0.74f, 1f, 1f);

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;
            toggle.isOn = false;
            return toggle;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            string content,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.94f, 0.96f, 1f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        private static void RemoveRootByName(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == rootName)
                    Object.DestroyImmediate(roots[i]);
            }
        }

        private static void EnsureFolderPath(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
