# 🏡Anno 分布式微服务开发框架

**Anno 是一个分布式开发框架,专注于服务治理、监控、链路追踪。RPC可选用高性能跨语言的Thrift（推荐）、Grpc。同时支持 .net core 、.net framework、java。**
[![downloads](https://img.shields.io/nuget/dt/Vive.Anno.EngineData.svg)](https://www.nuget.org/packages/Vive.Anno.EngineData)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/duyanming/Vive.Anno.Core/master/LICENSE)


[在线演示](http://49.232.112.70) :http://49.232.112.70

[示例项目](https://gitee.com/stoneson/Viper) :https://gitee.com/stoneson/Viper

## 🎄Nuget 基础

Package name                             |Description   | Version                     | Downloads
-----------------------------------------|--------------|-----------------------------|-------------------------
`Vive.Anno.Core`|核心库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Core.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Core/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Core.svg)

## 🎃Nuget 通信

Package name                       |Description       | Version                     | Downloads
-----------------------------------|------------------|-----------------------------|---------------------
`Vive.Anno.Rpc.Center`|Thrift注册中心库  | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.Center.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.Center/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.Center.svg)
`Vive.Anno.Rpc.Client` |Thrift客户端库| [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.Client.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.Client/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.Client.svg)
`Vive.Anno.Rpc.Server`|Thrift Server服务库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.Server.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.Server/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.Server.svg)
`Vive.Anno.Rpc.CenterGrpc`|Grpc注册中心库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.CenterGrpc.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.CenterGrpc/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.CenterGrpc.svg)
`Vive.Anno.Rpc.ClientGrpc`|Grpc客户端库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.ClientGrpc.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.ClientGrpc/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.ClientGrpc.svg)
`Vive.Anno.Rpc.ServerGrpc`|GrpcServer服务库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.ServerGrpc.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.ServerGrpc/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.ServerGrpc.svg)

## 🎁Nuget 扩展

Package name                           |Description     | Version                     | Downloads
---------------------------------------|----------------|-----------------------------|------------------------
`Vive.Anno.EventBus`|EventBus事件总线库 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.EventBus.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.EventBus/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.EventBus.svg)
`Vive.Anno.Plugs.MonitorService`|Anno服务监控中间件 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Plugs.MonitorService.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Plugs.MonitorService/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Plugs.MonitorService.svg)
`Vive.Anno.Rpc.Client.DynamicProxy`|接口代理Anno客户端扩展 | [![NuGet](https://img.shields.io/nuget/v/Vive.Anno.Rpc.Client.DynamicProxy.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Vive.Anno.Rpc.Client.DynamicProxy/) | ![downloads](https://img.shields.io/nuget/dt/Vive.Anno.Rpc.Client.DynamicProxy.svg)

## 🎂整体架构
![整体架构](https://s3.ax1x.com/2020/12/18/rtegcd.png)

整体架构主要分为三个部分

　　1、注册中心：AnnoCenter 

　　2、微服务：AnnoService（可以是多个服务例如：订单服务A、库存服务B、支付服务C、用户服务D）

　　3、ApiGateway：[参考Viper](https://gitee.com/stoneson/Viper)
　　
# 📢主要功能

　　服务注册中心、服务发现、健康检查、负载均衡、限流、失败重试、链路追踪、资源监控等功能


# 📦注册中心(AnnoCenter)

　　AnnoCenter 是一个服务注册中心，主要职责是 发现服务（例如订单服务A、库存服务B）、存储服务配置信息、健康检查、简单键值KV存储。客户端定时从注册中心获取服务信息缓存到本地。即便注册中心宕机也不影响整个集群运行，因为客户端已经缓存了整个集群的服务信息。但是新加入的服务无法注册进来，需要启动注册中心才可以。
　　客户端(例如：ApiGateway )发送过来请求时,客户端类库从本地缓存找出能够处理此请求的服务列表（这个过程可能涉及权重等策略）选择一个去处理请求，然后返回，如果失败会有重试机制。
　　注册中心会定时对每个服务做健康检查，如果连接不上服务则标记此服务为亚健康状态，此时不会将此服务踢出也不会将自服务返回给客户单使用，然后开始重复做检查。如果一分钟内恢复正常则重新标记为健康状态切可以对外提供服务，否则永久踢出服务。
    

    服务注册中心（AnnoCenter） 是整个集群第一个需要运行起来的程序。

配置文件：只需要配置端口、超时时间即可。服务节点信息会在服务注册进来的时候自动写入

```xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <!--#lbs 配置 Port 注册中心监听端口 TimeOut 超时时间毫秒
      dc：节点
      dc:nickname：服务名称 App001
      dc:name: 功能tag
      dc:ip:服务IP
      dc:port:服务端口
      dc:timeout:服务超时时间
      dc:weight:服务权重 数字
      -->
      <Port>6660</Port>
      <TimeOut>120000</TimeOut>
      <Servers>
        <dc name="Vive.Anno.Plugs.TraceService,Vive.Anno.Plugs.DLockService,Vive.Anno.Plugs.EsLogService" nickname="App001" ip="10.112.93.122" port="6659" timeout="20000" weight="1" />
      </Servers>
    </configuration>

```

# 👑服务(AnnoService)
服务宿主程序，本着约定大于配置的开发原则。
插件式开发具体参考：
Packages

    命名空间和程序集名称相同，格式为Vive.Anno.Plugs.XXXService 例如：Vive.Anno.Plugs.HelloWorldService

    初始化配置    
    实现接口：IPlugsConfigurationBootstrap

```cs
using Vive.Anno.EngineData;
using System;

namespace Vive.Anno.Plugs.HelloWorldService
{
    /// <summary>
    /// 插件启动引导器
    /// DependsOn 依赖的类型程序集自动注入DI容器
    /// </summary>
    [DependsOn(
        //typeof(Domain.Bootstrap)
        //, typeof(QueryServices.Bootstrap)
        //, typeof(Repository.Bootstrap)
        //, typeof(Command.Handler.Bootstrap
        )]
    public class HelloWorldBootStrap : IPlugsConfigurationBootstrap
    {
        /// <summary>
        /// Service 依赖注入构建之后调用
        /// </summary>
        public void ConfigurationBootstrap()
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Service 依赖注入构建之前调用
        /// </summary>
        /// </summary>
        public void PreConfigurationBootstrap()
        {
            //throw new NotImplementedException();
        }
    }
}

```

功能模块实现
继承: BaseModule

Module命名规范为 XXXModule，以Module结尾 例如：HelloWorldViperModule
```cs
/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/30 13:15:24 
Functional description： HelloWorldViperModule
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Vive.Anno.Plugs.HelloWorldService
{
    using Vive.Anno.Const.Attribute;
    using Vive.Anno.EngineData;
    using HelloWorldDto;
    using System.ComponentModel.DataAnnotations;

    public class HelloWorldViperModule : BaseModule
    {
        [AnnoInfo(Desc = "世界你好啊SayHi")]
        public dynamic SayHello([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("vname", name);
            input.Add("vage", age.ToString());
            var soEasyMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult<string>>(this.InvokeProcessor("Vive.Anno.Plugs.SoEasy", "AnnoSoEasy", "SayHi", input)).OutputData;
            return new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了", SoEasyMsg = soEasyMsg };
        }
    
        [AnnoInfo(Desc = "两个整数相减等于几？我来帮你算（x-y=?）")]
        public int Subtraction([AnnoInfo(Desc = "整数X")] int x, [AnnoInfo(Desc = "整数Y")] int y)
        {
            return x - y;
        }
        [AnnoInfo(Desc = "买个商品吧，双十一马上就来了")]
        public ProductDto BuyProduct([AnnoInfo(Desc = "商品名称")] string productName, [AnnoInfo(Desc = "商品数量")] int number)
        {
            double price = new Random().Next(2, 90);
            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add("productName", productName);
            input.Add("number", number.ToString());
            var product = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult<ProductDto>>(this.InvokeProcessor("Vive.Anno.Plugs.SoEasy", "AnnoSoEasy", "BuyProduct", input)).OutputData;
            product.CountryOfOrigin = $"中国北京中转--{ product.CountryOfOrigin}";
            return product;
        }       
    }
}

```

配置文件：
  ```xml

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <!--0,0 第一位是 工作站，第二位数据中心
      （所有的 AnnoService 的 两位数不能重复例如不能存在【1,2】【1,2】）
      可以存在【1,2】【2,1】
      -->
      <IdWorker>0,0</IdWorker>
      <!--App名称-->
      <AppName>App001</AppName>
      <!--监听端口-->
      <Port>6659</Port>
      <!--权重-->
      <Weight>1</Weight>
      <!--功能--> 
      <FuncName>Vive.Anno.Plugs.LogicService,Vive.Anno.Plugs.TraceService</FuncName>
      <!--忽略的功能 Trace,Logic-->
      <IgnoreFuncName></IgnoreFuncName>
      <!--超时时间毫秒-->
      <TimeOut>20000</TimeOut>
      <!--注册到的目标-->
      <Ts Ip="10.112.93.122" Port="6660"/>
      <IocDll>
        <!-- IOC 仓储、领域-->
        <Assembly>Vive.Anno.Repository</Assembly>
      </IocDll>
      <appSettings>
       
       </appSettings>
    </configuration>

  ```

# 🏄网关

[参考Viper](https://gitee.com/stoneson/Viper)
