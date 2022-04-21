using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Anno.EventBus
{
    public delegate void ConsumeErrorNotice(string exchange, string routingKey, Exception exception, string message);

    public interface IEventBus
    {
        ConsumeErrorNotice ErrorNotice { get; set; }

        void SubscribeAll(params Assembly[] assemblys);

        void Subscribe(Type type, object data);

        void Publish<TEventData>(TEventData eneity) where TEventData : IEventData;

        void PublishAsync<T>(T eneity) where T : IEventData;
    }

    public interface IEventData : Model.Message.IMessageContent
    {
        DateTime EventTime
        {
            get;
            set;
        }

        object EventSource
        {
            get;
            set;
        }
    }

    public interface IEventHandler<TEventData> where TEventData : IEventData
    {
        void Handler(TEventData entity);
    }

}
