using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsoleTest.MqTest;
using Anno.EventBus;
using Anno.Loader;
using Autofac;

namespace ConsoleTest
{
    class RabbitMqTest
    {
        public void Handle()
        {
            //var builder = Anno.Loader.IocLoader.GetAutoFacContainerBuilder();

            //builder.RegisterType<EventBusMemory>().SingleInstance();
            //builder.RegisterType<EventBusRabbitMQ>().SingleInstance();
            //builder.RegisterType<EventBusKafka>().SingleInstance();
            //builder.RegisterType<EventBusActiveMQ>().SingleInstance();

            ////builder.Register<IEventBus>(p => p.Resolve<EventBusMemory>()).SingleInstance();
            //builder.Register<IEventBus>(p => p.Resolve<EventBusRabbitMQ>()).SingleInstance();
            ////builder.Register<IEventBus>(p => p.Resolve<EventBusKafka>()).SingleInstance();
            ////builder.Register<IEventBus>(p => p.Resolve<EventBusActiveMQ>()).SingleInstance();

            //var bus =Anno.Loader.IocLoader.Resolve<IEventBus>();

            var bus = Anno.EventBus.EventBusFactory.CreateEventBus();
            Console.WriteLine(bus.MQType);
            bus.SubscribeAll(typeof(RabbitMqTest).Assembly);
            Notice notice = new Notice()
            {
                Id = 1100,
                EventSource = this,
                Name = "杜燕明",
                Msg = "后天放假，祝节假日快乐！发送时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss FFFFF")
            };

            TT tt = new TT()
            {
                Id = 1100,
                EventSource = notice,
                Name = "TT杜燕明",
                Msg = "TT后天放假，祝节假日快乐！发送时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss FFFFF")
            };
            bus.Publish(notice);
            bus.Publish(tt);


        }
    }
}
