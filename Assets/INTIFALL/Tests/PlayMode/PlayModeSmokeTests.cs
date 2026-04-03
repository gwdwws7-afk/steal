using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace INTIFALL.PlayModeTests
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator GameLoop_TicksAtLeastOneFrame()
        {
            yield return null;
            Assert.GreaterOrEqual(Time.frameCount, 1);
        }
    }
}
