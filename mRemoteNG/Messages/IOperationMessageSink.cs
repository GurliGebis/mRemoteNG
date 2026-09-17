using System;

namespace mRemoteNG.Messages
{
    /// <summary>
    /// Where a background operation reports what it is doing.
    ///
    /// The scanner and the file transfer are ordinary classes with no dependency on WinForms, and
    /// they could have been unit-tested from the day they were written — except that both talk to
    /// <c>Runtime.MessageCollector</c>, a process-wide singleton wired to the running application's
    /// notification panel. A test that constructs a scanner therefore constructs the application's
    /// messaging stack with it, which is why neither class has ever been reachable from the suite.
    ///
    /// One small seam fixes that. Production keeps the singleton through
    /// <see cref="DefaultMessageSink"/>, which stays the default argument, so no call site has to
    /// change and no behaviour moves.
    /// </summary>
    public interface IOperationMessageSink
    {
        /// <param name="onlyLog">
        /// True for detail that belongs in the log but not in the user's face — the same meaning
        /// the message collector has always given this flag.
        /// </param>
        void Information(string message, bool onlyLog = false);

        void Warning(string message, bool onlyLog = false);

        void Error(string message, bool onlyLog = false);

        /// <summary>Reports a failure together with the exception that caused it.</summary>
        void Exception(string message, Exception exception, bool onlyLog = false);
    }
}
