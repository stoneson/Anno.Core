using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTools;

namespace IPRange.Test
{
    [NUnit.Framework.TestFixture]
    public class IPAddressRangeParseTest
    {
        //public TestContext TestContext { get; set; }
        [NUnit.Framework.SetUp]
        public void InitConfig()
        {
            //Console.WriteLine("InitConfig");
        }

        [NUnit.Framework.TearDown]
        public void TearDown()
        {
            //Console.WriteLine("我是清理者");
        }

        [NUnit.Framework.Test]
        [NUnit.Framework.TestCase("192.168.60.13", "192.168.60.13", "192.168.60.13")]
        [NUnit.Framework.TestCase("  192.168.60.13  ", "192.168.60.13", "192.168.60.13")]
        [NUnit.Framework.TestCase("fe80::d503:4ee:3882:c586", "fe80::d503:4ee:3882:c586", "fe80::d503:4ee:3882:c586")]
        [NUnit.Framework.TestCase("  fe80::d503:4ee:3882:c586  ", "fe80::d503:4ee:3882:c586", "fe80::d503:4ee:3882:c586")]
        [NUnit.Framework.TestCase("::/0", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [NUnit.Framework.TestCase("3232252004", "192.168.64.100", "192.168.64.100")] // decimal - new 
        [NUnit.Framework.TestCase("  3232252004  ", "192.168.64.100", "192.168.64.100")] // decimal - new 

        [NUnit.Framework.TestCase("219.165.64.0/19", "219.165.64.0", "219.165.95.255")]
        [NUnit.Framework.TestCase("  219.165.64.0  /  19  ", "219.165.64.0", "219.165.95.255")]
        [NUnit.Framework.TestCase("192.168.1.0/255.255.255.0", "192.168.1.0", "192.168.1.255")]
        [NUnit.Framework.TestCase("  192.168.1.0  /  255.255.255.0  ", "192.168.1.0", "192.168.1.255")]
        [NUnit.Framework.TestCase("3232252004/24", "192.168.64.0", "192.168.64.255")] // decimal - new 
        [NUnit.Framework.TestCase("  3232252004  /  24  ", "192.168.64.0", "192.168.64.255")] // decimal - new 

        [NUnit.Framework.TestCase("192.168.60.26–192.168.60.37", "192.168.60.26", "192.168.60.37")]
        [NUnit.Framework.TestCase("  192.168.60.26  –  192.168.60.37  ", "192.168.60.26", "192.168.60.37")]
        [NUnit.Framework.TestCase("fe80::c586-fe80::c600", "fe80::c586", "fe80::c600")]
        [NUnit.Framework.TestCase("  fe80::c586  -  fe80::c600  ", "fe80::c586", "fe80::c600")]
        [NUnit.Framework.TestCase("3232252004-3232252504", "192.168.64.100", "192.168.66.88")]
        [NUnit.Framework.TestCase("  3232252004  -  3232252504  ", "192.168.64.100", "192.168.66.88")]
        [NUnit.Framework.TestCase("192.168.1- 192.168.1111", "192.168.0.1", "192.168.4.87")] // 3 part IPv4
        [NUnit.Framework.TestCase("173.1 -173.1111", "173.0.0.1", "173.0.4.87")] // 2 part IPv4

        // with "dash (–)" (0x2013) is also support.
        [NUnit.Framework.TestCase("192.168.61.26–192.168.61.37", "192.168.61.26", "192.168.61.37")]
        [NUnit.Framework.TestCase("  192.168.61.26  –  192.168.61.37  ", "192.168.61.26", "192.168.61.37")]
        [NUnit.Framework.TestCase("fe80::c586–fe80::c600", "fe80::c586", "fe80::c600")]
        [NUnit.Framework.TestCase("  fe80::c586  –  fe80::c600  ", "fe80::c586", "fe80::c600")]
        [NUnit.Framework.TestCase("3232252004–3232252504", "192.168.64.100", "192.168.66.88")]
        [NUnit.Framework.TestCase("  3232252004  –  3232252504  ", "192.168.64.100", "192.168.66.88")]
        [NUnit.Framework.TestCase("192.168.1.1-7", "192.168.1.1", "192.168.1.7")]

        // IPv6 with scope id (scope id should be stripped in begin/end properties.)
        [NUnit.Framework.TestCase("fe80::0%eth0/112", "fe80::", "fe80::ffff")]
        [NUnit.Framework.TestCase("fe80::8000%12-fe80::80ff%12", "fe80::8000", "fe80::80ff")]
        [NUnit.Framework.TestCase("fe80::1%lo1", "fe80::1", "fe80::1")]

        // IPv4 mapped to IPv6
        [NUnit.Framework.TestCase("::ffff:10.0.0.0/120", "::ffff:10.0.0.0", "::ffff:10.0.0.255")]
        [NUnit.Framework.TestCase("::ffff:192.168.10.20-::ffff:192.168.11.20", "::ffff:192.168.10.20", "::ffff:192.168.11.20")]
        [NUnit.Framework.TestCase("::ffff:10.0.0.203", "::ffff:10.0.0.203", "::ffff:10.0.0.203")]
        [NUnit.Framework.TestCase("::ffff:10.0.2.0/ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "::ffff:10.0.2.0", "::ffff:10.0.2.255")]
        public void ParseSucceeds(string input, string expectedBegin, string expectedEnd)
        {
            //TestContext.Run((string input, string expectedBegin, string expectedEnd) =>
            //{
                Console.WriteLine("TestCase: \"{0}\", Expected Begin: {1}, End: {2}", input, expectedBegin, expectedEnd);
                var range = IPAddressRange.Parse(input);
                //range.IsNotNull();
                Console.WriteLine("  Result: Begin: {0}, End: {1}", range.Begin, range.End);
                //range.Begin.ToString().Is(expectedBegin);
                //range.End.ToString().Is(expectedEnd);
            //});
        }

         [NUnit.Framework.Test]
        [NUnit.Framework.TestCase(null, typeof(ArgumentNullException))]
        [NUnit.Framework.TestCase("", typeof(FormatException))]
        [NUnit.Framework.TestCase(" ", typeof(FormatException))]
        [NUnit.Framework.TestCase("gvvdv", typeof(FormatException))]
        [NUnit.Framework.TestCase("192.168.0.10/48", typeof(FormatException))] // out of CIDR range 
        [NUnit.Framework.TestCase("192.168.0.10-192.168.0.5", typeof(ArgumentException))] // bigger to lower
        [NUnit.Framework.TestCase("fe80::2%eth1-fe80::1%eth1", typeof(ArgumentException))] // bigger to lower
        [NUnit.Framework.TestCase("10.256.1.1", typeof(FormatException))] // invalid ip
        [NUnit.Framework.TestCase("127.0.0.1%1", typeof(FormatException))] // ipv4, but with scope id
        [NUnit.Framework.TestCase("192.168.0.10/2.3.4", typeof(FormatException))] // ipv4, but subnet mask isn't linear 
        [NUnit.Framework.TestCase("192.168.0.0-192.168.0.1%1", typeof(FormatException))] // ipv4, but with scope id at end of range
        [NUnit.Framework.TestCase("192.168.0.0%1-192.168.0.1", typeof(FormatException))] // ipv4, but with scope id at begin of range
        [NUnit.Framework.TestCase("192.168.0.0%1-192.168.0.1%1", typeof(FormatException))] // ipv4, but with scope id at both of begin and end
        [NUnit.Framework.TestCase("192.168.0.0%1/24", typeof(FormatException))] // CIDR ipv4, but with scope id
        [NUnit.Framework.TestCase("192.168.0.0%1/255.255.255.0", typeof(FormatException))] // ipv4 and subnet mask, but with scope id
        [NUnit.Framework.TestCase("192.168-::1", typeof(FormatException))] // Invalid comibination of IPv4 and IPv6
        [NUnit.Framework.TestCase("192.168.0.0-256", typeof(FormatException))] // shortcut notation, but out of range
        [NUnit.Framework.TestCase("192.168.0.0-1%1", typeof(FormatException))] // ipv4 shortcut, but with scope id at end of range
        [NUnit.Framework.TestCase("192.168.0.0%1-1", typeof(FormatException))] // ipv4 shortcut, but with scope id at begin of range
        [NUnit.Framework.TestCase("172. 13.0.0/24", typeof(FormatException))] // ipv4, but include spaces
        [NUnit.Framework.TestCase("fe80::0-fe80: :ffff", typeof(FormatException))] // ipv4, but include spaces
        public void ParseFails(string input, Type expectedException)
        {
            //TestContext.Run((string input, Type expectedException) =>
            //{
                Console.WriteLine("TestCase: \"{0}\", Expected Exception: {1}", input, expectedException.Name);
                try
                {
                    IPAddressRange.Parse(input);
                NUnit.Framework.Assert.Fail("Expected exception of type {0} to be thrown for input \"{1}\"", expectedException.Name, input);
                }
                catch (NUnit.Framework.AssertionException)
                {
                    throw; // allow Assert.Fail to pass through 
                }
                catch (Exception ex)
                {
                    ex.GetType().Is(expectedException);
                }
            //});
        }

         [NUnit.Framework.Test]
        [NUnit.Framework.TestCase(null, false)] // bug3
        [NUnit.Framework.TestCase("", false)]
        [NUnit.Framework.TestCase(" ", false)]
        [NUnit.Framework.TestCase("fdfv", false)]
        [NUnit.Framework.TestCase("192.168.0.10/48", false)] // CIDR out of range
        [NUnit.Framework.TestCase("192.168.60.26-192.168.60.22", false)] // big to lower

        [NUnit.Framework.TestCase("192.168.60.13", true)]
        [NUnit.Framework.TestCase("fe80::d503:4ee:3882:c586", true)]
        [NUnit.Framework.TestCase("fe80:db8::dead:beaf%eth2", true)]
        [NUnit.Framework.TestCase("219.165.64.0/19", true)]
        [NUnit.Framework.TestCase("219.165.64.73/32", true)]
        [NUnit.Framework.TestCase("192.168.1.0/255.255.255.0", true)]
        [NUnit.Framework.TestCase("192.168.60.26-192.168.60.37", true)]
        [NUnit.Framework.TestCase("fe80:dead::beaf:a%eth2-fe80:dead::beaf:f%eth2", true)]
        [NUnit.Framework.TestCase("fe80:dead::beaf:f%eth2-fe80:dead::beaf:a%eth2", false)]
        public void TryParse(string input, bool expectedReturn)
        {
            //TestContext.Run((string input, bool expectedReturn) =>
            //{
                Console.WriteLine("TestCase: \"{0}\", Expected: {1}", input, expectedReturn);
                IPAddressRange temp;
                var result = IPAddressRange.TryParse(input, out temp);
                result.Is(expectedReturn);
                if (expectedReturn)
                {
                    temp.IsNotNull();
                }
                else
                {
                    temp.IsNull();
                }
            //});
        }
    }
}
