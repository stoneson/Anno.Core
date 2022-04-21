using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Executor.Active
{
    public abstract class AbstractActiveMessage<TConfig> : AbstractBaseMessaegHandler<TConfig> where TConfig : ServesConfig
    {
        /// <summary>
        /// 初始化Lock对象
        /// </summary>
        private static readonly object ConnectionFactoryLock = new object();
        /// <summary>
        /// 连接工厂
        /// </summary>
        private static ConnectionFactory ConnectionFactory = null;

        /// <summary>
        /// 连接对象
        /// </summary>
        protected IConnection Connection { get; set; }
        /// <summary>
        /// 会话
        /// </summary>
        protected ISession Session { get; set; }
        /// <summary>
        /// 消息消费者
        /// </summary>
        protected IMessageConsumer Consumer { get; set; }

        /// <summary>
        /// 是否打开连接
        /// </summary>
        protected bool IsOpen { get; set; }

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
                    if (ConnectionFactory == null)
                    {
                        ConnectionFactory = new ConnectionFactory(MQConfig.BrokerUri);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(MQConfig.UserName) && string.IsNullOrWhiteSpace(MQConfig.Password))
            {
                Connection = ConnectionFactory.CreateConnection();
            }
            else
            {
                Connection = ConnectionFactory.CreateConnection(MQConfig.UserName, MQConfig.Password);
            }
            Connection.Start();
            //设置连接最大尝试次数
            RedeliveryPolicy(Connection.RedeliveryPolicy);
            //创建连接会话
            Session = CreateSession(Connection);

            //打开状态
            IsOpen = true;
        }

        /// <summary>
        /// 尝试Open是否能成功
        /// </summary>
        /// <param name="tryOpenCount"></param>
        public void TryRequireOpen(int tryOpenCount)
        {
            int defaultValue = tryOpenCount;
            while (true)
            {
                try
                {
                    Open();
                    break;
                }
                catch (Exception ex)
                {
                    if (tryOpenCount <= 0)
                    {
                        throw ex;
                    }
                    Console.WriteLine("正在尝试连接服务实例" + (defaultValue - tryOpenCount + 1) + "次失败，异常信息为：" + ex.Message);
                    tryOpenCount -= 1;
                    System.Threading.Thread.Sleep(30 * 1000);
                }
            }
        }

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
        /// 设置发送策略
        /// </summary>
        /// <param name="policy"></param>
        public virtual void RedeliveryPolicy(IRedeliveryPolicy policy)
        {
            //默认发送6次
            policy.MaximumRedeliveries = 6;
            //发送时间间隔
            policy.InitialRedeliveryDelay = 3000;
        }

        /// <summary>
        /// 创建session
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual ISession CreateSession(IConnection connection)
        {
            //Session.AUTO_ACKNOWLEDGE为自动确认，客户端发送和接收消息不需要做额外的工作。哪怕是接收端发生异常，也会被当作正常发送成功。  
            //Session.CLIENT_ACKNOWLEDGE为客户端确认。客户端接收到消息后，必须调用javax.jms.Message的acknowledge方法。jms服务器才会当作发送成功，并删除消息。  
            //DUPS_OK_ACKNOWLEDGE允许副本的确认模式。一旦接收方应用程序的方法调用从处理消息处返回，会话对象就会确认消息的接收；而且允许重复确认。
            return connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
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
            return (config is ActiveSubscribeConfig);
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
            return content is ActiveMessageContent;
        }

    }
}
