using System.Collections.Generic;
using mRemoteNG.Config.Import;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Tree.Root;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Import
{
    /// <summary>
    /// Where a port scan puts the hosts it imported.
    ///
    /// These rules decide what a user sees after importing, and getting one wrong scatters their
    /// connections somewhere they did not choose. None of it was reachable from the suite until the
    /// decision moved out of the window: it read the connection tree through a process-wide
    /// singleton, so a test would have had to stand up the application to ask a question about
    /// three nodes.
    /// </summary>
    [TestFixture]
    public class PortScanDestinationResolverTests
    {
        private RootNodeInfo _connectionRoot = null!;

        [SetUp]
        public void Setup() => _connectionRoot = new RootNodeInfo(RootNodeType.Connection);

        [Test]
        public void NothingSelectedImportsIntoTheConnectionRoot()
        {
            Assert.That(PortScanDestinationResolver.Resolve(null, _connectionRoot),
                        Is.SameAs(_connectionRoot));
        }

        [Test]
        public void ASelectedFolderIsTheDestination()
        {
            ContainerInfo folder = new() { Name = "Servers" };
            _connectionRoot.AddChild(folder);

            Assert.That(PortScanDestinationResolver.Resolve(folder, _connectionRoot),
                        Is.SameAs(folder));
        }

        [Test]
        public void ASelectedConnectionImportsIntoItsParentFolder()
        {
            // "Next to the thing I selected", which is the folder holding it - not inside a
            // connection, which cannot hold anything.
            ContainerInfo folder = new() { Name = "Servers" };
            _connectionRoot.AddChild(folder);
            ConnectionInfo connection = new() { Name = "web01" };
            folder.AddChild(connection);

            Assert.That(PortScanDestinationResolver.Resolve(connection, _connectionRoot),
                        Is.SameAs(folder));
        }

        [Test]
        public void APuttySessionFallsBackToTheConnectionRoot()
        {
            // PuTTY sessions are read from PuTTY's own saved sessions and nothing can be imported
            // into them, so the import has to go somewhere that can hold it.
            PuttySessionInfo puttySession = new() { Name = "saved-session" };

            Assert.That(PortScanDestinationResolver.Resolve(puttySession, _connectionRoot),
                        Is.SameAs(_connectionRoot));
        }

        [Test]
        public void ThePuttySessionsRootFallsBackToTheConnectionRoot()
        {
            RootPuttySessionsNodeInfo puttyRoot = new();

            Assert.That(PortScanDestinationResolver.Resolve(puttyRoot, _connectionRoot),
                        Is.SameAs(_connectionRoot));
        }

        [Test]
        public void WithNoTreeAtAllThereIsNowhereToImport()
        {
            Assert.That(PortScanDestinationResolver.Resolve(null, null), Is.Null);
        }

        [Test]
        public void APuttySessionWithNoConnectionRootHasNowhereToGo()
        {
            // Rather than quietly importing into the PuTTY node, which cannot hold connections.
            PuttySessionInfo puttySession = new() { Name = "saved-session" };

            Assert.That(PortScanDestinationResolver.Resolve(puttySession, null), Is.Null);
        }

        [Test]
        public void TheConnectionRootIsFoundAmongTheTreesRoots()
        {
            RootPuttySessionsNodeInfo puttyRoot = new();
            List<ConnectionInfo> roots = [puttyRoot, _connectionRoot];

            Assert.That(PortScanDestinationResolver.ConnectionRootOf(roots), Is.SameAs(_connectionRoot));
        }

        [Test]
        public void ATreeWithNoConnectionRootReportsNoneRatherThanThrowing()
        {
            // The window used to call First() here, which throws on a tree that has only PuTTY
            // sessions - an exception where a null was the honest answer.
            List<ConnectionInfo> roots = [new RootPuttySessionsNodeInfo()];

            Assert.That(PortScanDestinationResolver.ConnectionRootOf(roots), Is.Null);
            Assert.That(PortScanDestinationResolver.ConnectionRootOf(null), Is.Null);
        }
    }
}
