using System;

namespace Anno.EventBus.Model.Message
{
    public class KafkaMessageContent : MessageContent
    {

        public KafkaMessageContent(string msg, string key = "")
        {
            this.Value = msg;
            this.Key = key;
        }

    }
}
