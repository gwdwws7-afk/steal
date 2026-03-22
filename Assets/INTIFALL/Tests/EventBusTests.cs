using NUnit.Framework;
using INTIFALL.System;

namespace INTIFALL.Tests
{
    public class EventBusTests
    {
        [Test]
        public void Subscribe_ShouldReceiveEvent()
        {
            int received = 0;
            EventBus.Subscribe<PlayerMovedEvent>(e => received++);
            EventBus.Publish(new PlayerMovedEvent());
            Assert.AreEqual(1, received);
        }

        [Test]
        public void Unsubscribe_ShouldNotReceiveEvent()
        {
            int received = 0;
            global::System.Action<PlayerMovedEvent> handler = e => received++;
            EventBus.Subscribe(handler);
            EventBus.Publish(new PlayerMovedEvent());
            EventBus.Unsubscribe(handler);
            EventBus.Publish(new PlayerMovedEvent());
            Assert.AreEqual(1, received);
        }

        [Test]
        public void Publish_MultipleSubscribers_ShouldDeliverToAll()
        {
            int a = 0, b = 0;
            EventBus.Subscribe<PlayerMovedEvent>(e => a++);
            EventBus.Subscribe<PlayerMovedEvent>(e => b++);
            EventBus.Publish(new PlayerMovedEvent());
            Assert.AreEqual(1, a);
            Assert.AreEqual(1, b);
        }

        [Test]
        public void Publish_NoSubscribers_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new PlayerMovedEvent()));
        }
    }
}
