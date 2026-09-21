using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;

namespace mRemoteNG.Config.Putty
{
    /// <summary>
    /// Puts the saved-PuTTY-sessions roots into the connection tree, or takes them out, to match
    /// the "show saved PuTTY sessions" option.
    ///
    /// The option has existed since #389 and did nothing: the setting was read only by the page
    /// that sets it, and <c>ConnectionsService</c> appended the PuTTY roots unconditionally. This
    /// is the missing consumer. It works on the model, and the tree already reflects a root added
    /// to or removed from the model, so a change made in Options shows at once.
    ///
    /// Only the tree changes. The sessions themselves stay loaded - Quick Connect and the PuTTY
    /// options page keep seeing them - which is why the roots are removed from the model rather
    /// than the sessions being unloaded.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class PuttySessionsVisibility
    {
        public static void Apply(ConnectionTreeModel? model,
                                 IEnumerable<RootPuttySessionsNodeInfo>? puttyRoots,
                                 bool show)
        {
            if (model is null || puttyRoots is null)
                return;

            if (show)
            {
                // Appending matches how they are placed at load time: after the connection root.
                foreach (RootPuttySessionsNodeInfo root in puttyRoots.Where(r => !model.RootNodes.Contains(r)))
                    model.AddRootNode(root);
                return;
            }

            foreach (RootPuttySessionsNodeInfo root in model.RootNodes.OfType<RootPuttySessionsNodeInfo>().ToList())
                model.RemoveRootNode(root);
        }
    }
}
