using Confluent.Kafka;
using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Executor.Kafka
{
    public class KafkaProducer : AbstractKafkaMessage<KafkaProducerConfig>, IProducerChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new KafkaProducerConfig MQConfig { get; set; }
        //ProducerConfigs IBaseMessaegHandler<ProducerConfigs>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as KafkaProducerConfig; }
        //}

        /// <summary>
        /// 生成者对象
        /// </summary>
        private IProducer<string, string> MessageProducer;

        /// <summary>
        /// 构造方法
        /// </summary>
        public KafkaProducer()
        {
            this.MQConfig = new KafkaProducerConfig();
            //配置参数校验
            MQConfig.Check();
            //message<key,value> 这个key目前没用，做消息指定分区投放有用的；我们直接用null
            //MessageProducer = new ProducerBuilder<string, string>(MQConfig.Build()).Build();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="config"></param>
        public KafkaProducer(KafkaProducerConfig config) : this()
        {
            if (config != null)
                this.MQConfig = config;
            ////配置参数校验
            //MQConfig.Check();
            ////message<key,value> 这个key目前没用，做消息指定分区投放有用的；我们直接用null
            //MessageProducer = new ProducerBuilder<Null, string>(config.Build()).Build();
        }

        /// <summary>
        /// 消息发送
        /// auto.create.topics.enable=false，在发送之前先要把Topic创建好，这点跟ActiveMQ、RabbitMQ不一样。
        /// 重要的事情说三遍，Topics要先创建好，Topics要先创建好，Topics要先创建好。
        /// </summary>
        /// <param name="messageContent"></param>
        public void Producer(IMessageContent messageContent)
        {
            if (!IsMessageContent(messageContent))
            {
                throw new MQException("messageContent 参数类型不是KafkaMessageContent");
            }
            var message = (KafkaMessageContent)messageContent;
            if (message == null || string.IsNullOrWhiteSpace(message.Value))
            {
                throw new MQException("发送消息内容为空");
            }
            if (string.IsNullOrWhiteSpace(message.Key))
            {
                message.Key = Guid.NewGuid().ToString();
            }
            if (MQConfig.CompleteHandler == null)
            {
                MQConfig.CompleteHandler = DefaultProducerHandler;
            }
            if (MessageProducer == null)
                MessageProducer = new ProducerBuilder<string, string>(MQConfig.Build()).Build();
            //消息发送
            MessageProducer.Produce(MQConfig.QueueName, new Message<string, string>()
            {
                Key = message.Key,
                Value = message.Value
            }, MQConfig.CompleteHandler);
            MessageProducer.Flush(TimeSpan.FromSeconds(10));
        }
        /// <summary>
        /// 生产消息并发送消息
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="message">需要传送的消息</param>
        public void Produce(string key, string message)
        {
            bool result = false;
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("消息内容不能为空！");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Guid.NewGuid().ToString();
            }
            var deliveryReport = MessageProducer.ProduceAsync(MQConfig.QueueName, new Message<string, string> { Key = key, Value = message });
            deliveryReport.ContinueWith(task =>
            {
                //MQConfig.CompleteHandler?.Invoke(new DeliveryReport<string, string>() {Error= task.Result. });
                Console.WriteLine("Producer：" + MessageProducer.Name + "\r\nTopic：" + MQConfig.QueueName + "\r\nPartition：" + task.Result.Partition + "\r\nOffset：" + task.Result.Offset + "\r\nMessage：" + task.Result.Value + "\r\nResult：" + result);
            });
            MessageProducer.Flush(TimeSpan.FromSeconds(10));
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            MessageProducer?.Dispose();
            MessageProducer = null;
        }

        /// <summary>
        /// auto.create.topics.enable=false，在发送之前先要把Topic创建好，这点跟ActiveMQ、RabbitMQ不一样。
        /// 重要的事情说三遍，Topics要先创建好，Topics要先创建好，Topics要先创建好。
        /// </summary>
        /// <param name="message"></param>
        public void Producer(string message)
        {
            this.Producer(new KafkaMessageContent(message));
        }

    }
}
