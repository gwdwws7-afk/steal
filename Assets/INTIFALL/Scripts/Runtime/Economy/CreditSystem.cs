using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Economy
{
    public struct CreditsChangedEvent
    {
        public int currentCredits;
        public int changeAmount;
        public bool isEarning;
    }

    public struct CreditsSpentEvent
    {
        public int amount;
        public string purchaseType;
        public string itemId;
    }

    public class CreditSystem : MonoBehaviour
    {
        [Header("Starting Credits")]
        [SerializeField] private int startingCredits = 0;

        private int _currentCredits;
        private int _totalEarned;
        private int _totalSpent;

        public int CurrentCredits => _currentCredits;
        public int TotalEarned => _totalEarned;
        public int TotalSpent => _totalSpent;

        private void Awake()
        {
            _currentCredits = startingCredits;
            _totalEarned = 0;
            _totalSpent = 0;
        }

        public bool CanAfford(int amount)
        {
            return _currentCredits >= amount;
        }

        public void EarnCredits(int amount, string source = "unknown")
        {
            if (amount <= 0) return;

            _currentCredits += amount;
            _totalEarned += amount;

            EventBus.Publish(new CreditsChangedEvent
            {
                currentCredits = _currentCredits,
                changeAmount = amount,
                isEarning = true
            });
        }

        public bool SpendCredits(int amount, string purchaseType, string itemId)
        {
            if (amount <= 0) return false;
            if (_currentCredits < amount) return false;

            _currentCredits -= amount;
            _totalSpent += amount;

            EventBus.Publish(new CreditsChangedEvent
            {
                currentCredits = _currentCredits,
                changeAmount = amount,
                isEarning = false
            });

            EventBus.Publish(new CreditsSpentEvent
            {
                amount = amount,
                purchaseType = purchaseType,
                itemId = itemId
            });

            return true;
        }

        public void SetCredits(int amount, string source = "LoadGame")
        {
            int clamped = Mathf.Max(0, amount);
            if (_currentCredits == clamped) return;

            int delta = clamped - _currentCredits;
            _currentCredits = clamped;
            if (delta > 0)
                _totalEarned += delta;
            else
                _totalSpent += -delta;

            EventBus.Publish(new CreditsChangedEvent
            {
                currentCredits = _currentCredits,
                changeAmount = Mathf.Abs(delta),
                isEarning = delta >= 0
            });
        }

        public void ResetForNewGame()
        {
            _currentCredits = startingCredits;
            _totalEarned = 0;
            _totalSpent = 0;
        }

        public void ResetForLevel()
        {
            _totalSpent = 0;
        }

        public void AddMissionReward(
            int rankScore,
            int secondaryObjectives,
            int intelCount,
            bool zeroKill,
            bool noDamage,
            float timeBonus,
            float routeMultiplier = 1f,
            int routeRiskTier = 0,
            bool usedOptionalExit = false,
            int alertsTriggered = 0,
            int toolsUsed = 0)
        {
            int baseReward = rankScore switch
            {
                5 => 760,
                4 => 520,
                3 => 320,
                2 => 170,
                _ => 80
            };

            int total = baseReward;

            if (zeroKill) total += 140;
            if (noDamage) total += 160;
            if (secondaryObjectives > 0) total += secondaryObjectives * 60;
            if (timeBonus > 0f)
                total += Mathf.RoundToInt(Mathf.Clamp(timeBonus, 0f, 3f) * 80f);
            total += Mathf.Max(0, intelCount) * 50;

            int normalizedAlerts = Mathf.Max(0, alertsTriggered);
            int normalizedTools = Mathf.Max(0, toolsUsed);
            float pressureFactor = Mathf.Clamp(1f - (normalizedAlerts * 0.06f), 0.7f, 1f);
            total = Mathf.RoundToInt(total * Mathf.Clamp(routeMultiplier, 0.5f, 2f) * pressureFactor);

            total += Mathf.Clamp(routeRiskTier, 0, 3) * 25;
            if (usedOptionalExit)
                total += Mathf.Max(20, 70 - (normalizedAlerts * 15));

            total -= Mathf.Max(0, normalizedAlerts - 1) * 25;
            total -= Mathf.Max(0, normalizedTools - 4) * 8;
            total = Mathf.Max(0, total);

            EarnCredits(total, "MissionComplete");
        }
    }
}
