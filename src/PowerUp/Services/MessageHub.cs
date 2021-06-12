using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Essentials;

namespace PowerUp.Services
{
    public class MessageHub
    {
        private readonly ConcurrentDictionary<string, List<Action<object>>> _subscriptions
            = new ConcurrentDictionary<string, List<Action<object>>>();

        public void Publish(string eventName, object payload)
        {
            foreach (var subscriber in _subscriptions.GetOrAdd(eventName))
            {
                subscriber?.Invoke(payload);
            }
        }

        public Task PublishAsync(string eventName, object payload)
        {
            return Task.Run(() =>
            {
                Publish(eventName, payload);
            });
        }

        public void Subscribe(string eventName, Action<object> callback)
        {
            _subscriptions.GetOrAdd(eventName)
                .Add(callback);
        }
    }
}
