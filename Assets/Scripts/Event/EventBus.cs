using System;
using System.Collections.Generic;

namespace Percent111.ProjectNS.Event
{
    public static class EventBus
    {
        private sealed class Subscription
        {
            public Subscription(Delegate listener, Action<object> invoker)
            {
                Listener = listener;
                Invoker = invoker;
            }

            public Delegate Listener { get; }
            public Action<object> Invoker { get; }
        }

        private static readonly Dictionary<Type, List<Subscription>> _subscriptions = new();
        private static readonly Dictionary<Type, HashSet<Type>> _publishTypesCache = new();

        public static void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            Type eventType = typeof(TEvent);
            List<Subscription> listeners = GetOrCreateSubscriptions(eventType);
            Delegate listenerDelegate = listener;
            bool alreadyRegistered = listeners.Exists(subscription => subscription.Listener == listenerDelegate);
            if (alreadyRegistered)
            {
                return;
            }

            Action<object> invoker = payload => listener((TEvent)payload);
            Subscription subscription = new Subscription(listenerDelegate, invoker);
            listeners.Add(subscription);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : IEvent
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (!_subscriptions.TryGetValue(eventType, out List<Subscription> listeners))
            {
                return;
            }

            Delegate listenerDelegate = listener;
            listeners.RemoveAll(subscription => subscription.Listener == listenerDelegate);
            if (listeners.Count == 0)
            {
                _subscriptions.Remove(eventType);
            }
        }

        public static void Publish<TEvent>(object owner) where TEvent : IEvent, new()
        {
            PublishCore(owner, new TEvent());
        }

        public static void Publish<TEvent>(object owner, TEvent payload) where TEvent : IEvent
        {
            PublishCore(owner, payload);
        }

        public static void Publish<TEvent>(object owner, Func<TEvent> eventAction) where TEvent : IEvent
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (eventAction == null)
            {
                throw new ArgumentNullException(nameof(eventAction));
            }

            PublishCore(owner, eventAction());
        }

        private static List<Subscription> GetOrCreateSubscriptions(Type eventType) 
        {
            if (_subscriptions.TryGetValue(eventType, out List<Subscription> listeners))
            {
                return listeners;
            }

            listeners = new List<Subscription>();
            _subscriptions[eventType] = listeners;
            return listeners;
        }

        private static Type GetPublisherType(object publisher)
        {
            if (publisher is Type publisherType)
            {
                return publisherType;
            }

            return publisher.GetType();
        }
        

        private static void PublishCore<TEvent>(object owner, TEvent payload) where TEvent : IEvent
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Type ownerType = GetPublisherType(owner);
            Type allowedType = payload.GetPublishType();
            Type eventType = typeof(TEvent);

            // 상속 관계도 허용 (PlayerIdleState는 PlayerStateBase를 상속하므로 허용)
            if (!allowedType.IsAssignableFrom(ownerType))
                throw new InvalidOperationException($"{ownerType.Name} cannot publish {eventType.Name}. Only {allowedType.Name} or its subclasses can publish this event.");

            if (!_subscriptions.TryGetValue(eventType, out List<Subscription> listeners) || listeners.Count == 0)
            {
                return;
            }

            object boxedPayload = payload;
            Subscription[] snapshot = listeners.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Invoker(boxedPayload);
            }
        }
    }
}
