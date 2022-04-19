using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarksApp
{
    //平均值mean，众数mode，中值median 和 标准差stddev
    //均值:一般代表算术平均值
    //众数:众数是一组数据分布的峰值,是一种位置代表值.其优点是易于理解,不受极端值的影响
    //中位数:中位数是一组数据中间位置上的代表值.其特点是不受数据极端值的影响
    //标称差:Standard Deviation），在概率统计中最常使用作为统计分布程度（statistical ... 一个较大的标准差，代表大部分数值和其平均值之间差异较大；一个较小的标准差，代表 ... 标准差数值越大，代表回报远离过去平均数值，回报较不稳定故风险越高。

    //Mean 的意思是 Arithmetic mean of all measurements 所有测量的算术平均值
    //Error 的意思是 Half of 99.9% confidence interval 99.9% 一半的置信度区间
    //StdDev 是所有测量的标准偏差

    //ms是毫秒=0.001秒
    //us是微秒 = 0.000001秒
    //ns是纳秒 = 0.000000001秒
    //毫秒是一种较为微小的时间单位，1 毫秒 = 0.001 秒。0.000 000 001 毫秒 = 1皮秒 ；0.000 001 毫秒 = 1纳秒 ；0.001 毫秒 = 1微秒。典型照相机的最短曝光时间为一毫秒。
    //微秒，时间单位，符号μs（英语：microsecond ），1微秒等于百万分之一秒（10的负6次方秒），1毫秒等于千分之一秒（10的负3次方秒）。
    //ns（nanosecond）：纳秒，时间单位。一秒的十亿分之一，即等于10的负9次方秒。
    class Config : ManualConfig
    {
        public Config()
        {
            AddJob(BenchmarkDotNet.Jobs.Job.Default.WithRuntime(CoreRuntime.Core60));
            AddJob(BenchmarkDotNet.Jobs.Job.Default.WithRuntime(CoreRuntime.Core50));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net461));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net471));

            AddExporter(BenchmarkDotNet.Exporters.DefaultExporters.Markdown, BenchmarkDotNet.Exporters.DefaultExporters.JsonFull).KeepBenchmarkFiles(true).DontOverwriteResults(true);
            AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig()), MemoryDiagnoser.Default);
        }
    }
    class ConfigNet : ManualConfig
    {
        public ConfigNet()
        {
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net461));
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net471));

            AddExporter(BenchmarkDotNet.Exporters.DefaultExporters.Markdown, BenchmarkDotNet.Exporters.DefaultExporters.JsonFull).KeepBenchmarkFiles(true).DontOverwriteResults(true);
            AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig()), MemoryDiagnoser.Default);
        }
    }
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class SingleVsFirst
    {
        private readonly List<string> _haystack = new List<string>();
        private readonly int _haystackSize = 1000000;
        private readonly string _needle = "needle";

        public SingleVsFirst()
        {
            //Add a large amount of items to our list. 
            Enumerable.Range(1, _haystackSize).ToList().ForEach(x => _haystack.Add(x.ToString()));
            //Insert the needle right in the middle. 
            _haystack.Insert(_haystackSize / 2, _needle);
        }

        [Benchmark]
        public string Single() => _haystack.SingleOrDefault(x => x == _needle);

        [Benchmark]
        public string First() => _haystack.FirstOrDefault(x => x == _needle);

    }
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class SingleVsFirst2
    {
        private readonly List<string> _haystack = new List<string>();
        private readonly int _haystackSize = 1000000;

        public List<string> _needles => new List<string> { "StartNeedle", "MiddleNeedle", "EndNeedle" };

        public SingleVsFirst2()
        {
            //Add a large amount of items to our list. 
            Enumerable.Range(1, _haystackSize).ToList().ForEach(x => _haystack.Add(x.ToString()));

            //One at the start. 
            _haystack.Insert(0, _needles[0]);
            //One right in the middle. 
            _haystack.Insert(_haystackSize / 2, _needles[1]);
            //One at the end. 
            _haystack.Insert(_haystack.Count - 1, _needles[2]);
        }

        [ParamsSource(nameof(_needles))]
        public string Needle { get; set; }

        [Benchmark]
        public string Single() => _haystack.SingleOrDefault(x => x == Needle);

        [Benchmark]
        public string First() => _haystack.FirstOrDefault(x => x == Needle);

    }
    public static class Program
    {
        public static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<SingleVsFirst1>();
            //var summaryw = BenchmarkRunner.Run<SingleVsFirst2>();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            Console.ReadLine();
            
        }
    }
}