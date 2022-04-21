using Anno.EventBus.Model;
using Anno.EventBus.Model.Config.Producer;
using Anno.EventBus.Model.Config.Subscribe;
using Anno.EventBus.Model.Message;
using System;

namespace Anno.EventBus.Executor
{
    public interface IBaseMessaegHandler<TConfig> where TConfig : ServesConfig
    {
        /// <summary>
        /// 消息处理异常事件
        /// </summary>
        ConsumeErrorNotice ErrorNotice { get; set; }
        /// <summary>
        /// 消息队列参数配置
        /// </summary>
        TConfig MQConfig { get; set; }

        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="msg"></param>
        void WriteLog(string msg);

        /// <summary>
        /// 错误信息记录
        /// </summary>
        /// <param name="msg"></param>
        void WriteError(string msg);

        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="exception"></param>
        void WriteException(Exception ex);

        /// <summary>
        /// 消费者参数对象类型校验
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
         bool IsSubscribeConfig(ISubscribeConfig config);

        /// <summary>
        /// 消息参数对象类型校验
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
         bool IsMessageContent(IMessageContent content);

    }

    /// <summary>
    /// MQ消费base基类,存放一些通用方法
    /// </summary>
    public abstract class AbstractBaseMessaegHandler<TConfig> : IBaseMessaegHandler<TConfig> where TConfig : ServesConfig
    {
        public ConsumeErrorNotice ErrorNotice { get; set; }
        /// <summary>
        /// 消息队列参数配置
        /// </summary>
        public virtual TConfig MQConfig { get; set; }

        //public AbstractBaseMessaegHandler(TConfig mqConfig)
        //{
        //    MQConfig = mqConfig;
        //}

        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="msg"></param>
        public virtual void WriteLog(string msg)
        {
            Log.Log.Info(msg);
        }

        /// <summary>
        /// 错误信息记录
        /// </summary>
        /// <param name="msg"></param>
        public virtual void WriteError(string msg)
        {
            Log.Log.Error(msg);
        }

        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="exception"></param>
        public virtual void WriteException(Exception ex)
        {
            Log.Log.Error(ex);
        }

        /// <summary>
        /// 消费者参数对象类型校验
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public abstract bool IsSubscribeConfig(ISubscribeConfig config);

        /// <summary>
        /// 消息参数对象类型校验
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public abstract bool IsMessageContent(IMessageContent content);

    }
}
