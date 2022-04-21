using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using Anno.EventBus.Interface;
using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Enums;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;
using System.Collections.Concurrent;

namespace Anno.EventBus.Executor.Active
{
    public class ActiveMQProducer : AbstractActiveMessage<ActiveProducerConfig>, IProducerChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new ActiveProducerConfig MQConfig { get; set; }
        //ProducerConfigs IBaseMessaegHandler<ProducerConfigs>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as ActiveProducerConfig; }
        //}

        /// <summary>
        /// 队列缓存字典 生产者
        /// </summary>
        private readonly ConcurrentDictionary<string, IMessageProducer> producerMaps = new ConcurrentDictionary<string, IMessageProducer>();

        /// <summary>
        /// 消息生成者
        /// </summary>
        private IMessageProducer MessageProducer;

        /// <summary>
        /// 构造方法
        /// </summary>
        public ActiveMQProducer()
        {
            if (MQConfig == null)
                this.MQConfig = new ActiveProducerConfig();
            //配置参数校验
            MQConfig.Check();
            if (!IsOpen)
            {
                //默认尝试连接3次
                TryRequireOpen(3);
            }
            ////创建producer
            //MessageProducer = CreateProducer(MQConfig);
        }
        public ActiveMQProducer(ActiveProducerConfig config) : this()
        {
            if (config != null)
                this.MQConfig = config;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            try
            {
                foreach (var producer in this.producerMaps.Values)
                {
                    producer?.Close();
                }
                this.producerMaps.Clear();
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }
            finally
            {
                Session?.Close();
                Connection?.Close();
                MessageProducer?.Close();
                Session = null;
                Connection = null;
                MessageProducer = null;
                IsOpen = false;
            }
        }

        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// 创建session
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public override ISession CreateSession(IConnection connection)
        {
            return connection.CreateSession();
        }

        /// <summary>
        /// 创建发送消息对象
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public IMessage CreateMessage(IMessageProducer producer, object body)
        {
            IMessage message;
            if (body is byte[])
            {
                message = producer.CreateBytesMessage(body as byte[]);
            }
            else if (body is string)
            {
                message = producer.CreateTextMessage(body as string);
            }
            else
            {
                message = producer.CreateObjectMessage(body);
            }
            return message;
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="messageContent"></param>
        public void Producer(IMessageContent messageContent)
        {
            if (!IsMessageContent(messageContent))
            {
                throw new MQException("messageContent 参数类型不是ActiveMessageContent");
            }
            var message = (ActiveMessageContent)messageContent;
            if (message == null || string.IsNullOrWhiteSpace(message.Value))
            {
                throw new MQException("发送消息内容为空");
            }
            //创建producer
            if (MessageProducer == null)
            {
                MessageProducer = CreateProducer(MQConfig);
            }
            //创建发送的消息
            var messageBody = CreateMessage(MessageProducer, message.Value);

            if (messageBody != null)
            {
                if (!string.IsNullOrWhiteSpace(this.MQConfig.MQFilterName))
                {
                    messageBody.Properties.SetString("filter", this.MQConfig.MQFilterName);
                }
                //默认为持久化的
                MessageProducer.Send(messageBody, MsgDeliveryMode.Persistent, MsgPriority.Normal, TimeSpan.MinValue);
            }
        }

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private IMessageProducer CreateProducer(ActiveProducerConfig config)
        {
            //创建新生产者
            IMessageProducer func(string queueName)
            {
                IMessageProducer producer = null;
                switch (config.ActiveMQType)
                {
                    case ActiveMQTypeEnum.Queue:
                        {
                            producer = Session.CreateProducer(new ActiveMQQueue(queueName));
                            break;
                        }
                    case ActiveMQTypeEnum.Topic:
                        {
                            producer = Session.CreateProducer(new ActiveMQTopic(queueName));
                            break;
                        }
                    default:
                        {
                            throw new MQException(string.Format("无法识别的MQMode类型:{0}", config.ActiveMQType));
                        }
                }

                return producer;
            }

            return this.producerMaps.GetOrAdd(config.QueueName, func);
        }

        /// <summary>
        /// 消息发送，queue默认是Queue队列，不是Topic。这个要注意
        /// </summary>
        /// <param name="message">队列消息</param>
        public void Producer(string message)
        {
            this.Producer(new ActiveMessageContent(message));
        }

    }
}
