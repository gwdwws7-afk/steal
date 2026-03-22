using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Player;

namespace INTIFALL.UI
{
    public class HPHUD : MonoBehaviour
    {
        [Header("HP Display")]
        [SerializeField] private Image[] hpBars = new Image[5];
        [SerializeField] private Text hpCountText;
        [SerializeField] private Image hpFillImage;

        [Header("First Aid")]
        [SerializeField] private Text firstAidCountText;
        [SerializeField] private Image firstAidCooldownImage;

        [Header("Colors")]
        [SerializeField] private Color fullHPColor = Color.green;
        [SerializeField] private Color mediumHPColor = Color.yellow;
        [SerializeField] private Color lowHPColor = Color.red;
        [SerializeField] private Color emptyHPColor = Color.gray;

        [Header("Damage Effect")]
        [SerializeField] private Image damageOverlay;
        [SerializeField] private float damageFadeSpeed = 2f;

        [Header("Heal Effect")]
        [SerializeField] private Image healOverlay;
        [SerializeField] private float healFadeSpeed = 1f;

        private PlayerHealthSystem _playerHealth;
        private float _damageAlpha;
        private float _healAlpha;
        private int _maxHP = 5;

        public void Initialize(PlayerHealthSystem healthSystem)
        {
            _playerHealth = healthSystem;
            _maxHP = healthSystem.MaxHP;

            if (hpBars.Length != _maxHP)
            {
                System.Array.Resize(ref hpBars, _maxHP);
            }

            UpdateHPDisplay(healthSystem.CurrentHP, healthSystem.MaxHP);
            UpdateFirstAidDisplay();
        }

        public void UpdateHPDisplay(int currentHP, int maxHP)
        {
            _maxHP = maxHP;

            for (int i = 0; i < hpBars.Length; i++)
            {
                if (hpBars[i] == null) continue;

                if (i < currentHP)
                {
                    hpBars[i].color = GetHPColor(currentHP, maxHP);
                    hpBars[i].fillAmount = 1f;
                }
                else if (i == currentHP)
                {
                    hpBars[i].color = emptyHPColor;
                    hpBars[i].fillAmount = 0f;
                }
                else
                {
                    hpBars[i].color = emptyHPColor;
                    hpBars[i].fillAmount = 0f;
                }
            }

            if (hpCountText != null)
                hpCountText.text = $"{currentHP}/{maxHP}";

            if (hpFillImage != null)
                hpFillImage.fillAmount = (float)currentHP / maxHP;

            if (currentHP <= 1 && currentHP > 0)
            {
                TriggerLowHPWarning();
            }
        }

        public void UpdateFirstAidDisplay()
        {
            if (_playerHealth == null) return;

            if (firstAidCountText != null)
                firstAidCountText.text = $"急救包: {_playerHealth.FirstAidCount}";

            if (firstAidCooldownImage != null)
            {
                if (_playerHealth.IsUsingFirstAid)
                {
                    firstAidCooldownImage.fillAmount = _playerHealth.FirstAidProgress;
                    firstAidCooldownImage.gameObject.SetActive(true);
                }
                else
                {
                    firstAidCooldownImage.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateHPRecoveryState()
        {
            if (_playerHealth != null)
                UpdateFirstAidDisplay();
        }

        private Color GetHPColor(int currentHP, int maxHP)
        {
            float ratio = (float)currentHP / maxHP;

            if (ratio > 0.6f)
                return fullHPColor;
            else if (ratio > 0.3f)
                return mediumHPColor;
            else
                return lowHPColor;
        }

        private void TriggerLowHPWarning()
        {
            if (damageOverlay != null)
            {
                damageOverlay.gameObject.SetActive(true);
                _damageAlpha = 0.5f;
            }
        }

        private void Update()
        {
            UpdateDamageOverlay();
            UpdateHealOverlay();
        }

        private void UpdateDamageOverlay()
        {
            if (_damageAlpha > 0)
            {
                _damageAlpha -= Time.deltaTime * damageFadeSpeed;
                _damageAlpha = Mathf.Max(0, _damageAlpha);

                if (damageOverlay != null)
                {
                    Color c = damageOverlay.color;
                    damageOverlay.color = new Color(c.r, c.g, c.b, _damageAlpha);

                    if (_damageAlpha <= 0)
                        damageOverlay.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHealOverlay()
        {
            if (_healAlpha > 0)
            {
                _healAlpha -= Time.deltaTime * healFadeSpeed;
                _healAlpha = Mathf.Max(0, _healAlpha);

                if (healOverlay != null)
                {
                    Color c = healOverlay.color;
                    healOverlay.color = new Color(c.r, c.g, c.b, _healAlpha);

                    if (_healAlpha <= 0)
                        healOverlay.gameObject.SetActive(false);
                }
            }
        }

        public void ShowDamageEffect()
        {
            if (damageOverlay != null)
            {
                damageOverlay.gameObject.SetActive(true);
                _damageAlpha = 0.6f;
            }
        }

        public void ShowHealEffect()
        {
            if (healOverlay != null)
            {
                healOverlay.gameObject.SetActive(true);
                _healAlpha = 0.4f;
            }
        }
    }
}