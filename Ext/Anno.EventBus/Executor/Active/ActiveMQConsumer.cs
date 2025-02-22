﻿using Apache.NMS;
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
        /// <summary>
        /// 构造方法
        /// </summary>
        public ActiveMQConsumer()
        {
            if (MQConfig == null)
                this.MQConfig = new ActiveSubscribeConfig();
            //配置参数校验
            MQConfig.Check();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="config"></param>
        public ActiveMQConsumer(ActiveSubscribeConfig config) : this()
        {
            if (config != null)
                this.MQConfig = config;
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
                if (string.IsNullOrWhiteSpace(Config.MQFilterName))
                    consumer = Session.CreateConsumer(new ActiveMQQueue(queueName));
                else
                    consumer = Session.CreateConsumer(new ActiveMQQueue(queueName), Config.MQFilterName);
            }
            //广播模式
            else if (Config.ActiveMQType == ActiveMQTypeEnum.Topic)
            {
                if (string.IsNullOrWhiteSpace(Config.MQFilterName))
                    consumer = Session.CreateConsumer(new ActiveMQTopic(queueName));
                else
                    consumer = Session.CreateConsumer(new ActiveMQTopic(queueName), Config.MQFilterName);
            }
            else
            {
                throw new MQException(string.Format("无法识别的MQMode类型:{0}", Config.ActiveMQType));
            }
            DicConsumer[queueName + Config.MQFilterName] = consumer;
            return consumer;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
        {
            foreach (var item in DicConsumer)
            {
                item.Value.Close();
            }
            DicConsumer?.Clear();

            Session?.Close();
            Connection?.Close();
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
            if (!DicConsumer.TryGetValue(queueName + MQConfig.MQFilterName, out var consumer))
            {
                consumer = CreateConsumer(queueName, MQConfig);
                consumer.Listener += new MessageListener(msg =>
                {
                    if (msg is ITextMessage text)
                    {
                        var bodyMsg = new ActiveMessageContent(text.Text);
                        action?.Invoke(bodyMsg);
                    }
                });
            }
        }

    }
}
