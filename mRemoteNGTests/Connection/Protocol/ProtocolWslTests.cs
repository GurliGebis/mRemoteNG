using System;
using System.Reflection;
using mRemoteNG.Connection.Protocol.WSL;
using NUnit.Framework;

namespace mRemoteNGTests.Connection.Protocol;

// wsl.exe is launched directly, so no shell is involved - but "-d name" and "-u name" are each a
// switch followed by one argument, and wsl.exe can be told to run a command. A distribution or
// user name with whitespace, or starting with '-', became further wsl.exe arguments.
[TestFixture]
public class ProtocolWslTests
{
    [Test]
    public void NoHostname_MeansTheDefaultDistribution()
    {
        Assert.That(Build("", ""), Is.EqualTo(""));
    }

    [Test]
    public void Localhost_MeansTheDefaultDistributionToo()
    {
        Assert.That(Build("LocalHost", ""), Is.EqualTo(""));
    }

    [Test]
    public void DistributionName_BecomesDashD()
    {
        Assert.That(Build("Ubuntu-22.04", ""), Is.EqualTo("-d Ubuntu-22.04"));
    }

    [Test]
    public void Username_BecomesDashU()
    {
        Assert.That(Build("Ubuntu-22.04", "me"), Is.EqualTo("-d Ubuntu-22.04 -u me"));
        Assert.That(Build("", "me"), Is.EqualTo("-u me"));
    }

    [Test]
    public void ValuesAreTrimmed()
    {
        Assert.That(Build("  Ubuntu  ", "  me  "), Is.EqualTo("-d Ubuntu -u me"));
    }

    [TestCase("Ubuntu -- calc.exe")]
    [TestCase("Ubuntu -e calc.exe")]
    [TestCase("-e")]
    [TestCase("--exec")]
    [TestCase("Ubu\"ntu")]
    [TestCase("Ubuntu\nnewline")]
    public void DistributionThatCouldBecomeMoreArguments_Throws(string hostname)
    {
        AssertRefused(hostname, "", "distribution name");
    }

    [TestCase("me -- calc.exe")]
    [TestCase("-e")]
    [TestCase("m\"e")]
    public void UsernameThatCouldBecomeMoreArguments_Throws(string username)
    {
        AssertRefused("Ubuntu", username, "username");
    }

    [Test]
    public void RefusalDoesNotEchoTheValue()
    {
        var ex = Assert.Throws<TargetInvocationException>(() => Invoke("evil\r\nInjected-Log-Line", ""));
        Assert.That(ex!.InnerException, Is.TypeOf<ArgumentException>());
        Assert.That(ex.InnerException!.Message, Does.Not.Contain("Injected-Log-Line"));
    }

    private static string Build(string hostname, string username) => (string)Invoke(hostname, username)!;

    private static void AssertRefused(string hostname, string username, string field)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => Invoke(hostname, username));
        Assert.That(ex!.InnerException, Is.TypeOf<ArgumentException>());
        Assert.That(ex.InnerException!.Message, Does.Contain(field));
    }

    private static object? Invoke(string hostname, string username)
    {
        MethodInfo? method = typeof(ProtocolWSL).GetMethod("BuildWslArguments",
            BindingFlags.NonPublic | BindingFlags.Static, [typeof(string), typeof(string)]);

        if (method == null)
            throw new InvalidOperationException("static BuildWslArguments(string, string) not found.");

        return method.Invoke(null, [hostname, username]);
    }
}
