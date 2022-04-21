using Anno.EventBus.Config;
using System;
using System.Configuration;

namespace Anno.EventBus.Model
{
    public class ServesConfig
    {
        /// <summary>
        /// 消息队列类型 MQ类型[1=ActiveMQ;2=Kafka;3=RabbitQM
        /// </summary>
        public Model.Enums.MQTypeEnum MQType { get; set; }
        /// <summary>
        /// 服务URL
        /// </summary>
        public string BrokerUri { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        public ServesConfig()
        {
            Init();
        }

        public ServesConfig(string servers,string username,string password)
        {
            Init();
            this.BrokerUri = servers;
            this.UserName = username;
            this.Password = password;
        }
        protected virtual void Init()
        {
            try
            {
                BrokerUri = this.GetAppSettings("MQBrokerUri", "").Trim();
                UserName = this.GetAppSettings("MQUserName", "").Trim();
                Password = this.GetAppSettings("MQPassword", "").Trim();
                try
                {
                    Enum.TryParse(this.GetAppSettings("MQType", Enums.MQTypeEnum.RabbitQM.ToString()).Trim(), out Enums.MQTypeEnum mqType);
                    MQType = mqType;
                }
                catch (Exception ex) { Log.Log.Error(ex.ToString()); }
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message);
            }
        }
        #region GetAppSettings
        /// <summary>
        /// GetAppSettings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetAppSettings(string key, string def = "")
        {
            return Constants.GetAppSettings(key, def);
        }

        #endregion
    }
}
