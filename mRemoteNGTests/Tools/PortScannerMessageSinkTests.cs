using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using NUnit.Framework;

namespace mRemoteNGTests.Tools
{
    /// <summary>
    /// The scanner reports what it is doing to a sink it is given, not to a process-wide singleton.
    ///
    /// That seam is the only reason any of this is reachable from a test: every progress message
    /// used to go straight to <c>Runtime.MessageCollector</c>, so constructing a scanner meant
    /// constructing the running application's notification stack with it.
    /// </summary>
    [TestFixture]
    public class PortScannerMessageSinkTests
    {
        private static readonly int[] Port80 = [80];

        /// <summary>Records what it was told instead of putting it on the screen.</summary>
        private sealed class RecordingSink : IOperationMessageSink
        {
            public ConcurrentQueue<string> Messages { get; } = new();

            public void Information(string message, bool onlyLog = false) => Messages.Enqueue(message);
            public void Warning(string message, bool onlyLog = false) => Messages.Enqueue(message);
            public void Error(string message, bool onlyLog = false) => Messages.Enqueue(message);

            public void Exception(string message, Exception exception, bool onlyLog = false) =>
                Messages.Enqueue(message);
        }

        private static object? SinkOf(PortScanner scanner) =>
            typeof(PortScanner)
                .GetField("_messages", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(scanner);

        [Test]
        public void TheScannerReportsProgressToTheSinkItWasGiven()
        {
            // 192.0.2.1 is TEST-NET-1 (RFC 5737): reserved for documentation and guaranteed not to
            // be a real host, so the scan ends on its own timeout rather than on whatever happens to
            // answer on the machine running the suite. One address at a 1 ms timeout keeps it short.
            RecordingSink sink = new();
            PortScanner scanner = new(IPAddress.Parse("192.0.2.1"),
                                      IPAddress.Parse("192.0.2.1"),
                                      Port80,
                                      timeoutInMilliseconds: 1,
                                      maxConcurrentHosts: 1,
                                      messageSink: sink);

            scanner.StartScan();

            // StartScan is deliberately fire-and-forget, so wait for the first message rather than
            // assume it has already been written.
            Stopwatch clock = Stopwatch.StartNew();
            while (sink.Messages.IsEmpty && clock.Elapsed < TimeSpan.FromSeconds(10))
                Thread.Sleep(10);

            scanner.StopScan();

            Assert.That(sink.Messages, Is.Not.Empty,
                        "the scan reported nothing to the sink it was constructed with");
            Assert.That(string.Join("\n", sink.Messages), Does.Contain("Starting scan of 1 hosts"));
        }

        [Test]
        public void WithNoSinkTheScannerFallsBackToTheApplicationsCollector()
        {
            // The default argument is what keeps every existing call site unchanged.
            PortScanner scanner = new(IPAddress.Parse("192.0.2.1"),
                                      IPAddress.Parse("192.0.2.1"),
                                      Port80);

            Assert.That(SinkOf(scanner), Is.InstanceOf<DefaultMessageSink>());
        }

        [Test]
        public void ThePortRangeConstructorTakesASinkToo()
        {
            RecordingSink sink = new();
            PortScanner scanner = new(IPAddress.Parse("192.0.2.1"),
                                      IPAddress.Parse("192.0.2.1"),
                                      80, 80,
                                      timeoutInMilliseconds: 1,
                                      checkDefaultPortsOnly: false,
                                      maxConcurrentHosts: 1,
                                      messageSink: sink);

            Assert.That(SinkOf(scanner), Is.SameAs(sink));
        }
    }
}
