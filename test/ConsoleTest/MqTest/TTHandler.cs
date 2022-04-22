using System;
using System.Collections.Generic;
using System.Text;
using Anno.EventBus;

namespace ConsoleTest.MqTest
{
    public class TTSend : IEventHandler<TT>
    {
        public void Handler(TT entity)
        {
            Console.WriteLine($"TTSend {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss FFFFF")}:【你好{entity.Name},{entity.Msg}】");
        }
    }
    public class TTend : IEventHandler<TT>
    {
        public void Handler(TT entity)
        {
            //Console.WriteLine($"EventSource,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss FFFFF")}:【{entity.EventSource}】");
            Console.WriteLine($"TTend {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss FFFFF")}:【消息发送完毕！】");
        }
    }
}
