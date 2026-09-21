using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Connection.Protocol.SSH;
using mRemoteNG.Properties;
using mRemoteNG.UI;
using NUnit.Framework;

namespace mRemoteNGTests.UI
{
    /// <summary>
    /// Every key the image list hands out must resolve to an image it actually holds.
    ///
    /// The tree asks for a key and paints whatever that key resolves to; a key nobody ever
    /// added paints nothing. For the reachability badge that is not a cosmetic gap - a host that
    /// is down shows no "off" badge, which reads as fine (#193).
    /// </summary>
    [TestFixture]
    public class StatusImageListTests
    {
        private bool _showHostStatusBefore;

        [SetUp]
        public void ShowTheBadge()
        {
            _showHostStatusBefore = OptionsConnectionsPage.Default.ShowHostStatus;
            OptionsConnectionsPage.Default.ShowHostStatus = true;
        }

        [TearDown]
        public void RestoreTheSetting() => OptionsConnectionsPage.Default.ShowHostStatus = _showHostStatusBefore;

        private static ConnectionInfo Connection(HostReachabilityStatus reachability, bool withOpenSession)
        {
            ConnectionInfo connection = new()
            {
                Name = "web01",
                Icon = "mRemoteNG",
                HostReachabilityStatus = reachability,
            };
            if (withOpenSession)
                connection.OpenConnections.Add(new ProtocolSSH2());
            return connection;
        }

        [TestCase(HostReachabilityStatus.Reachable, false)]
        [TestCase(HostReachabilityStatus.Unreachable, false)]
        [TestCase(HostReachabilityStatus.Reachable, true)]
        [TestCase(HostReachabilityStatus.Unreachable, true)]
        public void EveryKeyHandedOutResolvesToAnImage(HostReachabilityStatus reachability, bool withOpenSession)
        {
            // The two "with an open session" rows are the ones that used to come back empty: the
            // connected base icon never got its reachability variants generated.
            using StatusImageList images = new();
            ConnectionInfo connection = Connection(reachability, withOpenSession);

            string key = images.GetKey(connection);

            Assert.That(images.ImageList.Images.ContainsKey(key), Is.True,
                        $"the tree was handed '{key}', which the image list does not hold");
            Assert.That(images.GetImage(connection), Is.Not.Null);
        }

        [Test]
        public void AnUnreachableHostWithAnOpenTabStillShowsTheOffBadge()
        {
            using StatusImageList images = new();

            string key = images.GetKey(Connection(HostReachabilityStatus.Unreachable, withOpenSession: true));

            Assert.That(key, Does.EndWith("_Play_Off"));
            Assert.That(images.ImageList.Images.ContainsKey(key), Is.True);
        }
    }
}
