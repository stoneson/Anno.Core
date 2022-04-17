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
    [AnnoProxy(Channel = "Anno.Plugs.Tx3", Router = "Tm3")]
    public interface ITx3Service
    {
        [AnnoProxy(Method = "Tm3_action")]
        string Tm3_action(string name);

        [AnnoProxy(Method = "Tm3_action_Error")]
        string Tm3_action_Error(string name);

        [AnnoProxy(Method = "Tm3_action_Recover")]
        string Tm3_action_Recover(string name);

    }
    public class Tx3ServiceTest
    {
        public static void Handle()
        {
            Init();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var taskService = AnnoProxyBuilder.GetService<ITx3Service>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService：{stopWatch.Elapsed}");

            stopWatch.Restart();
            taskService = AnnoProxyBuilder.GetService<ITx3Service>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService2：{stopWatch.Elapsed}");

            //var helloWorldService = AnnoProxyBuilder.GetService<IHelloWorldService>();

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"--------------Start-{i}-------------------------------------------------------------");
                Console.WriteLine("Tm3_action:" + taskService.Tm3_action("Tm3_actionN"));
                Console.WriteLine("Tm3_action_Error:" + taskService.Tm3_action_Error("Tm3_action_ErrorN"));
                Console.WriteLine("Tm3_action_Recover:" + taskService.Tm3_action_Recover("Tm3_action_RecoverN"));

                //Console.WriteLine("SayHello:" + Newtonsoft.Json.JsonConvert.SerializeObject(helloWorldService.SayHello("Anno", 23)));
                Console.WriteLine($"--------------End-{i}-------------------------------------------------------------");
            }
        }
        static void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("Tx3ServiceTest", "127.0.0.1", 7010, false);
        }
    }
}
