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

        public void AddMissionReward(int rankScore, int secondaryObjectives, int intelCount, bool zeroKill, bool noDamage, float timeBonus)
        {
            int baseReward = rankScore switch
            {
                5 => 500,
                4 => 350,
                3 => 200,
                2 => 100,
                _ => 50
            };

            int total = baseReward;

            if (zeroKill) total += 150;
            if (noDamage) total += 200;
            if (secondaryObjectives > 0) total += secondaryObjectives * 50;
            if (timeBonus > 0) total += (int)(100 * timeBonus);
            total += intelCount * 50;

            EarnCredits(total, "MissionComplete");
        }
    }
}