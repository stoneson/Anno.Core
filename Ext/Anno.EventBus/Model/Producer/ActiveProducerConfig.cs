using Anno.EventBus.Model.Enums;
using System;

namespace Anno.EventBus.Model.Config.Producer
{
    public class ActiveProducerConfig : ProducerConfigs
    {

        public ActiveProducerConfig():base() { }

        /// <summary>
        /// 这些参数是必须要有的，所以这里创建一个有参构造
        /// </summary>
        /// <param name="serves"></param>
        /// <param name="queueName"></param>
        /// <param name="activeMQType"></param>
        public ActiveProducerConfig(string server, string username, string password, string queueName, ActiveMQTypeEnum activeMQType = ActiveMQTypeEnum.Queue)
            : base(server, username, password)
        {
            this.BrokerUri = server;
            this.UserName = username;
            this.Password = password;
            //队列信息
            this.QueueName = queueName;
            this.ActiveMQType = activeMQType;
        }

        /// <summary>
        /// 指定使用队列的模式
        /// 10 普通的Queue 20 Topic广播模式
        /// </summary>
        public ActiveMQTypeEnum ActiveMQType { get; set; } = ActiveMQTypeEnum.Queue;

        /// <summary>
        /// 队列过滤字段
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
