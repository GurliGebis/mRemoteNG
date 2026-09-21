using System;

namespace mRemoteNG.Connection.Protocol
{
    /// <summary>
    /// What a connection field may look like before it is placed on a console program's command
    /// line as a single argument.
    ///
    /// The console protocols (Terminal, OpenSSH, WSL) launch ssh.exe or wsl.exe directly, with no
    /// shell in between, so shell metacharacters cannot execute anything. What remains is
    /// argument injection: a value that the C runtime splits into more than one argv token, or
    /// that the program reads as an option instead of a target. The rules here are a whitelist by
    /// shape, and a value that fails them is refused - never trimmed into shape - because the
    /// fields come from the connections file, which is routinely shared, imported and synced.
    ///
    /// Written once so the three protocols cannot drift apart again; the Terminal protocol's copy
    /// of these rules was missing for six weeks after upstream fixed it there (#3335).
    /// </summary>
    public static class ConsoleArgument
    {
        /// <summary>
        /// True only for a value that reaches the program as exactly one argv token and cannot be
        /// read as an option: non-empty, no whitespace or control characters, no double quote,
        /// and not starting with '-'.
        ///
        /// The double quote is rejected because Windows argument parsing (CommandLineToArgvW)
        /// strips it, so a value written as "-oProxyCommand=..." does not start with '-' here yet
        /// arrives in argv as a token that does.
        /// </summary>
        public static bool IsSingleToken(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (value[0] == '-')
                return false;

            foreach (char c in value)
            {
                if (char.IsWhiteSpace(c) || char.IsControl(c) || c == '"')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// <see cref="IsSingleToken"/>, and additionally no '@' or '/'.
        ///
        /// A host name never contains either, but ssh reads both: "user@host" splits on the last
        /// '@', and "ssh://user@host:port" is a URI whose user, host and port silently replace the
        /// fields the profile displays. Neither runs a command; both let a connections file send
        /// the session somewhere other than what the user can see. IPv6 brackets contain neither
        /// character.
        /// </summary>
        public static bool IsHost(string? value) =>
            IsSingleToken(value) && !value!.Contains('@') && !value.Contains('/');

        /// <summary>
        /// A file path that can be placed inside a double-quoted argument. Everything a path may
        /// hold is fine except two things. The quote itself would end the argument early and turn
        /// the remainder into further arguments. And a trailing backslash does the opposite: the C
        /// runtime reads a backslash before a quote as escaping it, so an odd run of backslashes
        /// at the end of the path turns the closing quote into a literal and the argument swallows
        /// whatever follows it - verified against ssh.exe, where "C:\keys\" absorbed the destination.
        /// A file path never ends in a separator, so nothing legitimate is lost.
        /// </summary>
        public static bool IsQuotablePath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (char c in value)
            {
                if (c == '"' || char.IsControl(c))
                    return false;
            }

            return value[^1] != '\\';
        }

        /// <summary>
        /// The refusal, worded without the value: in the invalid case it may carry control
        /// characters or newlines that would land in logs and the UI verbatim.
        /// </summary>
        public static ArgumentException Refuse(string program, string field) =>
            new($"Refusing to start {program} session: the {field} contains characters that are not allowed.");
    }
}
