using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using mRemoteNG.Messages;
using mRemoteNG.Resources.Language;

namespace mRemoteNG.Tools
{
    /// <summary>
    /// Answers one question, once per process: can <c>ExternalConnectors.dll</c> be loaded?
    ///
    /// That assembly holds the password-vault connectors (LAPS, Secret Server, Passwordstate,
    /// 1Password, Password Safe, Vault/OpenBao, AWS EC2), and the connect path calls into it
    /// directly. When the file is gone the failure does not surface as a clear error: the JIT
    /// cannot compile the method that references those types, so the very first connection
    /// attempt dies with a bare <see cref="FileNotFoundException"/> and the app crashes
    /// (#175, #191, #192). The reporter of #192 found the cause on their machine: Windows
    /// Defender had quarantined the DLL, a plausible false positive for a library whose whole
    /// job is to read credentials and launch other programs' CLIs.
    ///
    /// The probe here is made before that method is ever compiled, so a missing file becomes a
    /// message that says what is missing, where it was expected, and what most likely took it,
    /// instead of a crash report with no hint in it. A DLL that loaded once stays loaded for the
    /// life of the process (the image is mapped and cannot be removed under it), so the answer
    /// is cached.
    /// </summary>
    public static class ExternalConnectorsAssembly
    {
        public const string Name = "ExternalConnectors";

        private static readonly object Sync = new();
        private static bool? _available;
        private static string? _failure;

        /// <summary>Where the loader expects to find the file.</summary>
        public static string ExpectedPath => Path.Combine(AppContext.BaseDirectory, Name + ".dll");

        /// <summary>True when the assembly loads. Probed on first use, cached afterwards.</summary>
        public static bool IsAvailable
        {
            get
            {
                lock (Sync)
                {
                    if (_available is null)
                        _available = TryLoad(() => Assembly.Load(Name), out _failure);
                    return _available.Value;
                }
            }
        }

        /// <summary>
        /// Probes the assembly and, when it cannot be loaded, reports why through
        /// <paramref name="messageCollector"/>. Returns the availability so callers can bail out.
        /// </summary>
        public static bool EnsureAvailable(MessageCollector messageCollector)
        {
            ArgumentNullException.ThrowIfNull(messageCollector);

            if (IsAvailable)
                return true;

            string failure;
            lock (Sync)
            {
                failure = _failure ?? string.Empty;
            }

            messageCollector.AddMessage(MessageClass.ErrorMsg,
                                        DescribeMissing(ExpectedPath, File.Exists(ExpectedPath), failure));
            return false;
        }

        /// <summary>
        /// Runs <paramref name="load"/> and classifies the outcome. The three exceptions the
        /// loader raises for a file that is absent, unreadable or not a valid image mean "not
        /// available" and their text is kept for the report; anything else is a bug and is left
        /// to propagate.
        /// </summary>
        public static bool TryLoad(Func<Assembly> load, out string? failure)
        {
            ArgumentNullException.ThrowIfNull(load);

            try
            {
                _ = load();
                failure = null;
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
            {
                failure = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// The text shown to the user. Names the file and the folder it was expected in, and says
        /// whether the file is there at all, because that decides what the user should do next:
        /// a file that is gone points at quarantine; a file that is present but will not load
        /// points at a damaged or mismatched copy.
        /// </summary>
        public static string DescribeMissing(string expectedPath, bool fileExists, string? loaderMessage)
        {
            string state = fileExists
                ? Language.ExternalConnectorsPresentButUnloadable
                : Language.ExternalConnectorsFileGone;

            return string.Format(CultureInfo.CurrentCulture,
                                 Language.ExternalConnectorsMissing,
                                 Name + ".dll",
                                 expectedPath,
                                 state,
                                 string.IsNullOrWhiteSpace(loaderMessage) ? "-" : loaderMessage);
        }
    }
}
