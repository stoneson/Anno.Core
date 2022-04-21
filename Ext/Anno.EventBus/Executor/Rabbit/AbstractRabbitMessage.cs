using RabbitMQ.Client;
using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;
using RabbitMQ.Client.Events;
using Polly.Retry;
using System.Net.Sockets;
using RabbitMQ.Client.Exceptions;
using Polly;

namespace Anno.EventBus.Executor.Rabbit
{
    public abstract class AbstractRabbitMessage<TConfig> : AbstractBaseMessaegHandler<TConfig> where TConfig : ServesConfig
    {
        /// <summary>
        /// 初始化Lock对象
        /// </summary>
        private static readonly object ConnectionFactoryLock = new object();
        /// <summary>
        /// 连接工厂
        /// </summary>
        private static ConnectionFactory ConnectionFactory = null;
        private static readonly object sync_root = new object();

        /// <summary>
        /// 连接对象
        /// </summary>
        protected IConnection Connection { get; set; }
        
       
        /// <summary>
        /// 是否打开连接
        /// </summary>
        protected bool IsOpen => Connection != null && Connection.IsOpen;

        protected virtual ConnectionFactory getConnectionFactory()
        {
            return new ConnectionFactory();
        }
        /// <summary>
        /// 连接MQ
        /// </summary>
        public virtual void Open()
        {
            if (string.IsNullOrWhiteSpace(MQConfig.BrokerUri))
            {
                throw new MQException("未指定连接消息队列的地址");
            }

            //创建连接对象
            if (ConnectionFactory == null)
            {
                lock (ConnectionFactoryLock)
                {
                    ConnectionFactory = getConnectionFactory();
                }
            }
            //创建链接
            TryConnect();
        }
        #region TryConnect
        public bool TryConnect()
        {
            var FailReConnectRetryCount= 1;
            if (ConnectionFactory == null)
            {
                lock (ConnectionFactoryLock)
                {
                    ConnectionFactory = getConnectionFactory();
                }
            }
            if (ConnectionFactory == null) return false;
            //_logger.WriteLog(LogLevel.Information, "RabbitMQ Client is trying to connect");
            lock (sync_root)
            {
                RetryPolicy policy = RetryPolicy.Handle<Exception>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(FailReConnectRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        Log.Log.Warn(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    Connection = ConnectionFactory.CreateConnection();
                });

                if (IsOpen)
                {
                    Connection.ConnectionShutdown -= OnConnectionShutdown;
                    Connection.CallbackException -= OnCallbackException;
                    Connection.ConnectionBlocked -= OnConnectionBlocked;
                    Connection.ConnectionShutdown += OnConnectionShutdown;
                    Connection.CallbackException += OnCallbackException;
                    Connection.ConnectionBlocked += OnConnectionBlocked;
                    //_logger.WriteLog(LogLevel.Information, $"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    Log.Log.Warn("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }
        }

        private  void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            //if (_disposed)
            //{
            //    return;
            //}
            Log.Log.Warn("A RabbitMQ connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        private  void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            //if (_disposed)
            //{
            //    return;
            //}
            Log.Log.Warn("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnect();
        }

        private  void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            //if (_disposed)
            //{
            //    return;
            //}
            Log.Log.Warn("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnect();
        }
        #endregion

        /// <summary>
        /// 关闭连接
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 暂停队列服务
        /// </summary>
        public virtual void Stop()
        {
            this.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual byte[] GetMessage(RabbitMessageContent messageContent) =>
            System.Text.Encoding.UTF8.GetBytes(messageContent.Value);

        /// <summary>
        /// 获取连接通道模板
        /// </summary>
        /// <returns></returns>
        public virtual IModel CreateChannel()
        {
            if (!IsOpen)
            {
                TryConnect();
            }
            return Connection.CreateModel();
        }
        /// <summary>
        /// 判断配置信息类型是否正确
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public override bool IsSubscribeConfig(ISubscribeConfig config)
        {
            if (config == null)
            {
                throw new MQException("config参数为空");
            }
            return (config is RabbitSubscribeConfig);
        }

        /// <summary>
        /// 判断配置信息类型是否正确
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public override bool IsMessageContent(IMessageContent content)
        {
            if (content == null)
            {
                throw new MQException("config参数为空");
            }
            return content is RabbitMessageContent;
        }

    }
}
