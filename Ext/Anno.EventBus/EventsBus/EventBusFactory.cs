using Anno.EventBus.Executor.Active;
using Anno.EventBus.Executor.Kafka;
using Anno.EventBus.Executor.Rabbit;
using Anno.EventBus.Interface;
using System;
using System.Collections.Generic;

namespace Anno.EventBus
{
    /// <summary>
    /// 消息总线工厂类
    /// </summary>
    public class EventBusFactory
    {
        private readonly static Model.Enums.MQTypeEnum MQType = Model.Enums.MQTypeEnum.None;
        static EventBusFactory()
        {
            try
            {
                if (Enum.TryParse(Config.Constants.GetAppSettings("MQType", Model.Enums.MQTypeEnum.RabbitQM.ToString()).Trim(), out Model.Enums.MQTypeEnum mqType))
                    MQType = mqType;
            }
            catch (Exception ex) { Log.Log.Error(ex.ToString()); }

        }

        /// <summary>
        /// 消息总线实例
        /// </summary>
        /// <returns></returns>
        public static IEventBus GetEventBus()
        {
            if (MQType == Model.Enums.MQTypeEnum.RabbitQM)
            {
                return new EventBusRabbitMQ();
            }
            else if (MQType == Model.Enums.MQTypeEnum.Kafka)
            {
                return new EventBusKafka();
            }
            else if (MQType == Model.Enums.MQTypeEnum.ActiveMQ)
            {
                return new EventBusActiveMQ();
            }
            return new EventBusMemory();
        }
        /// <summary>
        /// 消息总线实例
        /// </summary>
        /// <returns></returns>
        public static IEventBus GetEventBus(string config)
        {
            if (MQType == Model.Enums.MQTypeEnum.RabbitQM)
            {
                return new EventBusRabbitMQ(config);
            }
            else if (MQType == Model.Enums.MQTypeEnum.Kafka)
            {
                return new EventBusKafka(config);
            }
            else if (MQType == Model.Enums.MQTypeEnum.ActiveMQ)
            {
                return new EventBusActiveMQ(config);
            }
            return new EventBusMemory();
        }

    }
}
