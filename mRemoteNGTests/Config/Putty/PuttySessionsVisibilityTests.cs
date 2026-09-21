using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using mRemoteNG.Config.Putty;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Putty
{
    /// <summary>
    /// The "show saved PuTTY sessions" option, finally connected to the tree (#389).
    /// </summary>
    [TestFixture]
    public class PuttySessionsVisibilityTests
    {
        private ConnectionTreeModel _model = null!;
        private RootNodeInfo _connections = null!;
        private RootPuttySessionsNodeInfo _putty = null!;
        private List<NotifyCollectionChangedAction> _raised = null!;

        [SetUp]
        public void ATreeWithBothRoots()
        {
            _model = new ConnectionTreeModel();
            _connections = new RootNodeInfo(RootNodeType.Connection);
            _putty = new RootPuttySessionsNodeInfo();
            _model.AddRootNode(_connections);
            _model.AddRootNode(_putty);
            _raised = [];
            _model.CollectionChanged += (_, args) => _raised.Add(args.Action);
        }

        [Test]
        public void HidingTakesThePuttyRootOutAndLeavesTheConnectionsAlone()
        {
            PuttySessionsVisibility.Apply(_model, [_putty], show: false);

            Assert.That(_model.RootNodes, Is.EqualTo(new[] { _connections }));
            Assert.That(_raised, Is.EqualTo(new[] { NotifyCollectionChangedAction.Remove }),
                        "the tree learns about it through the model's own event");
        }

        [Test]
        public void ShowingPutsItBackAfterTheConnectionsRoot()
        {
            PuttySessionsVisibility.Apply(_model, [_putty], show: false);
            _raised.Clear();

            PuttySessionsVisibility.Apply(_model, [_putty], show: true);

            Assert.That(_model.RootNodes, Is.EqualTo(new[] { _connections, _putty }),
                        "same order as at load time: connections first, PuTTY after");
            Assert.That(_raised, Is.EqualTo(new[] { NotifyCollectionChangedAction.Add }));
        }

        [Test]
        public void ShowingWhenAlreadyShownChangesNothing()
        {
            PuttySessionsVisibility.Apply(_model, [_putty], show: true);

            Assert.That(_model.RootNodes, Is.EqualTo(new[] { _connections, _putty }));
            Assert.That(_raised, Is.Empty, "no event means no needless tree rebuild");
        }

        [Test]
        public void HidingWhenAlreadyHiddenChangesNothing()
        {
            PuttySessionsVisibility.Apply(_model, [_putty], show: false);
            _raised.Clear();

            PuttySessionsVisibility.Apply(_model, [_putty], show: false);

            Assert.That(_model.RootNodes, Is.EqualTo(new[] { _connections }));
            Assert.That(_raised, Is.Empty);
        }

        [Test]
        public void EveryPuttyProviderRootIsHandled()
        {
            // The registry provider and the file provider each contribute a root.
            RootPuttySessionsNodeInfo second = new();
            _model.AddRootNode(second);

            PuttySessionsVisibility.Apply(_model, [_putty, second], show: false);
            Assert.That(_model.RootNodes.OfType<RootPuttySessionsNodeInfo>(), Is.Empty);

            PuttySessionsVisibility.Apply(_model, [_putty, second], show: true);
            Assert.That(_model.RootNodes.OfType<RootPuttySessionsNodeInfo>(), Is.EquivalentTo(new[] { _putty, second }));
        }

        [Test]
        public void NothingLoadedYetIsNotAnError()
        {
            Assert.DoesNotThrow(() => PuttySessionsVisibility.Apply(null, [_putty], show: false));
            Assert.DoesNotThrow(() => PuttySessionsVisibility.Apply(_model, null, show: true));
        }
    }
}
