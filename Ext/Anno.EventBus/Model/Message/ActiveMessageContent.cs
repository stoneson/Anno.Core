using System;

namespace Anno.EventBus.Model.Message
{
    public class ActiveMessageContent : MessageContent
    {
        public ActiveMessageContent(string msg)
        {
            this.Value = msg;
        }
    }
}
