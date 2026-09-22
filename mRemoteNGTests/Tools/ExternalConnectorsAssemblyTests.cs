using System;
using System.IO;
using System.Reflection;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using NUnit.Framework;

namespace mRemoteNGTests.Tools
{
    /// <summary>
    /// The gate that turns a quarantined ExternalConnectors.dll into a message instead of a
    /// crash on the first connect (#175/#191/#192).
    /// </summary>
    [TestFixture]
    public class ExternalConnectorsAssemblyTests
    {
        [Test]
        public void TheRealAssemblyLoadsInThisProcess()
        {
            // The test project references ExternalConnectors, so the file sits next to this DLL.
            // If this ever fails, the shipped layout is broken before any user sees it.
            Assert.That(ExternalConnectorsAssembly.IsAvailable, Is.True);
            Assert.That(File.Exists(ExternalConnectorsAssembly.ExpectedPath), Is.True, ExternalConnectorsAssembly.ExpectedPath);
        }

        [Test]
        public void EnsureAvailable_ReportsNothingWhenTheAssemblyLoads()
        {
            MessageCollector collector = new();

            bool available = ExternalConnectorsAssembly.EnsureAvailable(collector);

            Assert.That(available, Is.True);
        }

        [TestCase(typeof(FileNotFoundException))]
        [TestCase(typeof(FileLoadException))]
        [TestCase(typeof(BadImageFormatException))]
        public void TryLoad_ClassifiesLoaderFailuresAsUnavailable(Type exceptionType)
        {
            Exception thrown = (Exception)Activator.CreateInstance(exceptionType, "loader said no")!;

            bool loaded = ExternalConnectorsAssembly.TryLoad(() => throw thrown, out string? failure);

            Assert.That(loaded, Is.False);
            Assert.That(failure, Is.EqualTo("loader said no"));
        }

        [Test]
        public void TryLoad_LetsUnrelatedExceptionsThrough()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ExternalConnectorsAssembly.TryLoad(() => throw new InvalidOperationException("bug"), out _));
        }

        [Test]
        public void TryLoad_ReportsSuccessWithNoFailureText()
        {
            bool loaded = ExternalConnectorsAssembly.TryLoad(() => Assembly.GetExecutingAssembly(), out string? failure);

            Assert.That(loaded, Is.True);
            Assert.That(failure, Is.Null);
        }

        [Test]
        public void DescribeMissing_NamesTheFileThePathAndTheLikelyCause()
        {
            string text = ExternalConnectorsAssembly.DescribeMissing(@"C:\apps\mRemoteNG\ExternalConnectors.dll", fileExists: false, "Could not load file or assembly");

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("ExternalConnectors.dll"));
                Assert.That(text, Does.Contain(@"C:\apps\mRemoteNG\ExternalConnectors.dll"));
                Assert.That(text, Does.Contain("not there"));
                Assert.That(text, Does.Contain("quarantined"));
                Assert.That(text, Does.Contain("Could not load file or assembly"));
            });
        }

        [Test]
        public void DescribeMissing_DistinguishesAPresentFileThatWillNotLoad()
        {
            string text = ExternalConnectorsAssembly.DescribeMissing(@"C:\apps\mRemoteNG\ExternalConnectors.dll", fileExists: true, null);

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("refused it"));
                Assert.That(text, Does.Not.Contain("not there"));
                Assert.That(text, Does.Contain("Loader: -"));
            });
        }
    }
}
