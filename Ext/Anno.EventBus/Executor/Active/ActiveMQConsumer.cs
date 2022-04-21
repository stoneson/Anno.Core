using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using Anno.EventBus.Interface;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Enums;
using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Executor.Active
{
    public class ActiveMQConsumer : AbstractActiveMessage<ActiveSubscribeConfig>, ISubscribeChannel
    {
        ///// <summary>
        ///// 消息队列参数配置
        ///// </summary>
        //public new ActiveSubscribeConfig MQConfig { get; set; }
        //SubscribeConfig IBaseMessaegHandler<SubscribeConfig>.MQConfig
        //{
        //    get => MQConfig;
        //    set { MQConfig = value as ActiveSubscribeConfig; }
        //}

        /// <summary>
        /// 构造方法
        /// </summary>
        public ActiveMQConsumer()
        {
            this.MQConfig = new ActiveSubscribeConfig();
            //配置参数校验
            MQConfig.Check();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="config"></param>
        public ActiveMQConsumer(ActiveSubscribeConfig config)
        {
            if (config != null)
                this.MQConfig = config;
            //配置参数校验
            MQConfig.Check();
        }

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <returns></returns>
        public IMessageConsumer CreateConsumer(string queueName, ActiveSubscribeConfig Config)
        {
            if (Session == null)
            {
                throw new MQException("Session is null ");
            }
            IMessageConsumer consumer;
            //普通的queue
            if (Config.ActiveMQType == ActiveMQTypeEnum.Queue)
            {
                consumer = Session.CreateConsumer(new ActiveMQQueue(queueName), Config.MQFilterName);
            }
            //广播模式
            else if (Config.ActiveMQType == ActiveMQTypeEnum.Topic)
            {
                consumer = Session.CreateConsumer(new ActiveMQTopic(queueName), Config.MQFilterName);
            }
            else
            {
                throw new MQException(string.Format("无法识别的MQMode类型:{0}", Config.ActiveMQType));
            }
            return consumer;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            Consumer?.Close();
            Session?.Close();
            Connection?.Close();
            Consumer = null;
            Session = null;
            Connection = null;
            IsOpen = false;
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
                TryRequireOpen(3);
            }

            if (Consumer == null)
            {
                Consumer = CreateConsumer(queueName, MQConfig);
            }

            Consumer.Listener += new MessageListener(msg =>
            {
                if (msg is ITextMessage text)
                {
                    var bodyMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<ActiveMessageContent>(text.Text);
                    action?.Invoke(bodyMsg);
                }
            });
        }

    }
}
