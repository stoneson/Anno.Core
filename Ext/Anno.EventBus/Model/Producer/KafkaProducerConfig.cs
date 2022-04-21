using Confluent.Kafka;
using System;

namespace Anno.EventBus.Model.Config.Producer
{
    public class KafkaProducerConfig : ProducerConfigs
    {

        /// <summary>
        /// 安全认证类型，这两个属性别乱调整，我们服务端搭建采用了SaslPlaintext安全认证模式
        /// 把set操作权限换成private ，后期如果认证模式修改了把private权限放开即可
        /// </summary>
        private SecurityProtocol Protocol { get; set; } = SecurityProtocol.SaslPlaintext;

        /// <summary>
        /// SaslMechanism
        /// 把set操作权限换成private ，后期如果认证模式修改了把private权限放开即可
        /// </summary>
        private SaslMechanism Mechanism { get; set; } = SaslMechanism.Plain;

        /// <summary>
        /// 如果是ALL表示消息要求全部同步到其它broker副本上才能成功，但这样效率低
        /// 这个参数很重要，没事别去修改这个参数，根据实际情况修改这个参数；我们默认Leader
        /// </summary>
        public Acks Ack { get; set; } = Acks.Leader;
        public KafkaProducerConfig() : base() { }
        public KafkaProducerConfig(string server, string username, string password, string queueName) : base(server, username, password)
        {
            //broker参数
            this.BrokerUri = server;
            this.UserName = username;
            this.Password = password;
            //topic
            this.QueueName = queueName;
        }

        /// <summary>
        /// 发送完成
        /// </summary>
        public Action<DeliveryReport<string, string>> CompleteHandler { get; set; }

        /// <summary>
        /// 构建ProducerConfig对象
        /// </summary>
        /// <returns></returns>
        public ProducerConfig Build()
        {
            ProducerConfig config = new ProducerConfig()
            {
                BootstrapServers = this.BrokerUri,
                Acks = this.Ack
            };

            //需要校验用户
            if (!string.IsNullOrWhiteSpace(this.UserName) && !string.IsNullOrWhiteSpace(this.Password))
            {
                config.SecurityProtocol = this.Protocol;
                config.SaslMechanism = this.Mechanism;
                config.SaslUsername = this.UserName;
                config.SaslPassword = this.Password;
            }
            return config;
        }

        protected override void Init()
        {
            base.Init();
            try
            {
                Enum.TryParse(this.GetAppSettings("KafkaAck", Acks.Leader.ToString()).Trim(), out Acks ack);
                Ack = ack;
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }
}
