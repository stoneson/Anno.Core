/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/8/18 18:02:49 
Functional description： ITaskService
******************************************************/
using Anno.EngineData;
using Anno.Rpc.Client.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Anno.Rpc.Client;

namespace BenchmarksApp
{
    [AnnoProxy(Channel = "Anno.Plugs.Viper", Router = "Exam")]
    public interface ITaskService
    {
        [AnnoProxy(Channel = "Anno.Plugs.Trace", Method = "GetServiceInstances", Router = "Router")]
        ActionResult ServiceInstances();

        [AnnoProxy(Method = "SayHi")]
        string CustomizeSayHi(string name);

        /// <summary>
        /// 异步
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "SayHi")]
        Task<string> TaskSayHi(string name);

        string SayHi(string name);


        [AnnoProxy(Method = "SayHi")]
        Task TaskVoidSayHi(string name);

        [AnnoProxy(Method = "SayHi")]
        void VoidSayHi(string name);

        [AnnoProxy(Method = "Add")]
        int Add(int x, int y);
        [AnnoProxy(Method = "Dynamic")]
        dynamic Dynamic();
        [AnnoProxy(Method = "Object")]
        object Object();
        [AnnoProxy(Method = "Dyn")]
        dynamic Dyn();

        [AnnoProxy(Method = "Dynamic")]
        UserDto DynamicReturnClass();

        [AnnoProxy(Method = "Dynamic")]
        Task<UserDto> DynamicReturnClassTask();

        [AnnoProxy(Channel = "Anno.Plugs.HelloWorld", Router = "HelloWorldTask", Method = "SayHello")]
        dynamic TaskSayHello(string name,int age);
    }

    public class TaskDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    public class UserDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [Config(typeof(ConfigNet))]
    [MaxColumn, MinColumn, MemoryDiagnoser]
    public class TaskServiceTest
    {
        private readonly List<string> _haystack = new List<string>();
        private readonly int _haystackSize = 1000000;

        public List<string> _needles => new List<string> { "StartNeedle", "MiddleNeedle", "EndNeedle" };
        private ITaskService taskService;
        static void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("TaskServiceTest", "127.0.0.1", 6660, false);
        }
        static TaskServiceTest()
        {
            Init();
        }
        public TaskServiceTest()
        {
            taskService = AnnoProxyBuilder.GetService<ITaskService>();
            //Add a large amount of items to our list. 
            //Enumerable.Range(1, _haystackSize).ToList().ForEach(x => _haystack.Add(x.ToString()));

            //One at the start. 
            //_haystack.Insert(0, _needles[0]);
            //One right in the middle. 
            //_haystack.Insert(_haystackSize / 2, _needles[1]);
            //One at the end. 
            //_haystack.Insert(_haystack.Count - 1, _needles[2]);
        }

        [ParamsSource(nameof(_needles))]
        public string Needle { get; set; }

        [Benchmark]
        public string TaskSayHi() => taskService.TaskSayHi(Needle).Result;

        [Benchmark]
        public string CustomizeSayHi() => taskService.CustomizeSayHi("杜燕明");

    }
}
