using System.Collections.Generic;
using NUnit.Framework;
using MergeStudio.Analytics;

namespace MergeStudio.Tests
{
    public sealed class ConsentAnalyticsTests
    {
        private sealed class Recorder : IAnalyticsService
        {
            public int Calls;
            public void LogEvent(string name, Dictionary<string, object> parameters) { Calls++; parameters.Clear(); }
        }

        [Test] public void ConsentDefaultsOffAndRevocationStopsDelivery()
        {
            var provider = new Recorder(); var service = new ConsentAnalyticsService(provider);
            service.LogEvent("session_start", null); Assert.AreEqual(0, provider.Calls);
            service.SetConsent(true); service.LogEvent("session_start", null); Assert.AreEqual(1, provider.Calls);
            service.SetConsent(false); service.LogEvent("session_start", null); Assert.AreEqual(1, provider.Calls);
            service.SetConsent(true); Assert.AreEqual(1, provider.Calls, "Dropped events must not be replayed.");
        }

        [Test] public void ProviderCannotMutateCallerDictionary()
        {
            var service = new ConsentAnalyticsService(new Recorder()); service.SetConsent(true);
            var fields = new Dictionary<string, object> { { "tier", 2 } };
            service.LogEvent("merge", fields); Assert.AreEqual(2, fields["tier"]);
        }
    }
}
