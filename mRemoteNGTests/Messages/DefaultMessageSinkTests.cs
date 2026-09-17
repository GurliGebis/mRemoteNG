using System;
using mRemoteNG.Messages;
using NUnit.Framework;

namespace mRemoteNGTests.Messages
{
    /// <summary>
    /// The production sink: what every call site gets when nobody supplies one.
    /// </summary>
    [TestFixture]
    public class DefaultMessageSinkTests
    {
        [Test]
        public void EveryLevelReachesTheCollectorWithoutThrowing()
        {
            DefaultMessageSink sink = new();

            Assert.DoesNotThrow(() =>
            {
                sink.Information("information", true);
                sink.Warning("warning", true);
                sink.Error("error", true);
                sink.Exception("exception", new InvalidOperationException("boom"), true);
            });
        }

        [Test]
        public void ReportingAFailureWithNoExceptionIsAProgrammingError()
        {
            // Rather than logging a failure with nothing to say about it.
            DefaultMessageSink sink = new();

            Assert.Throws<ArgumentNullException>(() => sink.Exception("exception", null!));
        }
    }
}
