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
    /// Kafka消息总线
    /// </summary>
    public class EventBusKafka : EventBusMemory
    {
        private const string BrokerName = "EventBus";
        private string _queueName = "BusQueue";

        /// <summary>
        /// 消息队列生产者通道
        /// </summary>
        protected Executor.Kafka.KafkaProducer producerChannel { get; set; }
        /// <summary>
        /// 消息队列消费者通道
        /// </summary>
        protected Executor.Kafka.KafkaConsumer subscribeChannel { get; set; }


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
                            _instance = new EventBusKafka();
                        }
                    }
                }

                return _instance;
            }
        }
        public override Model.Enums.MQTypeEnum MQType => Model.Enums.MQTypeEnum.Kafka;

        public EventBusKafka()
        {
            producerChannel = new  Executor.Kafka.KafkaProducer();
            subscribeChannel = new Executor.Kafka.KafkaConsumer();
        }
        public EventBusKafka(string config)
        {
            var ca = new Executor.Rabbit.RabbitMQChannelAdapter();
            producerChannel = ca.GetProducer(config) as Executor.Kafka.KafkaProducer;
            subscribeChannel = ca.GetSubscribe(config) as Executor.Kafka.KafkaConsumer;
        }

        public override void Publish<TEventData>(TEventData eneity)
        {
            if (eneity == null) return;

            var emsg = new Model.Message.KafkaMessageContent(JsonConvert.SerializeObject(eneity));
            emsg.Key = eneity.GetType().Name;
            emsg.Tag = eneity.GetType().Name;
            emsg.Value = JsonConvert.SerializeObject(emsg);

            producerChannel.MQConfig.QueueName = _queueName;
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
                subscribeChannel.MQConfig.GroupId = BrokerName;
                subscribeChannel.MQConfig.QueueName = _queueName;
                subscribeChannel.ErrorNotice = (exchange, routingKey, exception, message) =>
                            ErrorNotice?.Invoke(exchange, routingKey, exception, message);

                subscribeChannel.Subscribe(_queueName, (msg) =>
                 {
                     var rmsg = JsonConvert.DeserializeObject<Model.Message.MessageContent>(msg.Value);
                     HandleEvent(rmsg.Tag, rmsg.Value);
                 });
            }
        }
    }
}
