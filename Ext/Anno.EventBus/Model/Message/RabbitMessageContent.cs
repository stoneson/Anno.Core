using System;

namespace Anno.EventBus.Model.Message
{
    public class RabbitMessageContent : MessageContent
    {
        public RabbitMessageContent(string msg)
        {
            this.Value = msg;
            this.Key = "";
        }
        /// <summary>
        /// 交换器名称
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 交换类型
        /// </summary>
        public Enums.RabbitExchangeType ExchangeType { get; set; } = Enums.RabbitExchangeType.simple;
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 路由Key，默认的路由Key和队列名称(QueueName)完全一致
        /// </summary>
        public string RoutingKey { get; set; }
        ///// <summary>
        ///// 设置是否持久化, true表示队列为持久化, 持久化的队列会存盘, 在服务器重启的时候会保证不丢失相关信息
        ///// </summary>
        //public bool Durable { get; set; } = false;
    }
}
