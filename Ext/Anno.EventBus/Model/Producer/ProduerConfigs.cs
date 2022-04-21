using Anno.EventBus.Model.Exceptions;
using System;

namespace Anno.EventBus.Model.Config.Producer
{
    public class ProducerConfigs : ServesConfig, IProducerConfig
    {
        public ProducerConfigs() : base()
        {
        }
        public ProducerConfigs(string servers, string username, string password) : base(servers, username, password)
        {
        }
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 默认校验OK
        /// </summary>
        /// <returns></returns>
        public virtual bool Check()
        {
            if (string.IsNullOrWhiteSpace(this.BrokerUri))
            {
                throw new MQException("BrokerUri参数不能为空");
            }
            if (string.IsNullOrWhiteSpace(this.QueueName))
            {
                throw new MQException("QueueName参数不能为空");
            }
            return true;
        }

        protected override void Init()
        {
            base.Init();
            try
            {
                QueueName = this.GetAppSettings("MQQueueName", "").Trim();
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }

}
