using System.Linq;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Tree.Root;

namespace mRemoteNG.Config.Import
{
    /// <summary>
    /// Decides which container scanned hosts are imported into.
    ///
    /// The rules are small but not obvious, and getting one wrong puts a user's imported hosts
    /// somewhere they did not ask for: a PuTTY node cannot hold imported connections, so the import
    /// falls back to the connection root, and selecting a connection means its parent folder rather
    /// than the connection itself.
    ///
    /// The logic lived inside a WinForms window and read the connection tree through a static
    /// singleton, which put it out of reach of the test suite entirely. Taking the two nodes it
    /// actually needs as arguments makes it ordinary code that can be checked.
    /// </summary>
    public static class PortScanDestinationResolver
    {
        /// <param name="selectedNode">
        /// What the user has selected in the connection tree, or null when nothing is selected.
        /// </param>
        /// <param name="connectionRoot">
        /// The tree's connection root, used both as the fallback when nothing is selected and as
        /// the destination when the selection cannot hold imported connections.
        /// </param>
        /// <returns>
        /// The container to import into, or null when there is nowhere sensible to put them.
        /// </returns>
        public static ContainerInfo? Resolve(ConnectionInfo? selectedNode, RootNodeInfo? connectionRoot)
        {
            ConnectionInfo? destination = selectedNode ?? connectionRoot;

            if (destination is null)
                return null;

            // A PuTTY session, and the node holding them, are read from PuTTY's own saved sessions.
            // Nothing can be imported into them, so imports go to the connection root instead.
            if (destination is RootPuttySessionsNodeInfo or PuttySessionInfo)
                destination = connectionRoot;

            if (destination is null)
                return null;

            // Selecting a connection means "next to this one", which is its parent folder.
            return destination as ContainerInfo ?? destination.Parent;
        }

        /// <summary>
        /// The connection root of a tree.
        ///
        /// RootPuttySessionsNodeInfo derives from RootNodeInfo, so an OfType&lt;RootNodeInfo&gt;()
        /// filter matches the PuTTY sessions root as well - and whichever of the two the tree
        /// happens to list first is what a bare First() returns. The window's original code had
        /// exactly that shape, which means the fallback meant to steer imports AWAY from PuTTY
        /// could land them there. Excluded explicitly.
        ///
        /// Returns null for a tree with no connection root, rather than throwing the way First()
        /// does.
        /// </summary>
        public static RootNodeInfo? ConnectionRootOf(System.Collections.Generic.IEnumerable<ConnectionInfo>? rootNodes) =>
            rootNodes?.OfType<RootNodeInfo>()
                      .FirstOrDefault(root => root is not RootPuttySessionsNodeInfo);
    }
}
