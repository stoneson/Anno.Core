using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    /// <summary>
    /// 插件启动配置
    /// </summary>
    public interface IPlugsConfigurationBootstrap
    {
        /// <summary>
        ///IOC之前，Service 依赖注入构建之前调用
        /// </summary>
        void PreConfigurationBootstrap();
        /// <summary>
        /// 插件启动配置，Service 依赖注入构建之后调用
        /// </summary>
        void ConfigurationBootstrap();
    }
}
