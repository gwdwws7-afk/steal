using System.Reflection;
using INTIFALL.Tools;
using INTIFALL.UI;
using NUnit.Framework;

namespace INTIFALL.Tests
{
    public class ToolHUDTests
    {
        [Test]
        public void FormatCapacityLabel_ReturnsUsedAndRemaining()
        {
            string text = InvokePrivateStaticString("FormatCapacityLabel", 3, 4);
            Assert.AreEqual("Loadout 3/4  Remaining 1", text);
        }

        [Test]
        public void FormatSlotLabel_AppendsSlotCostOnlyWhenGreaterThanOne()
        {
            string oneSlot = InvokePrivateStaticString("FormatSlotLabel", "SmokeBomb", 1);
            string twoSlot = InvokePrivateStaticString("FormatSlotLabel", "EMP", 2);

            Assert.AreEqual("SmokeBomb", oneSlot);
            Assert.AreEqual("EMP [2]", twoSlot);
        }

        [Test]
        public void FormatEquipRejectedMessage_ContainsNeedAndRemaining()
        {
            ToolEquipRejectedEvent evt = new ToolEquipRejectedEvent
            {
                toolName = "Drone",
                slotCost = 2,
                remainingCapacity = 1
            };

            string text = InvokePrivateStaticString("FormatEquipRejectedMessage", evt);
            Assert.AreEqual("Cannot equip Drone: need 2, have 1 free.", text);
        }

        private static string InvokePrivateStaticString(string methodName, params object[] args)
        {
            MethodInfo method = typeof(ToolHUD).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, $"Missing ToolHUD.{methodName}.");

            return (string)method.Invoke(null, args);
        }
    }
}
