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
#if NETFRAMEWORK
    using Anno.Rpc.Client;
    public class AnnoWcfRpcInterceptor : IInterceptor
    {
        public string Channel { get; set; }
        public string Router { get; set; }

        private static Type _taskType = typeof(Task);
        private static Type rltObjectType = typeof(Anno.EngineData.ActionResult<>);
        public AnnoWcfRpcInterceptor(string channel, string router)
        {
            Channel = channel;
            Router = router;
        }
        public void Intercept(IInvocation invocation)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.AddOrUpdate(Const.Enum.Eng.NAMESPACE, Channel);
            input.AddOrUpdate(Const.Enum.Eng.CLASS, Router);
            input.AddOrUpdate(Const.Enum.Eng.METHOD, invocation.Method.Name);
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
                        try
                        {
                            var errorData = JsonConvert.DeserializeObject(rltStr, typeof(object)) as dynamic;
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
    }
#endif
}
