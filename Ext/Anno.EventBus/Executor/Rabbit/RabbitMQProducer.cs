
using Anno.EventBus.Interface;
using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Enums;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;

namespace Anno.EventBus.Executor.Rabbit
{
    public class RabbitMQProducer : AbstractRabbitMessage<RabbitProducerConfig>, IProducerChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new RabbitProducerConfig MQConfig { get; set; }
        //ProducerConfigs IBaseMessaegHandler<ProducerConfigs>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as RabbitProducerConfig; }
        //}

        /// <summary>
        /// 队列缓存字典 生产者
        /// </summary>
        private readonly ConcurrentDictionary<string, IModel> producerMaps = new ConcurrentDictionary<string, IModel>();

        /// <summary>
        /// 构造方法
        /// </summary>
        public RabbitMQProducer()
        {
            this.MQConfig = new RabbitProducerConfig();
            //配置参数校验
            MQConfig.Check();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="config"></param>
        public RabbitMQProducer(RabbitProducerConfig config)
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
                //Channel?.Close();
                Connection?.Close();
                //Channel = null;
                Connection = null;
            }
        }

        public void Dispose()
        {
            this.Close();
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
        /// 消息发送，queue默认是Queue队列，不是Topic。这个要注意
        /// </summary>
        /// <param name="message">队列消息</param>
        public void Producer(string message)
        {
            this.Producer(new RabbitMessageContent(message));
        }
        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="messageContent"></param>
        public void Producer(IMessageContent messageContent)
        {
            if (!IsMessageContent(messageContent))
            {
                throw new MQException("messageContent 参数类型不是RabbitMessageContent");
            }
            var message = (RabbitMessageContent)messageContent;
            if (message == null|| string.IsNullOrWhiteSpace(message.Value))
            {
                throw new MQException("发送消息内容为空");
            }
            //发送的消息
            Producer(MQConfig, message);
        }

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool Producer(RabbitProducerConfig config,RabbitMessageContent messageContent)
        {
            if (!IsOpen)
            {
                //默认尝试连接3次
                Open();
            }
            initMessageContent(config, messageContent);
            //创建新生产者
            IModel func(string queueName)
            {
                var channel = CreateChannel();
                switch (messageContent.ExchangeType)
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
                            channel.QueueDeclare(queue: queueName, durable: config.Durable, exclusive: false, autoDelete: false, arguments: null);

                            messageContent.ExchangeName = "";
                            messageContent.RoutingKey = queueName;
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
                            channel.QueueDeclare(queue: queueName, durable: config.Durable, exclusive: false, autoDelete: false, arguments: null);
                            channel.QueueBind(queue: queueName, exchange: messageContent.ExchangeName, routingKey: messageContent.RoutingKey, arguments: null);

                            break;
                        }
                    default:
                        {
                            throw new MQException(string.Format("无法识别的MQMode类型:{0}", config.ExchangeType));
                        }
                }

                return channel;
            }
            var Channel = this.producerMaps.GetOrAdd(messageContent.QueueName, func);
            if (Channel != null)
            {
                var properties = Channel.CreateBasicProperties();
                properties.MessageId = Guid.NewGuid().ToString("N");
                properties.DeliveryMode = config.Durable ? (byte)2 : (byte)1;
                properties.Persistent = config.Durable;
#if NET451
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.UtcTicks);
#else
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
#endif
                /*
                 * 发送消息
                 *  exchange: 交换机，为空表示使用默认的交换机
                 *  routingKey: 路由Key，默认的路由Key和队列名称完全一致
                 *  mandatory: 如果为true, 消息不能路由到指定的队列时，会触发channel.BasicReturn事件，如果为false，则broker会直接将消息丢弃
                 *  basicProperties: 消息属性
                 *  body: 消息内容
                 */
                Channel.BasicPublish(exchange: messageContent.ExchangeName, routingKey: messageContent.RoutingKey, mandatory: false, basicProperties: properties, body: GetMessage(messageContent));
                return true;
            }
            return false;
        }
       
        private void initMessageContent(RabbitProducerConfig config, RabbitMessageContent messageContent)
        {
            messageContent.ExchangeType = (int)config.ExchangeType < (int)messageContent.ExchangeType ? messageContent.ExchangeType : config.ExchangeType;
            messageContent.ExchangeName = string.IsNullOrWhiteSpace(messageContent.ExchangeName) ? config.ExchangeName : messageContent.ExchangeName;
            messageContent.QueueName = string.IsNullOrWhiteSpace(messageContent.QueueName) ? config.QueueName : messageContent.QueueName;
            messageContent.RoutingKey = string.IsNullOrWhiteSpace(messageContent.RoutingKey) ? config.RoutingKey : messageContent.RoutingKey;

        }
    }
}
