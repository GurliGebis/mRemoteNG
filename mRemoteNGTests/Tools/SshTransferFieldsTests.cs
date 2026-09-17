using mRemoteNG.Tools;
using NUnit.Framework;

namespace mRemoteNGTests.Tools
{
    /// <summary>
    /// What the SSH transfer window decides before it opens a connection.
    ///
    /// None of this was reachable from the suite: the decisions read text boxes and showed a
    /// message box, so asking whether a remote path resolved correctly meant standing up a form.
    /// </summary>
    [TestFixture]
    public class SshTransferFieldsTests
    {
        [Test]
        public void EveryFieldPresentIsEnoughToStart()
        {
            Assert.That(SshTransferFields.RequiredFieldsPresent("host", "22", "user", @"C:\a.txt", "/tmp/"),
                        Is.True);
        }

        [TestCase(null, "22", "user", @"C:\a.txt", "/tmp/")]
        [TestCase("", "22", "user", @"C:\a.txt", "/tmp/")]
        [TestCase("host", "", "user", @"C:\a.txt", "/tmp/")]
        [TestCase("host", "22", "", @"C:\a.txt", "/tmp/")]
        [TestCase("host", "22", "user", "", "/tmp/")]
        [TestCase("host", "22", "user", @"C:\a.txt", "")]
        public void AnyMissingFieldStopsTheTransfer(string? host, string? port, string? user,
                                                    string? localFile, string? remoteFile)
        {
            Assert.That(SshTransferFields.RequiredFieldsPresent(host, port, user, localFile, remoteFile),
                        Is.False);
        }

        [Test]
        public void ThePasswordIsNotRequiredHere()
        {
            // Deliberately: an empty password is a question for the user, asked by the window, not
            // a field that is simply missing.
            Assert.That(SshTransferFields.RequiredFieldsPresent("host", "22", "user", @"C:\a.txt", "/tmp/"),
                        Is.True);
        }

        [Test]
        public void ARemotePathNamingAFileIsLeftAlone()
        {
            Assert.That(SshTransferFields.ResolveRemotePath("/tmp/renamed.txt", @"C:\work\notes.txt"),
                        Is.EqualTo("/tmp/renamed.txt"));
        }

        [Test]
        public void ARemoteDirectoryKeepsTheFilesOwnName()
        {
            Assert.That(SshTransferFields.ResolveRemotePath("/upload/", @"C:\work\notes.txt"),
                        Is.EqualTo("/upload/notes.txt"));
        }

        [Test]
        public void ABackslashAlsoEndsADirectory()
        {
            Assert.That(SshTransferFields.ResolveRemotePath(@"\upload\", @"C:\work\notes.txt"),
                        Is.EqualTo(@"\upload\notes.txt"));
        }

        [Test]
        public void AForwardSlashLocalPathUploadsTheFileNotThePath()
        {
            // The window searched for a backslash and found none, so it appended the whole path and
            // the file arrived at /upload/C:/work/notes.txt.
            Assert.That(SshTransferFields.ResolveRemotePath("/upload/", "C:/work/notes.txt"),
                        Is.EqualTo("/upload/notes.txt"));
        }

        [Test]
        public void ABareFileNameIsAlreadyTheName()
        {
            Assert.That(SshTransferFields.ResolveRemotePath("/upload/", "notes.txt"),
                        Is.EqualTo("/upload/notes.txt"));
        }

        [Test]
        public void NothingToAppendLeavesTheDirectoryAsItIs()
        {
            Assert.That(SshTransferFields.ResolveRemotePath("/upload/", @"C:\work\"),
                        Is.EqualTo("/upload/"));
            Assert.That(SshTransferFields.ResolveRemotePath("/upload/", null), Is.EqualTo("/upload/"));
        }

        [Test]
        public void NoRemotePathAtAllResolvesToNothingRatherThanThrowing()
        {
            Assert.That(SshTransferFields.ResolveRemotePath(null, @"C:\work\notes.txt"),
                        Is.EqualTo(""));
        }
    }
}
