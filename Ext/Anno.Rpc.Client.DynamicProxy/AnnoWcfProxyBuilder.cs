﻿using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Thrift 动态代理
/// </summary>
namespace Anno.Rpc.Client.DynamicProxy
{
#if NETFRAMEWORK
    using Autofac;
    public static class AnnoWcfProxyBuilder
    {
        private static ProxyGenerator generator = new ProxyGenerator();
        //private static IInterceptor rpcInterceptor = new AnnoWcfRpcInterceptor("","");
        /// <summary>
        /// 返回Anno接口的动态实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface GetService<TInterface>(string channel, string router) where TInterface : class
        {
            return generator.CreateInterfaceProxyWithoutTarget<TInterface>(new AnnoWcfRpcInterceptor(channel, router));
        }
        /// <summary>
        /// 扫描程序集批量生成 Anno接口动态实例
        /// </summary>
        /// <param name="assembley">程序集</param>
        /// <returns></returns>
        public static Dictionary<Type, object> GetServices(params Assembly[] assembleys)
        {
            Dictionary<Type, object> annos = new Dictionary<Type, object>();
            if (assembleys.Length > 0)
            {
                for (int i = 0; i < assembleys.Length; i++)
                {
                    Assembly assembly = assembleys[i];
                    if (assembly != null)
                    {
                        assembly.GetTypes().Where(x => x.GetTypeInfo().IsInterface).ToList().ForEach(
                           t =>
                           {
                               if (t.GetCustomAttribute<System.ServiceModel.ServiceContractAttribute>() != null)
                               {
                                   string channel = t.Namespace;
                                   string router = t.Name;
                                   object instance = generator.CreateInterfaceProxyWithoutTarget(t, new AnnoWcfRpcInterceptor(channel, router));
                                   annos.Add(t, instance);
                               }

                           });
                    }
                }
            }
            return annos;
        }
        /// <summary>
        /// 扫描程序集Anno接口注入容器（Autofac.ContainerBuilder,Microsoft.Extensions.DependencyInjection.IServiceCollection）
        /// </summary>
        /// <param name="assembley">程序集</param>
        /// <param name="container">容器实例</param>
        public static void InjectAnnoContainer(this Assembly assembley, object container)
        {
            if (container != null && assembley != null)
            {
                var proxys = GetServices(assembley);
                if (container is ContainerBuilder)
                {
                    var _container = (ContainerBuilder)container;
                    foreach (var proxy in proxys)
                    {
                        _container.RegisterInstance(proxy.Value).As(proxy.Key);
                    }
                }
            }
        }
    }
#endif
}
