using System;
using System.Collections.Generic;
#if !NETFRAMEWORK
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
#else
using System.Configuration;
#endif
namespace Anno.Const
{
    public static class AppSettings
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static String ConnStr { get; set; }

        /// <summary>
        /// 用户重置密码的时候的默认密码
        /// </summary>
        public static String DefaultPwd { get; set; } = "Anno";

        /// <summary>
        /// Ioc插件DLL列表
        /// </summary>
        public static List<string> IocDll { get; set; } = new List<string>();

        static AppSettings()
        {
            try
            {
#if NETFRAMEWORK
                ConfigurationManager.RefreshSection("appSettings");
#else
                LoadConfiguration();
#endif
            }
            catch
            {
            }
        }


#if !NETFRAMEWORK
        private static IConfiguration s_ConfigJson = null;
        private static System.Collections.Specialized.NameValueCollection _configuration = null;
        /// <summary>
        /// 配置节点关键字
        /// </summary>
        private static string _configSection = "AppSettings";

        /// <summary>
        /// 解析Json文件
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <returns></returns>
        private static IConfiguration LoadJsonFile(string FilePath)
        {
            //#if !DEBUG
            s_ConfigJson = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(FilePath, optional: false, reloadOnChange: true)
                .Build();
            return s_ConfigJson;
        }
        /// <summary>
        /// 读取配置文件内容
        /// </summary>
        private static void LoadConfiguration(string configPath = "")
        {
            if (string.IsNullOrEmpty(configPath)) configPath = "appsettings.json";
            configPath = System.IO.Path.Combine(AppContext.BaseDirectory, configPath);

            s_ConfigJson = LoadJsonFile(configPath);
            if (s_ConfigJson == null)
            {
                Console.WriteLine(" s_ConfigJson == null ");
                return;
            }
            //--------------------------------------------------------------
            _configuration = new System.Collections.Specialized.NameValueCollection();
            foreach (var prop in s_ConfigJson.GetChildren())
            {
                if (!string.IsNullOrEmpty(prop.Value))
                    _configuration[prop.Key] = prop.Value;
            }
            //LogManager.Logger.Info("AppSettings=" + _configuration.Count);
            //--------------------------------------------------------------
            var configSteps = s_ConfigJson.GetSection(_configSection);
            foreach (var prop in configSteps.GetChildren())
            {
                if (!string.IsNullOrEmpty(prop.Value))
                    _configuration[prop.Key] = prop.Value;//.ToString();
            }
            //_configurationCollection = _configuration;
            //if(LoggingHelper.IsDebugEnabled)
            Console.WriteLine("AppSettings Count=" + _configuration.Count);
            Console.WriteLine("LoadConfiguration end ");
        }
#endif
        #region GetAppSettings
        /// <summary>
        /// GetAppSettings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSettings(string key, string def = "")
        {
            return GetAppSettings(key, def, "");
        }
        /// <summary>
        /// GetAppSettings
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="def"></param>
        /// <param name="configPath"></param>
        /// <returns></returns>
        public static string GetAppSettings(string strKey, string def = "", string configPath = "")
        {
            try
            {
#if !NETFRAMEWORK
                if (!string.IsNullOrEmpty(configPath))
                    LoadConfiguration(configPath);

                if (!strKey.StartsWith("AppSettings:", StringComparison.OrdinalIgnoreCase))
                {
                    strKey = "AppSettings:" + strKey;
                }
                var tem = s_ConfigJson[strKey];
                if (string.IsNullOrEmpty(tem))
                    return def;
                return tem.Trim();
#else
                if (!string.IsNullOrEmpty(configPath))
                {
                    strKey = strKey.Trim().ToLower();
                    var config = ConfigurationManager.OpenExeConfiguration(string.IsNullOrEmpty(configPath) ? System.Reflection.Assembly.GetEntryAssembly().Location : configPath);
                    foreach (string key in config.AppSettings.Settings.AllKeys)
                    {
                        if (key.Trim().ToLower() == strKey)
                        {
                            return config.AppSettings.Settings[strKey].Value.ToString();
                        }
                    }
                }
                var tem = System.Configuration.ConfigurationManager.AppSettings[strKey];
                if (string.IsNullOrEmpty(tem))
                    return def;
                return tem.Trim();
#endif
            }
            catch { }
            return def;
        }
        #endregion
    }
}
