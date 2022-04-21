using Confluent.Kafka;
using Anno.EventBus.Model.Exceptions;
using System;

namespace Anno.EventBus.Model.Config.Subscribe
{
    public class KafkaSubscribeConfig : SubscribeConfig
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
        /// 消费者组ID
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 是否自动提交，这个参数别乱去修改（慎重）
        /// </summary>
        public bool EnableAutoCommit { get; set; } = false;

        //earliest：当各分区下有已提交的offset时，从提交的offset开始消费；无提交的offset时，从头开始消费
        //latest：当各分区下有已提交的offset时，从提交的offset开始消费；无提交的offset时，消费新产生的该分区下的数据
        //none：topic各分区都存在已提交的offset时，从offset后开始消费；只要有一个分区不存在已提交的offset，则抛出异常
        public AutoOffsetReset OffsetReset { get; set; } = AutoOffsetReset.Earliest;

        /// <summary>
        /// 消费失败是否转发到DLQ队列
        /// </summary>
        public bool TransformToDLQ { get; set; } = true;
        public KafkaSubscribeConfig() :
           base()
        {
            
        }
        /// <summary>
        /// 这些参数是必备的，所以创建一个有参构造
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="groupId"></param>
        public KafkaSubscribeConfig(string server, string username, string password, string groupId) 
            : base(server, username, password)
        {
            //server信息
            this.BrokerUri = server;
            this.UserName = username;
            this.Password = password;
            //队列信息
            this.GroupId = groupId;
        }

        /// <summary>
        /// 构建ConsumerConfig对象
        /// </summary>
        /// <returns></returns>
        public ConsumerConfig Build()
        {
            var config = new ConsumerConfig()
            {
                BootstrapServers = this.BrokerUri,
                GroupId = this.GroupId,
                AutoOffsetReset = this.OffsetReset,
                EnableAutoCommit = this.EnableAutoCommit
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

        /// <summary>
        /// 参数配置校验
        /// </summary>
        /// <returns></returns>
        public override bool Check()
        {
            bool isOk = base.Check();
            if (!isOk)
            {
                if (string.IsNullOrWhiteSpace(this.GroupId))
                {
                    throw new MQException("MQ消费者群组不能为空");
                }
                isOk = true;
            }
            return isOk;
        }

        protected override void Init()
        {
            base.Init();
            GroupId = this.GetAppSettings("KafkaGroupId", "").Trim();
            try
            {
               var temp = this.GetAppSettings("KafkaEnableAutoCommit", "false").Trim();
                this.EnableAutoCommit = bool.Parse(temp);
            }
            catch (Exception ex) { this.EnableAutoCommit = false; Log.Log.Error(ex.ToString()); }
            try
            {
                var temp = this.GetAppSettings("KafkaTransformToDLQ", "false").Trim();
                this.TransformToDLQ = bool.Parse(temp);
            }
            catch (Exception ex) { this.TransformToDLQ = false; Log.Log.Error(ex.ToString()); }
            try
            {
                Enum.TryParse(this.GetAppSettings("KafkaOffsetReset", AutoOffsetReset.Earliest.ToString()).Trim(), out AutoOffsetReset ack);
                OffsetReset = ack;
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }
}
