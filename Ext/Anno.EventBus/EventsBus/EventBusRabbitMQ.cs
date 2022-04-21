using Anno.EventBus.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Anno.EventBus
{
    /// <summary>
    /// RabbitMQ消息总线
    /// </summary>
    public class EventBusRabbitMQ : EventBusMemory
    {
        private const string BrokerName = "EventBus";
        private string _queueName = "BusQueue";

        /// <summary>
        /// 消息队列生产者通道
        /// </summary>
        protected Executor.Rabbit.RabbitMQProducer producerChannel { get; set; }
        /// <summary>
        /// 消息队列消费者通道
        /// </summary>
        protected Executor.Rabbit.RabbitMQConsumer subscribeChannel { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public new static IEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new EventBusRabbitMQ();
                        }
                    }
                }

                return _instance;
            }
        }

        public EventBusRabbitMQ()
        {
            producerChannel = new Executor.Rabbit.RabbitMQProducer();
            subscribeChannel = new Executor.Rabbit.RabbitMQConsumer();
        }
        public EventBusRabbitMQ(string config)
        {
            var ca = new Executor.Rabbit.RabbitMQChannelAdapter();
            producerChannel = ca.GetProducer(config) as Executor.Rabbit.RabbitMQProducer;
            subscribeChannel = ca.GetSubscribe(config) as Executor.Rabbit.RabbitMQConsumer;
        }

        public override void Publish<TEventData>(TEventData eneity)
        {
            if (eneity == null) return;
            var emsg = new Model.Message.RabbitMessageContent(JsonConvert.SerializeObject(eneity));
            emsg.ExchangeName = BrokerName;
            emsg.RoutingKey = eneity.GetType().Name;
            emsg.Tag = eneity.GetType().Name;

            producerChannel.Producer(emsg);
        }

        //--------------------------------------------------------------------------------------------------------------
        public override void Subscribe(Type type, object data)
        {
            lock (_lock)
            {
                if (_dicHandlers.ContainsKey(type))
                {
                    List<object> list = _dicHandlers[type];
                    if (!list.Any((object o) => _Equals(o, data)))
                    {
                        list.Add(data);
                    }
                }
                else
                {
                    _dicHandlers[type] = new List<object>
                    {
                        data
                    };
                }
                //---------------------------------------------------------------------------------------------------------
                subscribeChannel.MQConfig.ExchangeName = BrokerName;
                subscribeChannel.MQConfig.ExchangeType = Model.Enums.RabbitExchangeType.direct;
                subscribeChannel.MQConfig.QueueName = _queueName;
                subscribeChannel.MQConfig.RoutingKey = type.Name;
                subscribeChannel.MQConfig.Durable = false;
                subscribeChannel.ErrorNotice = (exchange, routingKey, exception, message) =>
                            ErrorNotice?.Invoke(exchange, routingKey, exception, message);

                subscribeChannel.Subscribe(_queueName, (msg) =>
                 {
                     HandleEvent(msg.Tag, msg.Value);
                 });
            }
        }

    }
}
