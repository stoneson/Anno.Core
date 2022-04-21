using Anno.EventBus.Executor.Active;
using Anno.EventBus.Executor.Kafka;
using Anno.EventBus.Executor.Rabbit;
using Anno.EventBus.Interface;
using System;
using System.Collections.Generic;

namespace Anno.EventBus.Executor
{
    public class ChannelAdapterFactory
    {
        /// <summary>
        /// Channels
        /// </summary>
        private readonly static List<IChannelAdapter> Channels = new List<IChannelAdapter>();

        static ChannelAdapterFactory()
        {
            Channels.Add(new ActiveMQChannelAdapter());
            Channels.Add(new KafkaChannelAdapter());
            Channels.Add(new RabbitMQChannelAdapter());
        }

        /// <summary>
        /// 获取生成者实例
        /// </summary>
        /// <returns></returns>
        public static IProducerChannel GetProducerChannel()
        {
            var mqConfig = new Model.ServesConfig();
            foreach (var channel in Channels)
            {
                if (channel.Supports(mqConfig.MQType))
                {
                    return channel.GetProducer();
                }
            }
            return null;
        }
        /// <summary>
        /// 获取消费者实例
        /// </summary>
        /// <returns></returns>
        public static ISubscribeChannel GetSubscribeChannel()
        {
            var mqConfig = new Model.ServesConfig();
            foreach (var channel in Channels)
            {
                if (channel.Supports(mqConfig.MQType))
                {
                    return channel.GetSubscribe();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取生成者实例
        /// </summary>
        /// <param name="type">MQ类型[1=ActiveMQ;2=Kafka;3=RabbitQM]可以看MQTypeEnum</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IProducerChannel GetProducerChannel(string type, string config = "")
        {
            if (Enum.TryParse(type, out Model.Enums.MQTypeEnum mqType))
                return GetProducerChannel(mqType, config);
            return null;
        }
        /// <summary>
        /// 获取生成者实例
        /// </summary>
        /// <param name="type">MQ类型[1=ActiveMQ;2=Kafka;3=RabbitQM]可以看MQTypeEnum</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IProducerChannel GetProducerChannel(Model.Enums.MQTypeEnum type, string config = "")
        {
            foreach (var channel in Channels)
            {
                if (channel.Supports(type))
                {
                    if (string.IsNullOrWhiteSpace(config))
                        return channel.GetProducer();
                    else
                        return channel.GetProducer(config);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取消费者实例
        /// </summary>
        /// <param name="type">MQ类型[1=ActiveMQ;2=Kafka;3=RabbitQM]可以看MQTypeEnum</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ISubscribeChannel GetSubscribeChannel(string type, string config = "")
        {
            if (Enum.TryParse(type, out Model.Enums.MQTypeEnum mqType))
                return GetSubscribeChannel(mqType, config);
            return null;
        }

        /// <summary>
        /// 获取消费者实例
        /// </summary>
        /// <param name="type">MQ类型[1=ActiveMQ;2=Kafka;3=RabbitQM]可以看MQTypeEnum</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ISubscribeChannel GetSubscribeChannel(Model.Enums.MQTypeEnum type, string config = "")
        {
            foreach (var channel in Channels)
            {
                if (channel.Supports(type))
                {
                    if (string.IsNullOrWhiteSpace(config))
                        return channel.GetSubscribe();
                    else
                        return channel.GetSubscribe(config);
                }
            }
            return null;
        }

    }
}
