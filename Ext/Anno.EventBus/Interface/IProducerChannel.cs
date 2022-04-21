using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Interface
{
    /// <summary>
    /// 消息生成者
    /// </summary>
    public interface IProducerChannel: IDisposable//, Executor.IBaseMessaegHandler<Model.Config.Producer.ProducerConfigs>
    {
        /// <summary>
        /// 消息生产
        /// </summary>
        /// <param name="content">生产内容</param>
        void Producer(IMessageContent content);

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="message"></param>
        void Producer(string message);

    }
}
