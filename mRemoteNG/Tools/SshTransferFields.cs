using System.IO;

namespace mRemoteNG.Tools
{
    /// <summary>
    /// The two decisions the SSH transfer window makes before a transfer starts: whether it has
    /// enough to go on, and what the remote path actually resolves to.
    ///
    /// Both lived inside a method called <c>AllFieldsSet</c> which, despite the name, also put a
    /// message box on the screen and rewrote a text box. Separating the questions from the
    /// conversation leaves the window with the part that genuinely needs a user in front of it, and
    /// puts the part that can be silently wrong within reach of the test suite.
    /// </summary>
    public static class SshTransferFields
    {
        /// <summary>
        /// Everything except the password, which is allowed to be empty once the user has said so.
        /// </summary>
        public static bool RequiredFieldsPresent(string? host, string? port, string? user,
                                                 string? localFile, string? remoteFile) =>
            !string.IsNullOrEmpty(host) &&
            !string.IsNullOrEmpty(port) &&
            !string.IsNullOrEmpty(user) &&
            !string.IsNullOrEmpty(localFile) &&
            !string.IsNullOrEmpty(remoteFile);

        /// <summary>
        /// A remote path ending in a separator names a directory, so the uploaded file keeps its
        /// own name.
        ///
        /// The window built that name with <c>LastIndexOf('\')</c>, which sees only the Windows
        /// separator. A local path written with forward slashes - legal on Windows, and what a path
        /// pasted from a URL or a shell looks like - contains no backslash at all, so the search
        /// returned -1, the substring started at 0, and the entire path was appended: a file at
        /// <c>C:/tmp/notes.txt</c> was uploaded to <c>/upload/C:/tmp/notes.txt</c> instead of
        /// <c>/upload/notes.txt</c>. <see cref="Path.GetFileName(string)"/> knows both separators.
        /// </summary>
        public static string ResolveRemotePath(string? remoteFile, string? localFile)
        {
            string remote = remoteFile ?? "";

            if (!remote.EndsWith('/') && !remote.EndsWith('\\'))
                return remote;

            return remote + Path.GetFileName(localFile ?? "");
        }
    }
}
