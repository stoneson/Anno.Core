using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Interface
{
    /// <summary>
    /// 消息订阅者
    /// </summary>
    public interface ISubscribeChannel //: Executor.IBaseMessaegHandler<Model.Config.Subscribe.SubscribeConfig>
    {

        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="action">匿名方法</param>
        void Subscribe(string queueName, Action<IMessageContent> action);

    }

}
