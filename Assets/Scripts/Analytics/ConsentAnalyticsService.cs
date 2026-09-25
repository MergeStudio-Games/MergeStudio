using System;
using System.Collections.Generic;

namespace MergeStudio.Analytics
{
    /// <summary>Session-scoped opt-in boundary. Drops events until consent; never queues them.</summary>
    public sealed class ConsentAnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsService _provider;
        public bool HasConsent { get; private set; }

        public ConsentAnalyticsService(IAnalyticsService provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void SetConsent(bool granted) => HasConsent = granted;

        public void LogEvent(string name, Dictionary<string, object> parameters)
        {
            if (!HasConsent) return;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Event name required.", nameof(name));
            // Isolate the provider from the caller's mutable dictionary.
            _provider.LogEvent(name, parameters == null ? new Dictionary<string, object>() : new Dictionary<string, object>(parameters));
        }
    }
}
