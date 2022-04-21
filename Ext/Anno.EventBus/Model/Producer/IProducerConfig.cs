using System;

namespace Anno.EventBus.Model.Config.Producer
{
    public interface IProducerConfig
    {

        /// <summary>
        /// 校验参数配置
        /// </summary>
        /// <returns></returns>
        bool Check();

    }
}
