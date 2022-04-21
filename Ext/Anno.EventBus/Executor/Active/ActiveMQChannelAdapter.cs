using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Enums;
using Newtonsoft.Json;
using System;

namespace Anno.EventBus.Executor.Active
{
    public class ActiveMQChannelAdapter : IChannelAdapter
    {
        /// <summary>
        /// 构建生产者
        /// </summary>
        /// <returns></returns>
        public IProducerChannel GetProducer()
        {
            return new ActiveMQProducer();
        }

        /// <summary>
        /// 构建消费者handler
        /// </summary>
        /// <returns></returns>
        public ISubscribeChannel GetSubscribe()
        {
            return new ActiveMQConsumer();
        }

        /// <summary>
        /// 生成者handler
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IProducerChannel GetProducer(string input)
        {
            var config = JsonConvert.DeserializeObject<ActiveProducerConfig>(input);
            return new ActiveMQProducer(config);
        }

        /// <summary>
        /// 构建消费者handler
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ISubscribeChannel GetSubscribe(string input)
        {
            var config = JsonConvert.DeserializeObject<ActiveSubscribeConfig>(input);
            return new ActiveMQConsumer(config);
        }

        /// <summary>
        /// 支持ActiveMQ操作
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Supports(MQTypeEnum mqType)
        {
            return mqType == MQTypeEnum.ActiveMQ;
        }
    }
}
