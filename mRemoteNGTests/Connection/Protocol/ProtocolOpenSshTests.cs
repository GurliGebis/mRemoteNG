using System;
using System.Reflection;
using mRemoteNG.Connection.Protocol.SSH;
using NUnit.Framework;

namespace mRemoteNGTests.Connection.Protocol;

// The OpenSSH protocol always launched ssh.exe directly, so it never had the Terminal protocol's
// shell injection - but it built its arguments from the raw Hostname/Username fields with no
// validation, so a hostname of "-oProxyCommand=..." was argument injection into ssh itself.
// Same rules as the Terminal protocol now; these mirror ProtocolTerminalTests.
[TestFixture]
public class ProtocolOpenSshTests
{
    private const string Key = @"C:\Users\me\.ssh\id_ed25519";

    #region Valid targets

    [Test]
    public void HostnameOnly_ReturnsHost()
    {
        Assert.That(Build("server.example.com", "", 22, "", Key), Is.EqualTo($"-i \"{Key}\" server.example.com"));
    }

    [Test]
    public void WithUsername_ReturnsUserAtHost()
    {
        Assert.That(Build("server.example.com", "admin", 22, "", Key), Is.EqualTo($"-i \"{Key}\" admin@server.example.com"));
    }

    [Test]
    public void NonDefaultPort_PrependsPortFlag()
    {
        Assert.That(Build("host", "admin", 2222, "", Key), Is.EqualTo($"-p 2222 -i \"{Key}\" admin@host"));
    }

    [Test]
    public void SshOptionsArePassedThroughAsTheUsersOwn()
    {
        // The field exists to carry options; it is not validated, and that is recorded as a
        // property of the feature.
        Assert.That(Build("host", "", 22, "-o StrictHostKeyChecking=accept-new", Key),
                    Is.EqualTo($"-o StrictHostKeyChecking=accept-new -i \"{Key}\" host"));
    }

    [Test]
    public void Ipv6Address_IsAllowed()
    {
        Assert.That(Build("[fe80::1]", "root", 22, "", Key), Is.EqualTo($"-i \"{Key}\" root@[fe80::1]"));
    }

    [Test]
    public void AKeyPathWithSpaces_IsQuotedNotRefused()
    {
        const string key = @"C:\Users\me and you\.ssh\id_ed25519";
        Assert.That(Build("host", "", 22, "", key), Is.EqualTo($"-i \"{key}\" host"));
    }

    #endregion

    #region Injection attempts are refused

    [TestCase("-oProxyCommand=calc.exe")]
    [TestCase("\"-oProxyCommand=calc.exe\"")]
    [TestCase("host -oProxyCommand=calc.exe")]
    [TestCase("host\ttab")]
    [TestCase("host\nnewline")]
    [TestCase("ho\"st")]
    [TestCase("evilhost@realhost")]
    [TestCase("ssh://ruser@evilhost:31337")]
    [TestCase("")]
    public void HostThatCouldBecomeAnOptionOrRedirect_Throws(string hostname)
    {
        AssertRefused(hostname, "", "", Key, "hostname");
    }

    [TestCase("-oProxyCommand=calc.exe")]
    [TestCase("admin -oProxyCommand=calc.exe")]
    [TestCase("ad\"min")]
    public void UsernameThatCouldBecomeAnOption_Throws(string username)
    {
        AssertRefused("host", username, "", Key, "username");
    }

    [Test]
    public void KeyPathWithAQuote_Throws()
    {
        // -i "<path>" : a quote inside the path ends the argument and the remainder is parsed as
        // further ssh arguments.
        AssertRefused("host", "", "", "C:\\keys\\x\" -oProxyCommand=calc.exe \"", "private key path");
    }

    [TestCase("C:\\fakekey\\")]
    [TestCase("C:\\fakekey\\\\\\")]
    public void KeyPathEndingInABackslash_Throws(string keyPath)
    {
        // The CRT reads a backslash before a quote as escaping it, so -i "C:\fakekey\" never closes
        // and the destination that follows is swallowed into the key path. Verified against ssh.exe.
        AssertRefused("host", "", "", keyPath, "private key path");
    }

    [Test]
    public void RefusalDoesNotEchoTheValue()
    {
        var ex = Assert.Throws<TargetInvocationException>(() => Invoke("evil\r\nInjected-Log-Line", "", 22, "", Key));
        Assert.That(ex!.InnerException, Is.TypeOf<ArgumentException>());
        Assert.That(ex.InnerException!.Message, Does.Not.Contain("Injected-Log-Line"));
        Assert.That(ex.InnerException.Message, Does.Not.Contain("\n"));
    }

    #endregion

    #region Helpers

    private static string Build(string hostname, string username, int port, string options, string keyPath) =>
        (string)Invoke(hostname, username, port, options, keyPath)!;

    private static void AssertRefused(string hostname, string username, string options, string keyPath, string field)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => Invoke(hostname, username, 22, options, keyPath));
        Assert.That(ex!.InnerException, Is.TypeOf<ArgumentException>());
        Assert.That(ex.InnerException!.Message, Does.Contain(field));
    }

    private static object? Invoke(string hostname, string username, int port, string options, string keyPath)
    {
        MethodInfo? method = typeof(ProtocolOpenSSH).GetMethod("BuildSshArguments",
            BindingFlags.NonPublic | BindingFlags.Static,
            [typeof(string), typeof(string), typeof(int), typeof(string), typeof(string)]);

        if (method == null)
            throw new InvalidOperationException("static BuildSshArguments(string, string, int, string, string) not found.");

        return method.Invoke(null, [hostname, username, port, options, keyPath]);
    }

    #endregion
}
