using System.Collections.Generic;
using mRemoteNG.Tools;
using NUnit.Framework;

namespace mRemoteNGTests.Tools
{
    public class PortListParserTests
    {
        // Hoisted so the expectation is named once and the analyzer stops flagging a fresh array
        // allocated on every call (CA1861).
        private static readonly int[] SshHttpHttpsRdp = [22, 80, 443, 3389];
        private static readonly int[] SshAndHttp = [22, 80];
        private static readonly int[] Ports8000To8003 = [8000, 8001, 8002, 8003];
        private static readonly int[] MixedListAndRange = [22, 80, 443, 8000, 8001, 8002];

        [Test]
        public void ListOfPortsIsParsed()
        {
            bool parsed = PortListParser.TryParse("22, 80, 443, 3389", out List<int> ports, out string error);

            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.True, error);
                Assert.That(ports, Is.EqualTo(SshHttpHttpsRdp));
            });
        }

        [TestCase("22;80 443")]
        [TestCase("  443 ,22,  80 ")]
        public void SemicolonsSpacesAndPaddingAreAccepted(string input)
        {
            bool parsed = PortListParser.TryParse(input, out List<int> ports, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(ports, Is.SupersetOf(SshAndHttp), error);
        }

        [Test]
        public void RangeIsExpanded()
        {
            bool parsed = PortListParser.TryParse("8000-8003", out List<int> ports, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(ports, Is.EqualTo(Ports8000To8003));
        }

        [Test]
        public void MixedListAndRangeIsSortedAndDeduplicated()
        {
            bool parsed = PortListParser.TryParse("443, 22, 80, 8000-8002, 80, 8001", out List<int> ports, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(ports, Is.EqualTo(MixedListAndRange));
        }

        [Test]
        public void ReversedRangeIsNormalised()
        {
            bool parsed = PortListParser.TryParse("8003-8000", out List<int> ports, out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(ports, Is.EqualTo(Ports8000To8003));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("0")]
        [TestCase("65536")]
        [TestCase("-1")]
        [TestCase("http")]
        [TestCase("22, abc")]
        [TestCase("8000-")]
        [TestCase("8000-99999")]
        public void InvalidInputIsRejectedWithAReason(string input)
        {
            bool parsed = PortListParser.TryParse(input, out List<int> ports, out string error);

            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(ports, Is.Empty);
                Assert.That(error, Is.Not.Empty);
            });
        }

        [Test]
        public void AllPortsCoversTheWholeRange()
        {
            List<int> ports = PortListParser.AllPorts();

            Assert.Multiple(() =>
            {
                Assert.That(ports, Has.Count.EqualTo(65535));
                Assert.That(ports[0], Is.EqualTo(PortListParser.MinPort));
                Assert.That(ports[^1], Is.EqualTo(PortListParser.MaxPort));
            });
        }
    }
}
