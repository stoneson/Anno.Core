using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Enums;
using Newtonsoft.Json;
using System;

namespace Anno.EventBus.Executor.Rabbit
{
    public class RabbitMQChannelAdapter : IChannelAdapter
    {
        /// <summary>
        /// 构建生产者
        /// </summary>
        /// <returns></returns>
        public IProducerChannel GetProducer()
        {
            return new RabbitMQProducer();
        }

        /// <summary>
        /// 构建消费者handler
        /// </summary>
        /// <returns></returns>
        public ISubscribeChannel GetSubscribe()
        {
            return new RabbitMQConsumer();
        }

        /// <summary>
        /// 生成者handler
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IProducerChannel GetProducer(string input)
        {
            var config = JsonConvert.DeserializeObject<RabbitProducerConfig>(input);
            return new RabbitMQProducer(config);
        }

        /// <summary>
        /// 消费者handler
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ISubscribeChannel GetSubscribe(string input)
        {
            var config = JsonConvert.DeserializeObject<RabbitSubscribeConfig>(input);
            return new RabbitMQConsumer(config);
        }

        /// <summary>
        /// 支持RabbitQM操作
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Supports(MQTypeEnum mqType)
        {
            return mqType == MQTypeEnum.RabbitQM;
        }
    }
}
