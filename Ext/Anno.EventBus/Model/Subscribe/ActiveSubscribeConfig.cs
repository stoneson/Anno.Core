using Anno.EventBus.Model.Enums;
using System;

namespace Anno.EventBus.Model.Config.Subscribe
{
    public class ActiveSubscribeConfig : SubscribeConfig
    {
        public ActiveSubscribeConfig() : base()
        {
        }
        /// <summary>
        /// 订阅配置信息，这些参数是必须要的
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="queueName"></param>
        /// <param name="action"></param>
        public ActiveSubscribeConfig(string server, string username, string password) : base(server, username, password)
        {
            this.BrokerUri = server;
            this.UserName = username;
            this.Password = password;
        }

        /// <summary>
        /// 队列类型
        /// </summary>
        public ActiveMQTypeEnum ActiveMQType { get; set; } = ActiveMQTypeEnum.Queue;

        /// <summary>
        /// filter
        /// </summary>
        public string MQFilterName { get; set; }

        protected override void Init()
        {
            base.Init();
            try
            {
                MQFilterName = this.GetAppSettings("ActiveMQFilterName", "").Trim();
                try
                {
                    Enum.TryParse(this.GetAppSettings("ActiveMQType", ActiveMQTypeEnum.Queue.ToString()).Trim(), out ActiveMQTypeEnum mqType);
                    ActiveMQType = mqType;
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
