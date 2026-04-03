using System.Collections;
using INTIFALL.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class InputCompatibilityPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerController_CanTickAFrame_WithoutInputExceptions()
        {
            var player = new GameObject("InputCompat_Player");
            player.AddComponent<CharacterController>();
            player.AddComponent<PlayerStateMachine>();
            player.AddComponent<PlayerController>();

            yield return null;

            Assert.IsTrue(player != null);
            Object.Destroy(player);
        }
    }
}
