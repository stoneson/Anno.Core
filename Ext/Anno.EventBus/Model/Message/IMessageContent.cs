using System;

namespace Anno.EventBus.Model.Message
{
    public interface IMessageContent
    {

        /// <summary>
        /// 消息Key
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// 消息Value
        /// </summary>
        string Value { get; set; }
        /// <summary>
        /// 附加标记
        /// </summary>
        string Tag { get; set; }
    }
}
