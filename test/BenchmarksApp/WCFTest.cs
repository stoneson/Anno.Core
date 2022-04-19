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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace BenchmarksApp
{
#if NETFRAMEWORK
    using System.ServiceModel;
    [AnnoProxy(Channel = "CNative.Modules.WCFTest", Router = "ServiceTest")]
    public interface IWCFTest
    {
        [AnnoProxy(Method = "DoWork")]
        /// <summary>
        /// 测试无返回值
        /// </summary>
        void DoWork();

        /// <summary>
        /// 测试返回集合
        /// </summary>
        /// <returns></returns>
        [AnnoProxy(Method = "HelloWorld")]
        List<WeatherForecast> HelloWorld();
        /// <summary>
        /// 测试值入参
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "GetInt")]
        int GetInt(int a, int b);
        /// <summary>
        /// 返回日期
        /// </summary>
        /// <returns></returns>
        [AnnoProxy(Method = "GetDate")]
        DateTime GetDate();
        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="summarie"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "getBySummarie")]
        WeatherForecast getBySummarie(string summarie);
        /// <summary>
        /// 入参为实体
        /// </summary>
        /// <param name="MyCar"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "AddCar")]
        string AddCar(MyCar MyCar);
        /// <summary>
        /// 两实体入参
        /// </summary>
        /// <param name="car1"></param>
        /// <param name="car2"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "AddCar2")]
        string AddCar2(MyCar car1, MyCar car2, int id);
        /// <summary>
        /// 入参实体集合
        /// </summary>
        /// <param name="cars"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "AddCars")]
        string AddCars(List<MyCar> cars);
        /// <summary>
        /// 入参实体集合加实体
        /// </summary>
        /// <param name="cars"></param>
        /// <param name="car2"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "AddCar2")]
        string AddCar2(List<MyCar> cars, MyCar car2, string name);
        /// <summary>
        /// 入参为实体内嵌实体
        /// </summary>
        /// <param name="wf"></param>
        /// <returns></returns>
        [AnnoProxy(Method = "AddWeather")]
        string AddWeather(WeatherForecast wf);
    }

    [Config(typeof(ConfigNet))]
    [MaxColumn, MinColumn, MemoryDiagnoser]
    public class BenchmarkWcfTest
    {
        private readonly List<string> _haystack = new List<string>();
        private readonly int _haystackSize = 1000000;

        public List<string> _needles => new List<string> { "StartNeedle", "MiddleNeedle", "EndNeedle" };
        private IWCFTest taskService;
        MyCar car;
        static void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("WCFTest", "127.0.0.1", 6660, false);
        }
        static BenchmarkWcfTest()
        {
            Init();
        }
        public BenchmarkWcfTest()
        {
            taskService = AnnoProxyBuilder.GetService<IWCFTest>();
            //Add a large amount of items to our list. 
            //Enumerable.Range(1, _haystackSize).ToList().ForEach(x => _haystack.Add(x.ToString()));

            //One at the start. 
            //_haystack.Insert(0, _needles[0]);
            //One right in the middle. 
            //_haystack.Insert(_haystackSize / 2, _needles[1]);
            //One at the end. 
            //_haystack.Insert(_haystack.Count - 1, _needles[2]);

            car = new MyCar()
            {
                id = "1" + 1,
                name = "ff",
                msg1 = "测试 " + 1,
                wf = new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb" } },
                wfList = new List<WeatherForecast>() {
                        new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb3" } }
                        }
            };
        }

        [ParamsSource(nameof(_needles))]
        public string Needle { get; set; }

        [Benchmark]
        public void DoWork() => taskService.DoWork();

        [Benchmark]
        public DateTime TaskSayHi() => taskService.GetDate();

        [Benchmark]
        public List<WeatherForecast> HelloWorld() => taskService.HelloWorld();

        [Benchmark]
        public string AddCar() => taskService.AddCar(car);

    }

    /// <summary>
    /// 测试WCF服务
    /// </summary>
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IServiceTest”。
    [ServiceContract]
    public interface IServiceTest
    {
        /// <summary>
        /// 测试无返回值
        /// </summary>
        [OperationContract]
        void DoWork();

        /// <summary>
        /// 测试返回集合
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<WeatherForecast> HelloWorld();
        /// <summary>
        /// 测试值入参
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [OperationContract]
        int GetInt(int a, int b);
        /// <summary>
        /// 返回日期
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DateTime GetDate();
        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="summarie"></param>
        /// <returns></returns>
        [OperationContract]
        WeatherForecast getBySummarie(string summarie);
        /// <summary>
        /// 入参为实体
        /// </summary>
        /// <param name="MyCar"></param>
        /// <returns></returns>
        [OperationContract]
        string AddCar(MyCar MyCar);
        /// <summary>
        /// 两实体入参
        /// </summary>
        /// <param name="car1"></param>
        /// <param name="car2"></param>
        /// <returns></returns>
        [OperationContract]
        string AddCar2(MyCar car1, MyCar car2, int id);
        /// <summary>
        /// 入参实体集合
        /// </summary>
        /// <param name="cars"></param>
        /// <returns></returns>
        [OperationContract]
        string AddCars(List<MyCar> cars);
        /// <summary>
        /// 入参实体集合加实体
        /// </summary>
        /// <param name="cars"></param>
        /// <param name="car2"></param>
        /// <returns></returns>
        [OperationContract]
        string AddCar2(List<MyCar> cars, MyCar car2, string name);
        /// <summary>
        /// 入参为实体内嵌实体
        /// </summary>
        /// <param name="wf"></param>
        /// <returns></returns>
        [OperationContract]
        string AddWeather(WeatherForecast wf);
    }
    public class WCFTest
    {
        public static void Handle()
        {
            Init();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var taskService = AnnoWcfProxyBuilder.GetService<IServiceTest>("CNative.Modules.WCFTest", "ServiceTest");
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService：{stopWatch.Elapsed}");

            stopWatch.Restart();
            taskService = AnnoWcfProxyBuilder.GetService<IServiceTest>("CNative.Modules.WCFTest", "ServiceTest");
            stopWatch.Stop();
            Console.WriteLine($"AnnoProxyBuilder.GetService2：{stopWatch.Elapsed}");

            //var helloWorldService = AnnoProxyBuilder.GetService<IHelloWorldService>();

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"--------------Start-{i}-------------------------------------------------------------");
                taskService.DoWork();
                Console.WriteLine("GetDate:" + (taskService.GetDate()));
                //Console.WriteLine("HelloWorld:" + Newtonsoft.Json.JsonConvert.SerializeObject(taskService.HelloWorld()));
                Console.WriteLine("getBySummarie:" + Newtonsoft.Json.JsonConvert.SerializeObject(taskService.getBySummarie("Bracing")));

                var car = new MyCar()
                {
                    id = "1" + i,
                    name = "ff",
                    msg1 = "测试 " + i,
                    wf = new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb" } },
                    wfList = new List<WeatherForecast>() {
                        new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb3" } }
                        }
                };
                var car2 = new MyCar()
                {
                    id = "1" + i,
                    name = "ff",
                    msg1 = "测试2 " + i,
                    wf = new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb" } },
                    wfList = new List<WeatherForecast>() {
                        new WeatherForecast() { code = 22, Date = DateTime.Now, myCar = new MyCar2() { name = "adb3" } }
                        }
                };
                Console.WriteLine("AddCar:" + (taskService.AddCar(car)));
                Console.WriteLine("AddCar2:" + (taskService.AddCar2(car, car, i)));
                //Console.WriteLine("SayHello:" + Newtonsoft.Json.JsonConvert.SerializeObject(helloWorldService.SayHello("Anno", 23)));
                Console.WriteLine($"--------------End-{i}-------------------------------------------------------------");
            }
        }
        static void Init()
        {
            DefaultConfigManager.SetDefaultConnectionPool(1000, Environment.ProcessorCount * 2, 50);
            DefaultConfigManager.SetDefaultConfiguration("WCFTest", "127.0.0.1", 6660, false);
        }
    }

    public class MyCar
    {
        // public MyCar() { }
        //[DataMember]
        public string id { get; set; }
        //[DataMember]
        public string name { get; set; }
        public string mycode2 { get; set; }
        /// <summary>
        /// 返回码
        /// </summary>
       // [DataMember]
        public int code1 { get; set; }
        /// <summary>
        /// 返回结果描述
        /// </summary>
        //[DataMember]
        public string msg1 { get; set; }
        // [DataMember]

        public List<WeatherForecast> wfList { get; set; }
        // [DataMember]
        public WeatherForecast wf { get; set; }

        public override string ToString()
        {
            return $"{id},{name},{code1},{mycode2},{msg1};wf:{wf}";
        }
    }
    public class MyCar2
    {
        // public MyCar() { }
        //[DataMember]
        public string id { get; set; }
        //[DataMember]
        public string name { get; set; }
        public string mycode2 { get; set; }
    }
    public class WeatherForecast
    {
        public WeatherForecast() { }
        /// <summary>
        /// 返回码
        /// </summary>
        //[DataMember]
        public int code { get; set; }
        /// <summary>
        /// 返回结果描述
        /// </summary>
        //[DataMember]
        public string msg { get; set; }
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }

        public MyCar2 myCar { get; set; }
        public List<MyCar2> MyCar2List { get; set; }
        public override string ToString()
        {
            return $"{Summary},{code},{msg},{Date},{TemperatureC}";
        }
    }
#endif
}
