using mRemoteNG.Connection.Protocol;
using NUnit.Framework;

namespace mRemoteNGTests.Connection.Protocol;

/// <summary>
/// The shape rules every console protocol applies to a connection field before it becomes an
/// argument. Each rule is pinned with the value that motivated it.
/// </summary>
[TestFixture]
public class ConsoleArgumentTests
{
    [TestCase("server.example.com")]
    [TestCase("10.0.0.1")]
    [TestCase("[fe80::1]")]
    [TestCase("host_with.dots-and_underscores")]
    [TestCase("Ubuntu-22.04")]
    public void PlainValuesAreOneToken(string value)
    {
        Assert.That(ConsoleArgument.IsSingleToken(value), Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("host with space")]
    [TestCase("host\ttab")]
    [TestCase("host\nnewline")]
    [TestCase("host\rreturn")]
    [TestCase("host\u0000nul")]
    [TestCase("-oProxyCommand=calc.exe")]
    [TestCase("\"-oProxyCommand=calc.exe\"")]
    [TestCase("ho\"st")]
    public void ValuesThatSplitOrBecomeAnOptionAreRefused(string? value)
    {
        Assert.That(ConsoleArgument.IsSingleToken(value), Is.False);
    }

    [Test]
    public void AHostIsAlsoRefusedWhenSshWouldReadItAsUserOrUri()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ConsoleArgument.IsHost("evilhost@realhost"), Is.False);
            Assert.That(ConsoleArgument.IsHost("ssh://ruser@evilhost:31337"), Is.False);
            Assert.That(ConsoleArgument.IsHost("host/path"), Is.False);
            Assert.That(ConsoleArgument.IsHost("[fe80::1]"), Is.True, "IPv6 brackets contain neither character");
            Assert.That(ConsoleArgument.IsHost("server.example.com"), Is.True);
        });
    }

    [Test]
    public void AKeyPathMayHoldAnythingButAQuote()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ConsoleArgument.IsQuotablePath(@"C:\Users\me\.ssh\id_ed25519"), Is.True);
            Assert.That(ConsoleArgument.IsQuotablePath(@"C:\Users\me and you\.ssh\id_ed25519"), Is.True,
                        "spaces are fine inside quotes");
            Assert.That(ConsoleArgument.IsQuotablePath("C:\\keys\\x\" -oProxyCommand=calc.exe \""), Is.False,
                        "a quote ends the argument early and the rest becomes arguments");
            Assert.That(ConsoleArgument.IsQuotablePath("C:\\keys\\x\nfoo"), Is.False);
            Assert.That(ConsoleArgument.IsQuotablePath("C:\\keys\\"), Is.False,
                        "an odd trailing backslash escapes the closing quote and the argument swallows what follows");
            Assert.That(ConsoleArgument.IsQuotablePath("C:\\keys\\\\\\"), Is.False);
            Assert.That(ConsoleArgument.IsQuotablePath(""), Is.False);
            Assert.That(ConsoleArgument.IsQuotablePath(null), Is.False);
        });
    }

    [Test]
    public void TheRefusalNamesTheFieldAndNotTheValue()
    {
        var ex = ConsoleArgument.Refuse("SSH", "hostname");

        Assert.That(ex.Message, Is.EqualTo("Refusing to start SSH session: the hostname contains characters that are not allowed."));
    }
}
