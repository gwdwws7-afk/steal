using System.Collections.Generic;
using UnityEngine;

namespace INTIFALL.AI
{
    public static class EnemySquadCoordinator
    {
        private static readonly List<EnemyController> ActiveEnemies = new();
        private static int _waveCounter;
        public static int ActiveEnemyCount => ActiveEnemies.Count;

        public static void Register(EnemyController enemy)
        {
            PurgeInvalidEntries();

            if (enemy == null || ActiveEnemies.Contains(enemy))
                return;

            ActiveEnemies.Add(enemy);
        }

        public static void Unregister(EnemyController enemy)
        {
            PurgeInvalidEntries();

            if (enemy == null)
                return;

            ActiveEnemies.Remove(enemy);
        }

        public static int NextWaveId()
        {
            _waveCounter++;
            if (_waveCounter <= 0)
                _waveCounter = 1;

            return _waveCounter;
        }

        public static void BroadcastAlert(
            EnemyController source,
            Vector3 alertPosition,
            bool highPriority,
            float broadcastRange,
            int waveId)
        {
            PurgeInvalidEntries();

            if (source == null)
                return;

            float safeRange = Mathf.Max(1f, broadcastRange);
            float sqrRange = safeRange * safeRange;
            Vector3 sourcePos = source.transform.position;

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                EnemyController listener = ActiveEnemies[i];
                if (listener == null || listener == source || listener.IsDead)
                    continue;

                if ((listener.transform.position - sourcePos).sqrMagnitude > sqrRange)
                    continue;

                listener.ReceiveSquadAlert(alertPosition, waveId, highPriority);
            }
        }

        public static void ResetForTests()
        {
            ActiveEnemies.Clear();
            _waveCounter = 0;
        }

        private static void PurgeInvalidEntries()
        {
            for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = ActiveEnemies[i];
                if (enemy == null || enemy.Equals(null))
                    ActiveEnemies.RemoveAt(i);
            }
        }

        public static Vector3 ComputeSearchPoint(
            Vector3 anchor,
            int enemyInstanceId,
            int sectorCount,
            float radius,
            int step)
        {
            int safeSectors = Mathf.Max(3, sectorCount);
            int safeStep = Mathf.Max(0, step);
            float safeRadius = Mathf.Max(1f, radius);

            int baseSector = Mathf.Abs(enemyInstanceId) % safeSectors;
            int sector = (baseSector + safeStep) % safeSectors;
            float angle = (360f / safeSectors) * sector;

            float ringMultiplier = 1f + 0.35f * (safeStep % 3);
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * safeRadius * ringMultiplier;
            return anchor + offset;
        }
    }
}
