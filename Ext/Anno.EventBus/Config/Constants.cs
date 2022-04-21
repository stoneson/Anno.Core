
namespace Anno.EventBus.Config
{
    public sealed class Constants
    {
        /// <summary>
        /// GetAppSettings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSettings(string key, string def = "")
        {
            return Anno.Const.AppSettings.GetAppSettings(key, def, "");
        }
        public static string GetAppSettings(string strKey, string def = "", string configPath = "")
        {
            return Anno.Const.AppSettings.GetAppSettings(strKey, def, configPath);
        }
    }
}
