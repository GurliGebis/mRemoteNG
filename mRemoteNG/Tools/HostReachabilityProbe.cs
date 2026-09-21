using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace mRemoteNG.Tools
{
    /// <summary>
    /// The one place that decides whether a host counts as reachable.
    ///
    /// Two surfaces show that verdict - the reachability badge on every tree icon, painted by
    /// <see cref="HostStatusMonitor"/>, and the Status button on the Config panel - and until #193
    /// each carried its own copy of the check. They could disagree about the same host for no
    /// reason but drift, and a fix to one left the other as it was.
    ///
    /// What is measured, and deliberately nothing more: does the port accept a TCP connection,
    /// and - only when the user asks for it - does the host also answer an ICMP echo. An accepted
    /// connection is not proof the host is up: a ZTNA or WAN-optimisation client that terminates
    /// connections locally completes the handshake before it knows anything about the backend,
    /// which is exactly the false green in #193. The ping is the cheap corroboration that such a
    /// client cannot fake without forwarding it. It is opt-in because some of those same products
    /// do not carry ICMP at all, and for their users a mandatory ping would turn every host red.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class HostReachabilityProbe
    {
        public static async Task<bool> IsReachableAsync(string? hostname,
                                                        int port,
                                                        int timeoutMilliseconds,
                                                        bool requireIcmpEcho,
                                                        CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hostname) || port <= 0)
                return false;

            if (!await PortAcceptsConnectionAsync(hostname, port, timeoutMilliseconds, cancellationToken)
                     .ConfigureAwait(false))
                return false;

            return !requireIcmpEcho ||
                   await AnswersEchoAsync(hostname, timeoutMilliseconds, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<bool> PortAcceptsConnectionAsync(string hostname, int port, int timeoutMilliseconds,
                                                                   CancellationToken cancellationToken)
        {
            try
            {
                using TcpClient client = new();
                using CancellationTokenSource timeout =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(timeoutMilliseconds);
                await client.ConnectAsync(hostname, port, timeout.Token).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                // Refused, timed out, unresolvable, cancelled: all the same answer here.
                return false;
            }
        }

        private static async Task<bool> AnswersEchoAsync(string hostname, int timeoutMilliseconds,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(hostname, TimeSpan.FromMilliseconds(timeoutMilliseconds),
                                                           cancellationToken: cancellationToken)
                                            .ConfigureAwait(false);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
