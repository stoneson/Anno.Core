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
using Anno.Rpc.Client;
using Anno.Rpc.Client.DynamicProxy;

namespace Anno.Test
{
    public class AnnoRpcTest
    {
        public static void Handle()
        {
            Init();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //var proxys = AnnoProxyBuilder.GetServices(typeof(AnnoRpcTest).Assembly);
            //proxys.TryGetValue(typeof(ITaskService), out object taskServiceObj);
            //ITaskService taskService=taskServiceObj as ITaskService;

            var taskService = AnnoProxyBuilder.GetService<ITaskService>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService：{stopWatch.Elapsed}");

            stopWatch.Restart();
            taskService = AnnoProxyBuilder.GetService<ITaskService>();
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService2：{stopWatch.Elapsed}");

            //var helloWorldService = AnnoProxyBuilder.GetService<IHelloWorldService>();

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"--------------Start-{i}-------------------------------------------------------------");

                var rlt1 = taskService.ServiceInstances();

                var ss = taskService.TaskSayHi("杜燕明").Result;
                Console.WriteLine("TaskSayHi:" + ss);
                taskService.TaskVoidSayHi("TaskVoid").Wait();
                taskService.VoidSayHi("Void");
                Console.WriteLine("ServiceInstances:" + Newtonsoft.Json.JsonConvert.SerializeObject(rlt1));
                Console.WriteLine("CustomizeSayHi:" + taskService.CustomizeSayHi("AnnoProxy"));
                Console.WriteLine("Add:" + taskService.Add(6, 8));
                Console.WriteLine("Dyn:" + taskService.Dyn());
                Console.WriteLine("Object:" + taskService.Object());
                Console.WriteLine("Dynamic:" + taskService.Dynamic());

                Console.WriteLine("DynamicReturnClass:" + Newtonsoft.Json.JsonConvert.SerializeObject(taskService.DynamicReturnClass()));
                Console.WriteLine("DynamicReturnClassTask:" + Newtonsoft.Json.JsonConvert.SerializeObject(taskService.DynamicReturnClassTask().Result));

                var rlt = taskService.SayHi(null);

                var rlt2 = taskService.TaskSayHello("AnnoGrpc", 26);
                Console.WriteLine("SayHello-AnnoGrpc:" + Newtonsoft.Json.JsonConvert.SerializeObject(rlt2));

                Console.WriteLine($"--------------End-{i}-------------------------------------------------------------");
            }
        }
        static void Init()
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            //AnnoProxyBuilder.Init();
            //stopWatch.Stop();
            //Console.WriteLine($"AnnoProxyBuilder.Init(false)：{stopWatch.Elapsed}");
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("RpcTest", "127.0.0.1", 6660, false);
        }
    }
}
