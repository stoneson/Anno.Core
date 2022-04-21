using System;

namespace Anno.EventBus
{
    public class EventData : Model.Message.MessageContent, IEventData
    {
        public DateTime EventTime
        {
            get;
            set;
        }

        public object EventSource
        {
            get;
            set;
        }

        public EventData()
        {
            EventTime = DateTime.Now;
        }
    }

}
