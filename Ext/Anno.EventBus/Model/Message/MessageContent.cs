namespace Anno.EventBus.Model.Message
{
    public abstract class MessageContent : IMessageContent
    {

        /// <summary>
        /// 消息KEY
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 附加标记
        /// </summary>
        public string Tag { get; set; }
    }
}
