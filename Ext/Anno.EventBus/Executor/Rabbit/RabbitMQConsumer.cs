
using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Enums;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Anno.EventBus.Executor.Rabbit
{
    public class RabbitMQConsumer : AbstractRabbitMessage<RabbitSubscribeConfig>, ISubscribeChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new RabbitSubscribeConfig MQConfig { get; set; }
        //SubscribeConfig IBaseMessaegHandler<SubscribeConfig>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as RabbitSubscribeConfig; }
        //}

        /// <summary>
        /// 构造方法
        /// </summary>
        public RabbitMQConsumer() 
        {
            this.MQConfig = new RabbitSubscribeConfig();
            //配置参数校验
            MQConfig.Check();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="config"></param>
        public RabbitMQConsumer(RabbitSubscribeConfig config)
        {
            if (config != null)
                this.MQConfig = config;
            //配置参数校验
            MQConfig.Check();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            Connection?.Close();
            Connection = null;
        }
        protected override ConnectionFactory getConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = MQConfig.BrokerUri,
                Port = MQConfig.Port,
                //VirtualHost = "/",
                UserName = MQConfig.UserName,
                Password = MQConfig.Password,
                //自动重新连接
                AutomaticRecoveryEnabled = true,
                //心跳超时时间
#if NET451
                 RequestedHeartbeat = (ushort)(TimeSpan.FromSeconds(MQConfig.RequestedHeartbeat).TotalSeconds)
#else
                RequestedHeartbeat = TimeSpan.FromSeconds(MQConfig.RequestedHeartbeat)
#endif

            };
            if (!string.IsNullOrWhiteSpace(MQConfig.VirtualHost))
                connectionFactory.VirtualHost = MQConfig.VirtualHost;

            return connectionFactory;
        }
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <param name="queueName"></param>
        public void Subscribe(string queueName, Action<IMessageContent> action)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new MQException("QueueName为空");
            }
            if (action == null)
            {
                throw new MQException("消费者action参数为空");
            }
            if (!IsOpen)
            {
                //默认尝试连接3次
                TryConnect();
            }
            CreateConsumer(MQConfig, queueName, action);
        }
        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool CreateConsumer(RabbitSubscribeConfig config, string queueName, Action<IMessageContent> action)
        {
            var messageContent = new RabbitMessageContent("");
            initMessageContent(config, messageContent);
            queueName = string.IsNullOrWhiteSpace(queueName) ? config.QueueName : queueName;
            messageContent.QueueName = queueName;

           //创建新生产者
           var channel = CreateChannel();
            switch (config.ExchangeType)
            {
                case RabbitExchangeType.simple:
                    {
                        /*
                        * 声明队列
                        *  queue: 队列名称
                        *  durable: 设置是否持久化, true表示队列为持久化, 持久化的队列会存盘, 在服务器重启的时候会保证不丢失相关信息
                        *  exclusive: 设置是否排他, true表示队列为排他的, 如果一个队列被设置为排他队列, 该队列仅对首次声明它的连接可见, 并在连接断开时自动删除
                        *  autoDelete: 设置是否自动删除。为true 则设置队列为自动删除
                        *  arguments: 设置队列的其他一些参数, 如 x-message-ttl等
                       */
                        channel.QueueDeclare(queue: messageContent.QueueName, durable: config.Durable, exclusive: false, autoDelete: false, arguments: null);

                        messageContent.ExchangeName = "";
                        messageContent.RoutingKey = messageContent.QueueName;
                        break;
                    }
                case RabbitExchangeType.fanout:
                case RabbitExchangeType.topic:
                case RabbitExchangeType.direct:
                    {
                        if (string.IsNullOrWhiteSpace(messageContent.ExchangeName))
                        {
                            throw new MQException("ExchangeName交换器名称为空");
                        }
                        if (config.ExchangeType == RabbitExchangeType.fanout)
                            messageContent.RoutingKey = "";
                        // 声明交换机
                        channel.ExchangeDeclare(exchange: messageContent.ExchangeName, type: messageContent.ExchangeType.ToString(), durable: false, autoDelete: false, arguments: null);
                        // 声明队列
                        channel.QueueDeclare(queue: messageContent.QueueName, durable: config.Durable, exclusive: false, autoDelete: false, arguments: null);
                        channel.QueueBind(queue: messageContent.QueueName, exchange: messageContent.ExchangeName, routingKey: messageContent.RoutingKey, arguments: null);

                        break;
                    }
                default:
                    {
                        throw new MQException(string.Format("无法识别的MQMode类型:{0}", config.ExchangeType));
                    }
            }
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
#if NET451
                    var message = Encoding.UTF8.GetString(ea.Body);
#else
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
#endif
                   //messageContent.Value = message;
                    var bodyMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<RabbitMessageContent>(message);
                    action?.Invoke(bodyMsg);
                    // Console.WriteLine($"[Simple]received：{message}");
                }
                catch (Exception exception)
                {
                    channel.BasicReject(ea.DeliveryTag, requeue: false);
#if NET451
                    this.ErrorNotice?.Invoke(ea.Exchange, ea.RoutingKey, exception, Encoding.UTF8.GetString(ea.Body));
#else
                    this.ErrorNotice?.Invoke(ea.Exchange, ea.RoutingKey, exception, Encoding.UTF8.GetString(ea.Body.ToArray()));
#endif
                    return;
                }

                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };
            /*
            * 消费消息
            *  queue: 队列名称
            *  autoAck: 是否自动回复Ack
            *  consumerTag: 消费者标签，用来区分多个消费者
            *  noLocal: 设置为true，表示不能将同一个Conenction中生产者发送的消息传递给这个Connection中的消费者
            *  exclusive: 设置是否排他
            *  arguments: 消费者的参数
            *  consumer: 消费者
            */
            channel.BasicConsume(queue: messageContent.QueueName, autoAck: true, consumerTag: "testConsumer", noLocal: false, exclusive: false, arguments: null, consumer: consumer);
           
            return true;
        }

        private void initMessageContent(RabbitSubscribeConfig config, RabbitMessageContent messageContent)
        {
            messageContent.ExchangeType = (int)config.ExchangeType < (int)messageContent.ExchangeType ? messageContent.ExchangeType : config.ExchangeType;
            messageContent.ExchangeName = string.IsNullOrWhiteSpace(messageContent.ExchangeName) ? config.ExchangeName : messageContent.ExchangeName;
            messageContent.QueueName = string.IsNullOrWhiteSpace(messageContent.QueueName) ? config.QueueName : messageContent.QueueName;
            messageContent.RoutingKey = string.IsNullOrWhiteSpace(messageContent.RoutingKey) ? config.RoutingKey : messageContent.RoutingKey;

        }
    }
}
