/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/8/18 18:01:41 
Functional description： AnnoRpcTest
******************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Anno.EngineData;
using Anno.Rpc.Client;
using Anno.Rpc.Client.DynamicProxy;

namespace Anno.Test
{
    [AnnoProxy(Channel = "Anno.Plugs.Tx1", Router = "Tm1")]
    public interface ITx1Service
    {
        [AnnoProxy(Method = "Buy")]
        string Buy(string name);

        [AnnoProxy(Method = "BuyError")]
        string BuyError(string name);

        [AnnoProxy(Method = "Tm3_action_Error")]
        string Tm3_action_Error(string name);

        [AnnoProxy(Method = "BuyTimeOut")]
        string BuyTimeOut(string name);

        [AnnoProxy(Method = "Buy_Recover")]
        string Buy_Recover(string name);
    }
    public class Tx1ServiceTest
    {
        public static void Handle()
        {
            Init();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var taskService = AnnoProxyBuilder.GetService<ITx1Service>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService：{stopWatch.Elapsed}");

            stopWatch.Restart();
            taskService = AnnoProxyBuilder.GetService<ITx1Service>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService2：{stopWatch.Elapsed}");

            //var helloWorldService = AnnoProxyBuilder.GetService<IHelloWorldService>();

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"--------------Start-{i}-------------------------------------------------------------");
                //Console.WriteLine("Buy:" + taskService.Buy("BuyN"));
                Console.WriteLine("BuyError:" + taskService.BuyError("BuyErrorN"));
                //Console.WriteLine("Tm3_action_Error:" + taskService.Tm3_action_Error("Tm3_action_ErrorN"));
                //Console.WriteLine("BuyTimeOut:" + taskService.BuyError("BuyTimeOutN"));
                //Console.WriteLine("Buy_Recover:" + taskService.BuyError("Buy_RecoverN"));

                //Console.WriteLine("SayHello:" + Newtonsoft.Json.JsonConvert.SerializeObject(helloWorldService.SayHello("Anno", 23)));
                Console.WriteLine($"--------------End-{i}-------------------------------------------------------------");
            }
        }
        static void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("Tx1ServiceTest", "127.0.0.1", 7010, false);
        }
    }
}
