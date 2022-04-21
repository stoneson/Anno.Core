using Anno.EventBus.Model.Exceptions;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Model.Config.Subscribe
{
    public class SubscribeConfig : ServesConfig, ISubscribeConfig
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        public SubscribeConfig() : base()
        {
        }
        public SubscribeConfig(string servers, string username, string password) : base(servers, username, password)
        {
        }
        /// <summary>
        /// 校验参数配置
        /// </summary>
        /// <returns></returns>
        public virtual bool Check()
        {
            if (string.IsNullOrWhiteSpace(this.BrokerUri))
            {
                throw new MQException("BrokerUri参数不能为空");
            }
            return true;
        }

        protected override void Init()
        {
            base.Init();
            try
            {
                QueueName = this.GetAppSettings("QueueName", "").Trim();
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }
}
