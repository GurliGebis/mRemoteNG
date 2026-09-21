using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using mRemoteNG.Connection;
using mRemoteNG.Properties;
using mRemoteNG.Tree;

namespace mRemoteNG.Tools
{
    /// <summary>
    /// Background service that periodically probes each connection's host port and updates
    /// <see cref="ConnectionInfo.HostReachabilityStatus"/> so the connection tree can show
    /// online/offline status overlays (issue #1109).
    ///
    /// Usage:
    ///   var monitor = new HostStatusMonitor(model);
    ///   monitor.Start();   // begins background scanning
    ///   monitor.Stop();    // cancel (also called by Dispose)
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class HostStatusMonitor : IDisposable
    {
        private readonly ConnectionTreeModel _model;

        /// <summary>The model this monitor was constructed with.</summary>
        public ConnectionTreeModel Model => _model;
        private CancellationTokenSource? _cts;

        /// <summary>How long to wait between full scan cycles (default 30 s).</summary>
        public int CheckIntervalSeconds { get; set; } = 30;

        /// <summary>TCP connect timeout per individual host (default 1 000 ms).</summary>
        public int CheckTimeoutMilliseconds { get; set; } = 1000;

        /// <summary>Delay between successive host checks to avoid network bursts (default 50 ms).</summary>
        public int StaggerDelayMilliseconds { get; set; } = 50;

        /// <summary>
        /// Whether a host must also answer an ICMP echo to count as reachable. Null follows the
        /// Options > Connections setting on every cycle, so a change there takes effect on the
        /// next scan without restarting the monitor; a test sets it explicitly.
        /// </summary>
        public bool? RequireIcmpEcho { get; set; }

        public HostStatusMonitor(ConnectionTreeModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>Start (or restart) the background monitoring loop.</summary>
        public void Start()
        {
            Stop();
            _cts = new CancellationTokenSource();
            Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        }

        /// <summary>Stop the background monitoring loop.</summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose() => Stop();

        private async Task RunAsync(CancellationToken ct)
        {
            // Small initial delay so the UI settles before the first scan starts.
            try { await Task.Delay(5000, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { return; }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await CheckAllHostsAsync(ct).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Swallow unexpected errors — monitoring must not crash the app.
                    try { await Task.Delay(5000, ct).ConfigureAwait(false); } catch { break; }
                }
            }
        }

        private async Task CheckAllHostsAsync(CancellationToken ct)
        {
            var connections = _model.GetRecursiveChildList()
                .Where(c => !c.IsContainer
                         && !string.IsNullOrWhiteSpace(c.Hostname))
                .ToList();

            bool requireIcmpEcho = RequireIcmpEcho ?? OptionsConnectionsPage.Default.RequireIcmpEchoForHostStatus;

            foreach (var connection in connections)
            {
                if (ct.IsCancellationRequested) break;

                int port = connection.Port > 0 ? connection.Port : connection.GetDefaultPort();
                bool reachable = await HostReachabilityProbe
                    .IsReachableAsync(connection.Hostname, port, CheckTimeoutMilliseconds, requireIcmpEcho, ct)
                    .ConfigureAwait(false);

                // The probe answers false for a cancelled connect as well as a refused one. A stop
                // that lands mid-probe - every Options > Connections save restarts this monitor -
                // must not be written down as the host being unreachable.
                if (ct.IsCancellationRequested) break;

                connection.HostReachabilityStatus = reachable
                    ? HostReachabilityStatus.Reachable
                    : HostReachabilityStatus.Unreachable;

                if (StaggerDelayMilliseconds > 0)
                {
                    try { await Task.Delay(StaggerDelayMilliseconds, ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }
    }
}
