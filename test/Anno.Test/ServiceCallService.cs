using Anno.Rpc.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Test
{
    [TestFixture]
    public class ServiceCallService
    {
        [Test]
        public void BuyProduct_test()
        {
            Dictionary<string, string> input = new Dictionary<string, string>();

            input.Add("channel", "Anno.Plugs.HelloWorld");
            input.Add("router", "HelloWorldViper");
            input.Add("method", "BuyProduct");
            input.Add("productName", "ThinkBook 14");
            input.Add("number", "6");
            var x = Connector.BrokerDns(input);
            Console.WriteLine(x);
            Log.Log.Debug("method = BuyProduct_test,rets = " + x);
            Assert.IsTrue(x.IndexOf("true") > 0);
        }

        [SetUp]
        public void SetUp()
        {
            Console.OutputEncoding = Encoding.UTF8;
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 100);
            DefaultConfigManager.SetDefaultConfiguration("ServiceCallService", "127.0.0.1", 6660, false);
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("我是清理者");
        }
    }
}
