using NUnit.Framework;
using INTIFALL.System;

namespace INTIFALL.Tests
{
    public class EventBusTests
    {
        [SetUp]
        public void Setup()
        {
            EventBus.ClearAllSubscribers();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllSubscribers();
        }

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

        [Test]
        public void Subscribe_SameHandlerTwice_DoesNotDuplicateInvocation()
        {
            int received = 0;
            global::System.Action<PlayerMovedEvent> handler = _ => received++;

            EventBus.Subscribe(handler);
            EventBus.Subscribe(handler);
            EventBus.Publish(new PlayerMovedEvent());

            Assert.AreEqual(1, received);
            Assert.AreEqual(1, EventBus.GetSubscriberCount<PlayerMovedEvent>());
        }
    }
}
