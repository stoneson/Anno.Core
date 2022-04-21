using Anno.EventBus.Model.Enums;
using System;

namespace Anno.EventBus.Model.Config.Producer
{
    public class RabbitProducerConfig : ProducerConfigs
    {

        public RabbitProducerConfig():base() { }

        /// <summary>
        /// 这些参数是必须要有的，所以这里创建一个有参构造
        /// </summary>
        /// <param name="serves"></param>
        /// <param name="queueName"></param>
        /// <param name="exchangeType"></param>
        public RabbitProducerConfig(string server, string username, string password, Enums.RabbitExchangeType exchangeType
            , string queueName, string exchangeName = "", string routingKey = ""):base(server,  username,  password)
        {
            this.BrokerUri = server;
            this.UserName = username;
            this.Password = password;

            ExchangeType = exchangeType;
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }

        /// <summary>
        /// 队列类型
        /// </summary>
        public RabbitExchangeType ExchangeType { get; set; } = RabbitExchangeType.direct;

        /// <summary>
        /// Virtual host to access during this connection.
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        ///  The port to connect on. RabbitMQ.Client.AmqpTcpEndpoint.UseDefaultPort indicates
        ///     the default for the protocol should be used.
        /// </summary>
        public int Port { get; set; } = 5672;
        /// <summary>
        ///  Heartbeat timeout to use when negotiating with the server.
        /// </summary>
        public int RequestedHeartbeat { get; set; }

        /// <summary>
        /// 交换名称
        /// </summary>
        public string ExchangeName { get; set; }

        ///// <summary>
        ///// 交换类型
        ///// </summary>
        //public Enums.RabbitExchangeType ExchangeType { get; set; } = RabbitExchangeType.simple;
        
        /// <summary>
        /// 路由Key，默认的路由Key和队列名称(QueueName)完全一致
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 设置是否持久化, true表示队列为持久化, 持久化的队列会存盘, 在服务器重启的时候会保证不丢失相关信息
        /// </summary>
        public bool Durable { get; set; } = false;

        protected override void Init()
        {
            base.Init();
            try
            {
                VirtualHost = this.GetAppSettings("RabbitVirtualHost", "").Trim();
                ExchangeName = this.GetAppSettings("RabbitExchangeName", "").Trim();
                RoutingKey = this.GetAppSettings("RabbitRoutingKey", "").Trim();
                string temp = "";
                try
                {
                    temp =this.GetAppSettings("RabbitPort", "5672").Trim();
                    this.Port = int.Parse(temp);
                }
                catch (Exception ex) { this.Port = 5672; Log.Log.Error(ex.ToString()); }
                try
                {
                    temp = this.GetAppSettings("RabbitHeartbeat", "5672").Trim();
                    this.RequestedHeartbeat = int.Parse(temp);
                }
                catch (Exception ex) { this.RequestedHeartbeat = 5672; Log.Log.Error(ex.ToString()); }
                try
                {
                    temp = this.GetAppSettings("RabbitDurable", "false").Trim();
                    this.Durable = bool.Parse(temp);
                }
                catch (Exception ex) { this.Durable = false; Log.Log.Error(ex.ToString()); }
                try
                {
                    Enum.TryParse(this.GetAppSettings("RabbitExchangeType", RabbitExchangeType.direct.ToString()).Trim(), out RabbitExchangeType exchangeType);
                    ExchangeType = exchangeType;
                }
                catch (Exception ex) { Log.Log.Error(ex.ToString()); }
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }

}
