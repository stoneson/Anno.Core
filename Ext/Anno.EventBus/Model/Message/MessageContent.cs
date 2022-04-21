namespace Anno.EventBus.Model.Message
{
    public class MessageContent : IMessageContent
    {

        /// <summary>
        /// 消息KEY
        /// </summary>
        public virtual string Key { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// 附加标记
        /// </summary>
        public virtual string Tag { get; set; }
    }
}
