namespace Anno.EventBus.Model.Enums
{
    /// <summary>
    /// 消息队列类型
    /// </summary>
    public enum MQTypeEnum
    {
        None = 0,
        /// <summary>
        /// activemq
        /// </summary>
        ActiveMQ = 1,

        /// <summary>
        /// kafka
        /// </summary>
        Kafka = 2,

        /// <summary>
        /// RabbitQM
        /// </summary>
        RabbitQM = 3

    }
}
