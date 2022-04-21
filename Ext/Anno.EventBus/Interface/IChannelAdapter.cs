namespace Anno.EventBus.Interface
{
    public interface IChannelAdapter
    {

        /// <summary>
        /// 是否支持
        /// </summary>
        /// <param name="type">MQ类型</param>
        /// <returns></returns>
        bool Supports(Model.Enums.MQTypeEnum type);

        /// <summary>
        /// 构建生产者
        /// </summary>
        /// <returns></returns>
        IProducerChannel GetProducer();

        /// <summary>
        /// 构建消费者handler
        /// </summary>
        /// <returns></returns>
        ISubscribeChannel GetSubscribe();

        /// <summary>
        /// 构建生产者
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IProducerChannel GetProducer(string input);

        /// <summary>
        /// 构建消费者handler
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        ISubscribeChannel GetSubscribe(string input);

    }
}
