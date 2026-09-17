using System;
using mRemoteNG.App;

namespace mRemoteNG.Messages
{
    /// <summary>
    /// The production sink: everything goes to the application's message collector, exactly as it
    /// did when these operations called the singleton directly.
    ///
    /// The collector is read per call rather than captured in the constructor, so a sink created
    /// early keeps working against whatever <see cref="Runtime.MessageCollector"/> resolves to
    /// later. The null-conditional is belt and braces - the property is initialised inline and
    /// cannot currently be null - and it means a message with nowhere to go is dropped rather than
    /// taking a scan down with it if that ever changes.
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
                                                          MessageClass.ErrorMsg, onlyLog);
        }

        private static void Send(MessageClass messageClass, string message, bool onlyLog) =>
            Runtime.MessageCollector?.AddMessage(messageClass, message, onlyLog);
    }
}
