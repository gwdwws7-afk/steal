using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.UI
{
    public class EagleEyeUI : MonoBehaviour
    {
        [Header("Intel Display")]
        [SerializeField] private Text intelCountText;
        [SerializeField] private Text[] intelLabels = new Text[3];
        [SerializeField] private Image[] intelIcons = new Image[3];
        [SerializeField] private GameObject intelPopupPrefab;

        [Header("Minimap")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private Transform playerOnMinimap;
        [SerializeField] private float minimapZoom = 1f;

        [Header("Objectives")]
        [SerializeField] private Text primaryObjectiveText;
        [SerializeField] private Text secondaryObjectiveText;

        [Header("Settings")]
        [SerializeField] private int totalIntelPerLevel = 3;
        [SerializeField] private float popupDuration = 3f;

        private int _currentIntel;
        private GameObject _activePopup;

        public int CurrentIntel => _currentIntel;

        private void Start()
        {
            UpdateIntelDisplay();
        }

        public void CollectIntel(int intelIndex)
        {
            if (intelIndex < 0 || intelIndex >= totalIntelPerLevel) return;

            _currentIntel = Mathf.Min(_currentIntel + 1, totalIntelPerLevel);
            UpdateIntelDisplay();
            ShowIntelPopup($"情报 {intelIndex + 1}");
        }

        public void SetIntelCount(int count)
        {
            _currentIntel = Mathf.Clamp(count, 0, totalIntelPerLevel);
            UpdateIntelDisplay();
        }

        private void UpdateIntelDisplay()
        {
            if (intelCountText != null)
                intelCountText.text = $"{_currentIntel}/{totalIntelPerLevel}";

            for (int i = 0; i < intelIcons.Length; i++)
            {
                if (intelIcons[i] != null)
                {
                    intelIcons[i].color = i < _currentIntel ? Color.yellow : Color.gray;
                }
            }
        }

        private void ShowIntelPopup(string intelName)
        {
            if (_activePopup != null)
                Destroy(_activePopup);

            if (intelPopupPrefab != null)
            {
                _activePopup = Instantiate(intelPopupPrefab, transform);
                var popupText = _activePopup.GetComponentInChildren<Text>();
                if (popupText != null)
                    popupText.text = $"获得情报: {intelName}";

                Destroy(_activePopup, popupDuration);
            }
        }

        public void SetPrimaryObjective(string objective)
        {
            if (primaryObjectiveText != null)
                primaryObjectiveText.text = objective;
        }

        public void SetSecondaryObjective(string objective)
        {
            if (secondaryObjectiveText != null)
                secondaryObjectiveText.text = objective;
        }

        public void UpdateMinimap(Vector3 playerPosition, float playerRotation)
        {
            if (playerOnMinimap != null)
            {
                playerOnMinimap.localRotation = Quaternion.Euler(0, 0, -playerRotation);
            }
        }

        public void ResetIntel()
        {
            _currentIntel = 0;
            UpdateIntelDisplay();
        }
    }
}