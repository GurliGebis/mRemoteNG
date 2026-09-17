using System;
using mRemoteNG.App;

namespace mRemoteNG.Messages
{
    /// <summary>
    /// The production sink: everything goes to the application's message collector, exactly as it
    /// did when these operations called the singleton directly.
    ///
    /// The collector is resolved per call rather than captured in the constructor, because
    /// <see cref="Runtime.MessageCollector"/> is assigned during startup and an operation can
    /// outlive or precede that assignment. A message with nowhere to go is dropped rather than
    /// throwing: a scan must not fail because the notification panel is not up yet.
    /// </summary>
    public sealed class DefaultMessageSink : IOperationMessageSink
    {
        public void Information(string message, bool onlyLog = false) =>
            Send(MessageClass.InformationMsg, message, onlyLog);

        public void Warning(string message, bool onlyLog = false) =>
            Send(MessageClass.WarningMsg, message, onlyLog);

        public void Error(string message, bool onlyLog = false) =>
            Send(MessageClass.ErrorMsg, message, onlyLog);

        public void Exception(string message, Exception exception, bool onlyLog = false)
        {
            ArgumentNullException.ThrowIfNull(exception);
            Runtime.MessageCollector?.AddExceptionMessage(message, exception,
                                                          MessageClass.ErrorMsg, !onlyLog);
        }

        private static void Send(MessageClass messageClass, string message, bool onlyLog) =>
            Runtime.MessageCollector?.AddMessage(messageClass, message, onlyLog);
    }
}
