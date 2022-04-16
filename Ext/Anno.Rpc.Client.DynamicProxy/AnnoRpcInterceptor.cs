using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Anno.Rpc.Client.DynamicProxy
{
    using Anno.Rpc.Client;
    public class AnnoRpcInterceptor : IInterceptor
    {
        private static Type _taskType = typeof(Task);
        private static Type rltObjectType = typeof(Anno.EngineData.ActionResult<>);
        public void Intercept(IInvocation invocation)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            AnnoProxyAttribute proxyAttribute = new AnnoProxyAttribute();
            var moduleAttribute = invocation.Method.DeclaringType.GetCustomAttributes<AnnoProxyAttribute>().FirstOrDefault();
            var methodAttribute = invocation.Method.GetCustomAttributes<AnnoProxyAttribute>().FirstOrDefault();

            if (moduleAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(moduleAttribute.Channel))
                {
                    proxyAttribute.Channel = moduleAttribute.Channel;
                }
                if (!string.IsNullOrWhiteSpace(moduleAttribute.Router))
                {
                    proxyAttribute.Router = moduleAttribute.Router;
                }
                if (!string.IsNullOrWhiteSpace(moduleAttribute.Method))
                {
                    proxyAttribute.Method = moduleAttribute.Method;
                }
            }
            if (methodAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(methodAttribute.Channel))
                {
                    proxyAttribute.Channel = methodAttribute.Channel;
                }
                if (!string.IsNullOrWhiteSpace(methodAttribute.Router))
                {
                    proxyAttribute.Router = methodAttribute.Router;
                }
                if (!string.IsNullOrWhiteSpace(methodAttribute.Method))
                {
                    proxyAttribute.Method = methodAttribute.Method;
                }
            }
            if (string.IsNullOrWhiteSpace(proxyAttribute.Channel))
            {
                proxyAttribute.Method = invocation.Method.DeclaringType.Module.Name;
            }
            if (string.IsNullOrWhiteSpace(proxyAttribute.Router))
            {
                proxyAttribute.Method = invocation.Method.DeclaringType.Name;
            }
            if (string.IsNullOrWhiteSpace(proxyAttribute.Method))
            {
                proxyAttribute.Method = invocation.Method.Name;
            }
            input.AddOrUpdate("channel", proxyAttribute.Channel);
            input.AddOrUpdate("router", proxyAttribute.Router);
            input.AddOrUpdate("method", proxyAttribute.Method);
            var _params = invocation.Method.GetParameters();
            for (int i = 0; i < _params.Length; i++)
            {
                //null 不传递
                if (invocation.Arguments[i] == null)
                {
                    continue;
                }
                var param = _params[i];
                if (param.ParameterType.IsClass && param.ParameterType != "".GetType())
                {
                    input.AddOrUpdate(param.Name, JsonConvert.SerializeObject(invocation.Arguments[i]));
                }
                else
                {
                    input.AddOrUpdate(param.Name, invocation.Arguments[i].ToString());
                }
            }
            if (invocation.Method.ReturnType != typeof(void))
            {
                var rltStr = Connector.BrokerDns(input);
                if (rltStr.IndexOf("tatus\":false") > 0)
                {
                    string errorMsg = "服务端未知错误";
                    try
                    {
                        var errorData = JsonConvert.DeserializeObject(rltStr, rltObjectType) as dynamic;
                        errorMsg = errorData.Msg;
                    }
                    catch
                    {
                        int errorMsgLength = rltStr.IndexOf("\",\"Status\"");
                        if (errorMsgLength > 11)
                        {
                            errorMsg = rltStr.Substring(8, errorMsgLength - 8);
                        }
                    }
                    throw new AnnoRpcException(errorMsg);
                }

                bool isTask = false;
                Type realReturnType = invocation.Method.ReturnType;
                if (invocation.Method.ReturnType.BaseType != null && invocation.Method.ReturnType.BaseType.FullName.Equals("System.Threading.Tasks.Task"))
                {
#if NET40
                    var generics = invocation.Method.ReturnType.GetGenericArguments();
#else
                    var generics = invocation.Method.ReturnType.GenericTypeArguments;
#endif
                    if (generics != null && generics.Length > 0)
                    {
                        realReturnType = generics[0];
                    }
                    else
                    {
                        realReturnType = _taskType;
                    }

                    isTask = true;
                }
                else
                {
                    realReturnType = invocation.Method.ReturnType;
                }

                if (realReturnType == _taskType)
                {
#if NET40
                    invocation.ReturnValue = TaskEx.FromResult(realReturnType);
#else
                    invocation.ReturnValue = Task.FromResult(realReturnType);
#endif
                }
                else
                {
                    dynamic returnValue;//= ChangeType(rltStr, invocation.Method.ReturnType);
                    if (typeof(Anno.EngineData.IActionResult).IsAssignableFrom(realReturnType))
                    {
                        returnValue = JsonConvert.DeserializeObject(rltStr, type: realReturnType);
                    }
                    else
                    {
                        var rltType = typeof(Anno.EngineData.ActionResult<>).MakeGenericType(realReturnType);
                        var data = JsonConvert.DeserializeObject(rltStr, rltType)//ChangeType(rltStr, rltType)//  
                            as dynamic;
                        if (data.Status == false && data.OutputData == null && !string.IsNullOrWhiteSpace(data.Msg))
                        {
                            throw new AnnoRpcException(data.Msg);
                        }
                        else
                        {
                            returnValue = data.OutputData;
                        }
                    }
#if NET40
                    invocation.ReturnValue = isTask ? TaskEx.FromResult(returnValue) : returnValue;
#else
                    invocation.ReturnValue = isTask ? Task.FromResult(returnValue) : returnValue;
#endif
                }
            }
            else
            {
                Connector.BrokerDns(input);
            }
        }
        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ChangeType<T>(object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <param name="obj">待转换的对象</param>
        /// <param name="type">目标类型</param>
        /// <returns>转换后的对象</returns>
        public static object ChangeType(object obj, Type type)
        {
            if (type == null) return obj;
            if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (type.IsAssignableFrom(obj.GetType())) return obj;
            else if ((underlyingType ?? type).IsEnum)
            {
                if (underlyingType != null && string.IsNullOrEmpty(obj.ToString())) return null;
                else return Enum.Parse(underlyingType ?? type, obj.ToString());
            }
            // 处理DateTime -> DateTimeOffset 类型
            else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
            {
                return ConvertToDateTimeOffset((DateTime)obj);
            }
            // 处理 DateTimeOffset -> DateTime 类型
            else if (obj.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
            {
                return ConvertToDateTime((DateTimeOffset)obj);
            }
            else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
            {
                try
                {
                    return Convert.ChangeType(obj, underlyingType ?? type, null);
                }
                catch
                {
                    return underlyingType == null ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(obj.GetType())) return converter.ConvertFrom(obj);

                if (obj is string) obj = JsonConvert.DeserializeObject(obj.ToString(), type);
                 var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var o = constructor.Invoke(null);
                    var propertys = type.GetProperties();
                    var oldType = obj.GetType();

                    foreach (var property in propertys)
                    {
                        var p = oldType.GetProperty(property.Name);
                        if (property.CanWrite && p != null && p.CanRead)
                        {
                            property.SetValue(o, ChangeType(p.GetValue(obj, null), property.PropertyType), null);
                        }
                    }
                    return o;
                }
            }
            return obj;
        }
        /// <summary>
        /// 将 DateTimeOffset 转换成 DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
            else
                return dateTime.DateTime;
        }
        /// <summary>
        /// 将 DateTime 转换成 DateTimeOffset
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToDateTimeOffset(DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }
        public static object ChangeType2(object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }
    }
}
