using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;
using System.Windows.Forms;
using mRemoteNG.App;
using mRemoteNG.Messages;
using mRemoteNG.Resources.Language;

namespace mRemoteNG.Connection.Protocol.WSL
{
    [SupportedOSPlatform("windows")]
    public class ProtocolWSL(ConnectionInfo connectionInfo) : ExternalProcessProtocolBase
    {
        #region Private Fields

        private readonly ConnectionInfo _connectionInfo = connectionInfo;
        private ConsoleControl.ConsoleControl? _consoleControl;

        #endregion

        #region Public Methods

        public override bool Connect()
        {
            try
            {
                // Check if WSL is installed
                if (!IsWslInstalled())
                {
                    Runtime.MessageCollector?.AddMessage(MessageClass.ErrorMsg, 
                        "WSL is not installed on this system. Please install WSL to use this protocol.", true);
                    return false;
                }

                Runtime.MessageCollector?.AddMessage(MessageClass.InformationMsg, 
                    "Attempting to start WSL session.", true);

                _consoleControl = new ConsoleControl.ConsoleControl
                {
                    Dock = DockStyle.Fill,
                    BackColor = ColorTranslator.FromHtml("#300A24"), // Ubuntu terminal color
                    ForeColor = Color.White,
                    IsInputEnabled = true,
                    Padding = new Padding(0, 20, 0, 0)
                };

                // Path to wsl.exe
                string wslExe = @"C:\Windows\System32\wsl.exe";
                
                // Build arguments based on connection info
                string arguments = BuildWslArguments();

                _consoleControl.StartProcess(wslExe, arguments);

                _handle = _consoleControl.Handle;
                NativeMethods.SetParent(_handle, InterfaceControl.Handle);

                Resize(this, EventArgs.Empty);
                base.Connect();
                return true;
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector?.AddExceptionMessage(Language.ConnectionFailed, ex);
                return false;
            }
        }

        private static bool IsWslInstalled()
        {
            try
            {
                // Check if wsl.exe exists
                string wslPath = @"C:\Windows\System32\wsl.exe";
                if (!File.Exists(wslPath))
                {
                    return false;
                }

                // Additional check: Try to execute wsl.exe --status to verify it's properly installed
                // For now, just check if the file exists
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string BuildWslArguments() =>
            BuildWslArguments(_connectionInfo.Hostname, _connectionInfo.Username);

        /// <summary>
        /// wsl.exe is launched directly, so no shell is involved; but "-d name" and "-u name" are
        /// each a switch followed by one argument, and wsl.exe itself can be told to run a command
        /// ("-- cmd", "-e cmd"). A distribution or user name containing whitespace, or starting
        /// with '-', would become further wsl.exe arguments. Both are validated and refused with
        /// the same rules as the SSH protocols (ConsoleArgument). An empty or "localhost" hostname
        /// means the default distribution, exactly as before.
        /// </summary>
        private static string BuildWslArguments(string? rawHostname, string? rawUsername)
        {
            string arguments = "";

            string hostname = (rawHostname ?? string.Empty).Trim();
            if (hostname.Length > 0 && !hostname.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                if (!ConsoleArgument.IsSingleToken(hostname))
                    throw ConsoleArgument.Refuse("WSL", "distribution name");

                arguments = $"-d {hostname}";
            }

            string username = (rawUsername ?? string.Empty).Trim();
            if (username.Length > 0)
            {
                if (!ConsoleArgument.IsSingleToken(username))
                    throw ConsoleArgument.Refuse("WSL", "username");

                arguments += $" -u {username}";
            }

            return arguments.Trim();
        }

        #endregion

        #region Enumerations

        public enum Defaults
        {
            Port = 0 // WSL doesn't use a traditional port
        }

        #endregion
    }
}
