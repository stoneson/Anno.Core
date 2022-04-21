using System;

namespace Anno.EventBus.Model.Enums
{
    /// <summary>
    /// 队列模式
    /// </summary>
    public enum RabbitExchangeType
    {
        /// <summary>
        /// 简单队列
        /// </summary>
        simple = -1,
        /// <summary>
        /// Exchange type used for AMQP direct exchanges.
        /// </summary>
        direct = 0,
        /// <summary>
        /// Exchange type used for AMQP fanout exchanges.
        /// </summary>
        fanout = 1,
        /// <summary>
        ///Exchange type used for AMQP headers exchanges.
        /// </summary>
        //headers = 2,
        /// <summary>
        /// Exchange type used for AMQP topic exchanges.
        /// </summary>
        topic = 3

    }
}
