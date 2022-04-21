using Confluent.Kafka;
using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Anno.EventBus.Executor.Kafka
{
    //public class KafkaConsumer2 : AbstractKafkaMessage<KafkaSubscribeConfig>, ISubscribeChannel
    //{
    //    /// <summary>
    //    /// 构造方法
    //    /// </summary>
    //    public KafkaConsumer2()
    //    {
    //        this.MQConfig = new KafkaSubscribeConfig();
    //        //配置参数校验
    //        MQConfig.Check();
    //    }
    //    /// <summary>
    //    /// 构造方法
    //    /// </summary>
    //    /// <param name="brokerUri"></param>
    //    /// <param name="username"></param>
    //    /// <param name="password"></param>
    //    public KafkaConsumer2(KafkaSubscribeConfig config)
    //    {
    //        if (config != null)
    //            this.MQConfig = config;
    //        //配置参数校验
    //        MQConfig.Check();
    //    }
    //    //public static string brokerUrl = ConfigurationManager.AppSettings["Broker"];
    //    //public static string topic = ConfigurationManager.AppSettings["ConsumeTopic"];
    //    //public static string groupid = ConfigurationManager.AppSettings["GroupID"];
    //    //public static string consumercount = ConfigurationManager.AppSettings["ConsumerCount"];
    //    public void Consume(string topic)
    //    {
    //        var mode = "consume";
    //        var brokerList = brokerUrl;
    //        List<string> topics = new List<string>(topic.Split(','));

    //        try
    //        {
    //            CancellationTokenSource cts = new CancellationTokenSource();
    //            Console.CancelKeyPress += (_, e) =>
    //            {
    //                e.Cancel = true; // prevent the process from terminating.
    //                cts.Cancel();
    //            };

    //            switch (mode)
    //            {
    //                case "consume":
    //                    Run_Consume(brokerList, topics, cts.Token);
    //                    break;
    //                case "manual":
    //                    Run_ManualAssign(brokerList, topics, cts.Token);
    //                    break;
    //                default:
    //                    PrintUsage();
    //                    break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WriteError(DateTime.Now.ToString() + ex.Message);
    //        }

    //    }
    //    /// <summary>
    //    ///     In this example
    //    ///         - offsets are manually committed.
    //    ///         - no extra thread is created for the Poll (Consume) loop.
    //    /// </summary>
    //    public void Run_Consume(string brokerList, List<string> topics, CancellationToken cancellationToken)
    //    {
    //        const int commitPeriod = 5;

    //        using (var consumer = new Consumer<string, string>(MQConfig.Build()))
    //        {
    //            consumer.Subscribe(topics);

    //            while (!cancellationToken.IsCancellationRequested)
    //            {
    //                try
    //                {
    //                    var consumeResult = consumer.Consume(cancellationToken);
    //                    if (consumeResult.Offset % commitPeriod == 0)
    //                    {
    //                        // The Commit method sends a "commit offsets" request to the Kafka
    //                        // cluster and synchronously waits for the response. This is very
    //                        // slow compared to the rate at which the consumer is capable of
    //                        // consuming messages. A high performance application will typically
    //                        // commit offsets relatively infrequently and be designed handle
    //                        // duplicate messages in the event of failure.
    //                        var committedOffsets = consumer.Commit(consumeResult);
    //                        Console.WriteLine(string.Format("Committed offset: {0}", committedOffsets));
    //                    }
    //                }
    //                catch (ConsumeException e)
    //                {
    //                    Console.WriteLine(string.Format("Consume error: {0}", e.Error));
    //                }
    //            }

    //            consumer.Close();
    //        }
    //    }
    //    /// <summary>
    //    ///     In this example
    //    ///         - consumer group functionality (i.e. .Subscribe + offset commits) is not used.
    //    ///         - the consumer is manually assigned to a partition and always starts consumption
    //    ///           from a specific offset (0).
    //    /// </summary>
    //    public static void Run_ManualAssign(string brokerList, List<string> topics, CancellationToken cancellationToken)
    //    {
    //        var config = new ConsumerConfig
    //        {
    //            // the group.id property must be specified when creating a consumer, even 
    //            // if you do not intend to use any consumer group functionality.
    //            GroupId = new Guid().ToString(),
    //            BootstrapServers = brokerList,
    //            // partition offsets can be committed to a group even by consumers not
    //            // subscribed to the group. in this example, auto commit is disabled
    //            // to prevent this from occuring.
    //            EnableAutoCommit = true
    //        };

    //        using (IConsumer consumer = new Consumer<string, string>(config))
    //        {
    //            consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 0, Offset.Beginning)).ToList());

    //            consumer.OnError += (_, e)
    //                => Console.WriteLine(string.Format("Error: {0}", e.Reason));

    //            consumer.OnPartitionEOF += (_, topicPartitionOffset)
    //                => Console.WriteLine(string.Format("End of partition: {0}", topicPartitionOffset));

    //            while (!cancellationToken.IsCancellationRequested)
    //            {
    //                try
    //                {
    //                    var consumeResult = consumer.Consume(cancellationToken);
    //                    Console.WriteLine(string.Format("Received message at {0}:${1}", consumeResult.TopicPartitionOffset, consumeResult.Message));
    //                }
    //                catch (ConsumeException e)
    //                {
    //                    Console.WriteLine(string.Format("Consume error: {0}", e.Error));
    //                }
    //            }

    //            consumer.Close();
    //        }
    //    }
    //    private static void PrintUsage()
    //    {
    //        Console.WriteLine("Usage: .. <poll|consume|manual> <broker,broker,..> <topic> [topic..]");
    //    }
    //}
    public class KafkaConsumer : AbstractKafkaMessage<KafkaSubscribeConfig>, ISubscribeChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new KafkaSubscribeConfig MQConfig { get; set; }
        //SubscribeConfig IBaseMessaegHandler<SubscribeConfig>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as KafkaSubscribeConfig; }
        //}

        /// <summary>
        /// 构造方法
        /// </summary>
        public KafkaConsumer()
        {
            this.MQConfig = new KafkaSubscribeConfig();
            //配置参数校验
            MQConfig.Check();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="brokerUri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public KafkaConsumer(KafkaSubscribeConfig config)
        {
            if (config != null)
                this.MQConfig = config;
            //配置参数校验
            MQConfig.Check();
        }

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <param name="subscribe"></param>
        public void Subscribe(string queueName, Action<IMessageContent> action)
        {
            if (string.IsNullOrWhiteSpace(MQConfig.GroupId))
            {
                throw new MQException("消费者群组GroupId参数为空");
            }
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new MQException("QueueName为空");
            }
            if (action == null)
            {
                throw new MQException("消费者action参数为空");
            }
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                using (var consumer = new ConsumerBuilder<string, string>(MQConfig.Build())
                     .SetErrorHandler((_, e) =>
                     {
                         WriteLog(e.Reason);
                         Console.ForegroundColor = ConsoleColor.Yellow;
                         Console.WriteLine("连接出错：" + e.Reason);
                     })
                    .Build())
                {
                    //订阅topicName
                    consumer.Subscribe(queueName);

                    CancellationTokenSource cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        //prevent the process from terminating.
                        e.Cancel = true;
                        cts.Cancel();
                    };

                    //是否消费成功
                    bool isOK = false;
                    //result
                    ConsumeResult<string, string> consumeResult = null;
                    try
                    {
                        while (true)
                        {
                            isOK = false;
                            try
                            {
                                //如果需要从指定位置消费，可以参考下面这段代码；下面这段代码别乱起用（慎重）
                                //consumer.Assign(new TopicPartitionOffset(queueName, 0, Offset.Beginning));
                                consumeResult = consumer.Consume(cts.Token);
                                if (consumeResult.IsPartitionEOF)
                                {
                                    //WriteLog($"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
                                    continue;
                                }
                                //接收到的消息记录Log
                                //WriteLog($"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Message.Value}");
                                var bodyMsg = new KafkaMessageContent(consumeResult.Message.Value);
                                bodyMsg.Key = consumeResult.Message.Key != null ? consumeResult.Message.Key.ToString() : bodyMsg.Key;
                                //消息消费
                                action?.Invoke(bodyMsg);
                                //消费成功
                                isOK = true;
                                //提交方法向Kafka集群发送一个“提交偏移量”请求，并同步等待响应。
                                //与消费者能够消费消息的速度相比，这是非常慢的。
                                //一个高性能的应用程序通常会相对不频繁地提交偏移量，并且在失败的情况下被设计来处理重复的消息
                                consumer.Commit(consumeResult);
                                //消费成功Log记录
                                //WriteLog($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                            }
                            catch (ConsumeException e)
                            {
                                isOK = false;
                                WriteError($"Error occured: {e.Error.Reason}");
                            }
                            catch (Exception ex)
                            {
                                isOK = false;
                                WriteError($"Error occured: {ex.StackTrace}");
                            }

                            //消费失败后置处理
                            if (!isOK && consumeResult != null)
                            {
                                //消费失败代码逻辑处理
                                ErrorHandler(consumer, consumeResult);
                            }
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        WriteException(e);
                        // Ensure the consumer leaves the group cleanly and final offsets are committed.
                        consumer.Close();
                    }
                }
            });
        }

        /// <summary>
        /// 消费异常处理
        /// </summary>
        /// <param name="consumer">消费者</param>
        /// <param name="consumeResult">消息</param>
        private void ErrorHandler(IConsumer<string, string> consumer, ConsumeResult<string, string> consumeResult)
        {
            if (consumeResult != null && consumer != null)
            {
                string queueName = consumeResult.Topic;
                //WriteLog($"Consumed '{queueName}' message fail '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                //消费失败，并且需要不需要转发到DLQ队列中，所以我们这里需要把(Offset-1)
                if (!this.MQConfig.TransformToDLQ || queueName.StartsWith("DLQ.", StringComparison.OrdinalIgnoreCase))
                {
                    //偏移量往回拉一位，尝试6次操作。如果执行失败，确保消息不遗漏直接停止消费。
                    OffsetBack(consumer, consumeResult);
                    return;
                }

                //需要转发到DLQ队列中
                string transformTopics = "DLQ." + queueName;
                //WriteLog($"消息开始转发到{transformTopics}队列");
                KafkaProducerConfig config = new KafkaProducerConfig(MQConfig.BrokerUri,
                    MQConfig.UserName, MQConfig.Password, transformTopics);
                try
                {
                    //将消息转发到死信队列
                    using (IProducerChannel producer = new KafkaProducer(config))
                    {
                        producer.Producer(consumeResult.Message.Value?.ToString());
                    }
                    //提交偏移量
                    consumer.Commit(consumeResult);
                    //WriteLog($"消息转发到{transformTopics}队列成功");
                }
                catch (Exception ex)
                {
                    WriteError($"消息转发到{transformTopics}队列失败。Error occured: {ex.StackTrace}");
                    //偏移量往回拉一位，尝试6次操作。如果执行失败，确保消息不遗漏直接停止消费。
                    OffsetBack(consumer, consumeResult);
                }
            }
        }

        /// <summary>
        /// 把Offset偏移量往回拉一位
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="consumeResult"></param>
        /// <param name="tryTimes">默认执行6次</param>
        private void OffsetBack(IConsumer<string, string> consumer, ConsumeResult<string, string> consumeResult, int tryTimes = 6)
        {
            int count = tryTimes;
            string queueName = consumeResult.Topic;
            while (count > 0)
            {
                WriteLog($"消息消费失败，执行偏移量Offset-1操作");
                try
                {
                    //消费失败，重置一下最新偏移量
                    consumer.Assign(new TopicPartitionOffset(queueName, consumeResult.Partition, consumeResult.Offset));
                    WriteLog($"偏移量重置成功{consumeResult.Offset}");
                    count--;
                    return;
                }
                catch (Exception ex)
                {
                    WriteLog($"消息消费失败，执行偏移量Offset-1操作失败。Error occured: {ex.StackTrace}");
                    //尝试重置偏移量次数到了最大次数，直接抛出异常。停止消费
                    if (count == 0)
                    {
                        WriteError($"消息消费失败，执行偏移量Offset-1操作失败次数已达到${tryTimes}，消费者停止消费");
                        //抛出这个异常，会引发Subscribe()到catch代码块。catch会停止消费
                        throw new OperationCanceledException($"消息消费失败，执行偏移量Offset-1操作失败次数已达到${tryTimes}，消费者停止消费");
                    }
                    //停止3s在重新重置偏移量
                    Thread.Sleep(3000);
                }
            }
        }

    }
}
