using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Anno.Const.Enum;
using Anno.EngineData.Filters;

namespace Anno.EngineData
{
    /// <summary>
    /// 数据集散中心
    /// </summary>
    public static class Engine
    {
        private readonly static Type enumType = typeof(Enum);
        /// <summary>
        /// 转发器
        /// </summary>
        /// <param name="input">表单数据</param>
        /// <returns></returns>
        public static ActionResult Transmit(Dictionary<string, string> input)
        {
            #region 查找路由信息RoutInfo
            var key = $"{input[Eng.NAMESPACE]}Service.{input[Eng.CLASS]}Module/{input[Eng.METHOD]}";

            var exists = Routing.Routing.Router.TryGetValue(key, out Routing.RoutInfo routInfo);
            if (exists == false)
            {
                key = $"{input[Eng.NAMESPACE]}.{input[Eng.CLASS]}/{input[Eng.METHOD]}";
                exists = Routing.Routing.Router.TryGetValue(key, out routInfo);
            }
            if (exists)
            {
                try
                {
                    return Transmit(input, routInfo);
                }
                catch (Exception ex)
                {
                    //记录日志
                    Log.Log.Error(ex, routInfo.RoutModuleType);
                    return new ActionResult()
                    {
                        Status = false,
                        OutputData = null,
                        Msg = ex.InnerException?.Message ?? ex.Message
                    };
                }
            }
            else
            {
                return new ActionResult()
                {
                    Status = false,
                    OutputData = null,
                    Msg = $"在【{input[Eng.NAMESPACE]}】中找不到【{input[Eng.CLASS]}.{input[Eng.METHOD]}】！"
                };
            }
            #endregion
        }
        /// <summary>
        /// 转发器异步
        /// </summary>
        /// <param name="input">表单数据</param>
        /// <returns></returns>
#if NET40
        public static Task<ActionResult> TransmitAsync(Dictionary<string, string> input)
        {
            return TaskEx.Run(() => Transmit(input));
#else
        public static async Task<ActionResult> TransmitAsync(Dictionary<string, string> input)
        {
            return await Task.Run(() => Transmit(input)).ConfigureAwait(false);
#endif
        }
        /// <summary>
        /// 根据服务转发
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type">表示类型声明：类类型、接口类型、数组类型、值类型、枚举类型、类型参数、泛型类型定义，以及开放或封闭构造的泛型类型</param>
        /// <returns></returns>
        public static ActionResult Transmit(Dictionary<string, string> input, Routing.RoutInfo routInfo)
        {
            BaseModule module = null;
            try
            {
                #region Cache
                string key = string.Empty;
                if (routInfo.CacheMiddleware?.Count > 0)
                {
                    key = GetDicStrHashCode(input);
                    if (TryGetCache(routInfo, key, out ActionResult rltCache))
                    {
                        return rltCache;
                    }
                }
                #endregion

                if (routInfo.RoutMethod == null)
                {
                    return new ActionResult()
                    {
                        Status = false,
                        OutputData = null,
                        Msg = $"在【{input[Eng.NAMESPACE]}】中找不到【{input[Eng.CLASS]}.{input[Eng.METHOD]}】！"
                    };
                }
                if (typeof(BaseModule).IsAssignableFrom(routInfo.RoutModuleType))
                {
                    List<object> lo = new List<object>() { input };
                    module = Loader.IocLoader.Resolve<BaseModule>(routInfo.RoutModuleType);
                    var init = module.Init(input);
                    if (!init)
                    {
                        return new ActionResult()
                        {
                            Status = false,
                            Msg = "Init拦截！"
                        };
                    }
                    #region Authorization
                    for (int i = 0; i < routInfo.AuthorizationFilters.Count; i++)
                    {
                        routInfo.AuthorizationFilters[i].OnAuthorization(module);
                        if (!module.Authorized)
                        {
                            return module.ActionResult == null ? new ActionResult()
                            {
                                Status = false,
                                OutputData = 401,
                                Msg = "401,Unauthrized"
                            } : module.ActionResult
                            ;
                        }
                    }
                    #endregion
                    #region OnActionExecuting
                    for (int i = 0; i < routInfo.ActionFilters.Count; i++)
                    {
                        routInfo.ActionFilters[i].OnActionExecuting(module);
                    }
                    #endregion

                    #region 调用业务方法
                    object rltCustomize = null;
                    if (routInfo.ReturnTypeIsTask)
                    {
                        var rlt = routInfo.RoutMethod.FastInvoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray()) as Task;
                        //(routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray()) as Task);
                        rltCustomize = FastReflection.FastGetValue("Result", rlt);// routInfo.RoutMethod.ReturnType.GetProperty("Result").GetValue(rlt, null);
                    }
                    else
                    {
                        rltCustomize = routInfo.RoutMethod.FastInvoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray());
                        //routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray());
                    }

                    if (routInfo.ReturnTypeIsIActionResult && rltCustomize != null)
                    {
                        module.ActionResult = rltCustomize as ActionResult;
                    }
                    else
                    {
                        module.ActionResult = new ActionResult(true, rltCustomize);
                    }
                    #endregion

                    #region OnActionExecuted
                    for (int i = (routInfo.ActionFilters.Count - 1); i >= 0; i--)
                    {
                        routInfo.ActionFilters[i].OnActionExecuted(module);
                    }
                    #endregion
                    #region CacheMiddleware
                    if (routInfo.CacheMiddleware.Count > 0)
                    {
                        AddCache(routInfo, key, module.ActionResult);
                    }
                    #endregion

                    return module.ActionResult;
                }
                else
                {
                    #region 调用业务方法
                    object rltCustomize = null;
                    if (routInfo.ReturnTypeIsTask)
                    {
                        var rlt = routInfo.RoutMethod.FastInvoke(routInfo.RoutModuleType, DicToParameters(routInfo.RoutMethod, input).ToArray()) as Task;
                        //(routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray()) as Task);
                        rltCustomize = FastReflection.FastGetValue("Result", rlt);// routInfo.RoutMethod.ReturnType.GetProperty("Result").GetValue(rlt, null);
                    }
                    else
                    {
                        rltCustomize = routInfo.RoutMethod.FastInvoke(routInfo.RoutModuleType, DicToParameters(routInfo.RoutMethod, input).ToArray());
                        //routInfo.RoutMethod.Invoke(module, DicToParameters(routInfo.RoutMethod, input).ToArray());
                    }

                    if (routInfo.ReturnTypeIsIActionResult && rltCustomize != null)
                    {
                        return rltCustomize as ActionResult;
                    }
                    else
                    {
                        return new ActionResult(true, rltCustomize);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                if (routInfo.RoutMethod != null && module != null)
                {
                    foreach (var ef in routInfo.ExceptionFilters)
                    {
                        ef.OnException(ex, module);
                    }
                }
#if DEBUG
                //记录日志
                Log.Log.Error(ex, routInfo.RoutModuleType);
#endif
                return new ActionResult()
                {
                    Status = false,
                    OutputData = null,
                    Msg = ex.InnerException?.Message ?? ex.Message
                };
            }
        }

        /// <summary>
        /// 根据服务转发
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type">表示类型声明：类类型、接口类型、数组类型、值类型、枚举类型、类型参数、泛型类型定义，以及开放或封闭构造的泛型类型</param>
        /// <returns></returns>
#if NET40
        public static Task<ActionResult> TransmitAsync(Dictionary<string, string> input, Routing.RoutInfo routInfo)
        {
            return TaskEx.Run(() => Transmit(input, routInfo));
#else
        public static async Task<ActionResult> TransmitAsync(Dictionary<string, string> input, Routing.RoutInfo routInfo)
        {
            return await Task.Run(() => Transmit(input, routInfo)).ConfigureAwait(false);
#endif
        }
        /// <summary>
        /// 扩展属性校验
        /// </summary>
        /// <param name="method"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static List<object> DicToParameters(MethodInfo method, Dictionary<string, string> input)
        {
            List<object> parameters = new List<object>();
            foreach (var p in method.GetParameters())
            {
                if (p.GetCustomAttributes<FromBodyAttribute>().Any())
                {
                    parameters.Add(p.ToObjFromDic(input));
                    continue;
                }
                else if (input.ContainsKey(p.Name))
                {
                    if (p.ParameterType.FullName.StartsWith("System.Collections.Generic"))
                    {
                        parameters.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(input[p.Name], p.ParameterType));
                    }
                    else if (p.ParameterType.FullName.StartsWith("System."))//系统基础数据类型
                    {
                        parameters.Add(Convert.ChangeType(input[p.Name], p.ParameterType));//枚举
                    }
                    else if (p.ParameterType.BaseType == enumType)
                    {

                        parameters.Add(Enum.Parse(p.ParameterType, input[p.Name]));
                    }
                    else // 系统基础数据类型、枚举 之外。例如 结构体、类、匿名对象
                    {
                        parameters.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(input[p.Name], p.ParameterType));
                    }
                }
                //-------------------------------解决post body自动识别入参问题-------------------------------------------------------------------------------
                else if (input.Keys.Contains("body", StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        var json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(input["body"]);
                        if (json != null)
                        {
                            Newtonsoft.Json.Linq.JToken valueStr;
                            if (json.TryGetValue(p.Name, StringComparison.OrdinalIgnoreCase, out valueStr))
                            {
                                if (valueStr != null)
                                {
                                    var value = valueStr.ToObject(p.ParameterType);
                                    parameters.Add(value);

                                    continue;
                                }
                            }
                        }
                        parameters.Add(p.ToObjFromDic(input));
                    }
                    catch (Exception ex)
                    {   //记录日志
                        //Log.Log.Error(ex, p.ParameterType);
                        throw new Exception("body 转JSON对象时异常：body非正确JSON格式，" + ex.Message);
                    }
                }
                //--------------------------------------------------------------------------------------------------------------
                else
                {
                    parameters.Add(default);
                }
            }
            return parameters;
        }

        private static bool TryGetCache(Routing.RoutInfo routInfo, string key, out ActionResult actionResult)
        {
            actionResult = null;
            for (int i = 0; i < routInfo.CacheMiddleware.Count; i++)
            {
                var cm = routInfo.CacheMiddleware[i];
                if (cm.TryGetCache(key, out actionResult))
                {
                    return true;
                }
            }
            return false;
        }
        private static void AddCache(Routing.RoutInfo routInfo, string key, ActionResult actionResult)
        {
            for (int i = 0; i < routInfo.CacheMiddleware.Count; i++)
            {
                var cm = routInfo.CacheMiddleware[i];
                cm.SetCache(key, actionResult);
            }
        }

        private static string GetDicStrHashCode(Dictionary<string, string> input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in input)
            {
                if (item.Key == "X-Original-For"
                    || item.Key == "profile"
                      || item.Key == "TraceId"
                        || item.Key == "PreTraceId"
                          || item.Key == "GlobalTraceId"
                            || item.Key == "AppName"
                              || item.Key == "Target"
                                || item.Key == "AppNameTarget"
                                 || item.Key == "TTL"
                                  || item.Key == "t"
                    )//排除系统内置参数
                {
                    continue;
                }
                stringBuilder.Append(item.Key);
                stringBuilder.Append(item.Value);
            }
            return stringBuilder.ToString().HashCode();
        }

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="buffer">被操作的源数据流</param>
        /// <param name="algo">加密算法</param>
        /// <returns>哈希值16进制字符串</returns>
        private static string HashCode(this string str, string algo = "md5")
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            HashAlgorithm hashAlgorithm = null;
            switch (algo)
            {
                case "sha1":
                    hashAlgorithm = new SHA1CryptoServiceProvider();
                    break;
                case "md5":
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
                default:
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
            }

            var hash = hashAlgorithm.ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        private static object ToObjFromDic(this ParameterInfo parameterInfo, Dictionary<string, string> input)
        {
            var type = parameterInfo.ParameterType;
            var body = type.Assembly.CreateInstance(type.FullName);
            List<PropertyInfo> targetProps = type.GetProperties().Where(p => p.CanWrite == true).ToList();
            var fields = type.GetFields().Where(p => p.IsPublic).ToList();
            if (targetProps != null && targetProps.Count > 0)
            {
                var keys = input.Keys.ToList();
                var isExists = false;
                foreach (var propertyInfo in targetProps)
                {
                    isExists = false;
                    foreach (var key in keys)
                    {
                        if (key.Equals(propertyInfo.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var valueStr = input[key];
                            try
                            {
                                if (propertyInfo.PropertyType.IsPrimitive ||
                                    (propertyInfo.PropertyType.FullName.StartsWith("System.") && !propertyInfo.PropertyType.FullName.StartsWith("System.Collections.Generic")))
                                {
                                    var value = Convert.ChangeType(valueStr, propertyInfo.PropertyType);
                                    propertyInfo.SetValue(body, value, null);
                                }
                                else if (propertyInfo.PropertyType.BaseType == enumType)
                                {
                                    var value = Enum.Parse(propertyInfo.PropertyType, valueStr);
                                    propertyInfo.SetValue(body, value, null);
                                }
                                else
                                {
                                    var value = Newtonsoft.Json.JsonConvert.DeserializeObject(valueStr, propertyInfo.PropertyType);
                                    propertyInfo.SetValue(body, value, null);
                                }
                                isExists = true;
                            }
                            catch { }
                            break;
                        }
                    }
                    //---------------------解决post body自动识别入参问题-----------------------------------------------------------------------------------------
                    if (isExists == false && (keys.Contains(parameterInfo.Name, StringComparer.OrdinalIgnoreCase)
                        || keys.Contains("body", StringComparer.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            Newtonsoft.Json.Linq.JObject json = null;
                            if (keys.Contains(parameterInfo.Name, StringComparer.OrdinalIgnoreCase))
                                json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(input[parameterInfo.Name]);
                            if (json == null)
                                json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(input["body"]);
                            if (json != null)
                            {
                                Newtonsoft.Json.Linq.JToken valueStr;
                                if (json.TryGetValue(propertyInfo.Name, StringComparison.OrdinalIgnoreCase, out valueStr))
                                {
                                    if (valueStr != null)
                                    {
                                        var value = valueStr.ToObject(propertyInfo.PropertyType);
                                        propertyInfo.SetValue(body, value, null);
                                    }
                                }
                                else if (json.TryGetValue(parameterInfo.Name, StringComparison.OrdinalIgnoreCase, out valueStr))
                                {
                                    var value = valueStr.ToObject(parameterInfo.ParameterType);
                                    if (value != null)
                                    {
                                        return value;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {   //记录日志
                            Log.Log.Error(ex, parameterInfo.ParameterType);
                            throw new Exception("body 转JSON对象时异常：body非正确JSON格式，" + ex.Message);
                        }
                    }
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            return body;
        }
    }
}
