using Confluent.Kafka;
using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Executor.Kafka
{
    public abstract class AbstractKafkaMessage<TConfig> : AbstractBaseMessaegHandler<TConfig> where TConfig : ServesConfig
    {
        public AbstractKafkaMessage()
        {
            DefaultProducerHandler =(result) =>WriteLog(!result.Error.IsError ? $"Delivered message to {result.TopicPartitionOffset}"
                                                            : $"Delivery Error: {result.Error.Reason}");
        }
        /// <summary>
        /// 发送事件绑定
        /// </summary>
        public Action<DeliveryReport<string, string>> DefaultProducerHandler { get; private set; }

        /// <summary>
        /// 生产者默认参数
        /// </summary>
        /// <param name="bootstrapServers"></param>
        /// <returns></returns>
        protected ProducerConfig GetDefaultProducerConfig(string bootstrapServers)
        {
            ProducerConfig config = new ProducerConfig()
            {
                BootstrapServers = bootstrapServers
            };
            return config;
        }

        /// <summary>
        /// 判断配置信息类型是否正确
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool IsSubscribeConfig(ISubscribeConfig config)
        {
            if (config == null)
            {
                throw new MQException("config参数为空");
            }
            return (config is KafkaSubscribeConfig);
        }

        /// <summary>
        /// 判断配置信息类型是否正确
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public override bool IsMessageContent(IMessageContent content)
        {
            if (content == null)
            {
                throw new MQException("config参数为空");
            }
            return content is KafkaMessageContent;
        }

    }

}
