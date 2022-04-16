using Anno.Const.Attribute;
using Anno.EngineData;
using HelloWorldDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anno.Plugs.HelloWorldService
{
    public class HelloWorldTaskModule : BaseModule
    {
#if NET40
        [AnnoInfo(Desc = "世界你好啊 async Task<dynamic> SayHelloAsync")]
        public dynamic SayHelloAsync([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            dynamic rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return (rlt);
        }
        [AnnoInfo(Desc = "世界你好啊Task<dynamic> SayHello")]
        public dynamic SayHello([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return (rlt);
        }
         [AnnoInfo(Desc = "世界你好啊SayHello2")]
        public dynamic SayHello2([AnnoInfo(Desc = "人员信息")][FromBody] PersonDto person
            , [AnnoInfo(Desc = "人员信息2")] PersonDto person2)
        {
            return new { HelloWorldViperMsg = $"{person.Name}你好啊，今年{ person2.Age}岁了" };
        }
        [AnnoInfo(Desc = "世界你好啊SayHello3")]
        public dynamic SayHello3([AnnoInfo(Desc = "人员信息")] PersonDto person)
        {
            return new { HelloWorldViperMsg = $"{person.Name}你好啊，今年{ person.Age}岁了" };
        }

        [AnnoInfo(Desc = "" +
            "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
            "SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello")]
        public dynamic ApiDocLengthTest([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return (rlt);
        }
        [AnnoInfo(Desc = "Task<ActionResult> 返回类型测试")]
        public ActionResult TaskActionResult()
        {
            return new ActionResult(true, "outputData", null, "");
        }
#else
        [AnnoInfo(Desc = "世界你好啊 async Task<dynamic> SayHelloAsync")]
        public async Task<dynamic> SayHelloAsync([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            dynamic rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return await Task.FromResult(rlt);
        }
        [AnnoInfo(Desc = "世界你好啊Task<dynamic> SayHello")]
        public async Task<dynamic> SayHello([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return await Task.FromResult(rlt);
        }
        [AnnoInfo(Desc = "世界你好啊SayHello2")]
        public dynamic SayHello2([AnnoInfo(Desc = "人员信息")][FromBody] PersonDto person
            , [AnnoInfo(Desc = "人员信息2")] PersonDto person2)
        {
            return new { HelloWorldViperMsg = $"{person.Name}你好啊，今年{ person2.Age}岁了" };
        }
        [AnnoInfo(Desc = "世界你好啊SayHello3")]
        public dynamic SayHello3([AnnoInfo(Desc = "人员信息")] PersonDto person)
        {
            return new { HelloWorldViperMsg = $"{person.Name}你好啊，今年{ person.Age}岁了" };
        }

        [AnnoInfo(Desc = "" +
            "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
             "世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> " +
            "SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello世界你好啊Task<dynamic> SayHello")]
        public async Task<dynamic> ApiDocLengthTest([AnnoInfo(Desc = "称呼")] string name, [AnnoInfo(Desc = "年龄")] int age)
        {
            object rlt = new { HelloWorldViperMsg = $"{name}你好啊，今年{age}岁了" };

            return await Task.FromResult(rlt);
        }
        [AnnoInfo(Desc = "Task<ActionResult> 返回类型测试")]
        public  Task<ActionResult> TaskActionResult() {
            return Task.FromResult(new ActionResult(true,"outputData",null,""));
        }
#endif
    }
}
