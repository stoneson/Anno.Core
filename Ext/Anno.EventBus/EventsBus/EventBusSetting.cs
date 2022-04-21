using System.Collections.Generic;

namespace Anno.EventBus
{
    public class EventBusSetting
    {
        private static object _lock = new object();

        private static EventBusSetting eventBusSetting = null;


        public static EventBusSetting Default
        {
            get
            {
                if (eventBusSetting == null)
                {
                    lock (_lock)
                    {
                        if (eventBusSetting == null)
                        {
                            eventBusSetting = new EventBusSetting();
                        }
                    }
                }

                return eventBusSetting;
            }
        }

        public List<string> BusAssemblyName
        {
            get;
            private set;
        } = new List<string>();


        //private EventBusSetting()
        //{
        //    InitConst();
        //}

        //private void InitConst()
        //{
        //    string text = Path.Combine(Directory.GetCurrentDirectory(), "Anno.config");
        //    if (!File.Exists(text))
        //    {
        //        return;
        //    }

        //    XmlDocument xmlDocument = new XmlDocument();
        //    xmlDocument.Load(text);
        //    RabbitInit(xmlDocument);
        //    foreach (XmlNode item in xmlDocument.SelectNodes("//IocDll/Assembly")!)
        //    {
        //        BusAssemblyName.Add(item.InnerText);
        //    }
        //}

        //private void RabbitInit(XmlDocument doc)
        //{
        //    RabbitConfiguration = new RabbitConfiguration();
        //    RabbitConfiguration.HostName = RabbitMQGetValue("HostName", doc);
        //    RabbitConfiguration.UserName = RabbitMQGetValue("UserName", doc);
        //    RabbitConfiguration.Password = RabbitMQGetValue("Password", doc);
        //    RabbitConfiguration.VirtualHost = RabbitMQGetValue("VirtualHost", doc);
        //    int.TryParse(RabbitMQGetValue("Port", doc), out int result);
        //    if (result > 0)
        //    {
        //        RabbitConfiguration.Port = result;
        //    }
        //}

        //private static string RabbitMQGetValue(string key, XmlDocument doc)
        //{
        //    XmlNode xmlNode = doc.SelectSingleNode("//RabbitMQ[@key='RabbitMQ']");
        //    if (xmlNode != null)
        //    {
        //        return xmlNode.Attributes![key]!.Value;
        //    }

        //    return string.Empty;
        //}

        //private static string AppSetting(string key, XmlDocument doc)
        //{
        //    XmlNode xmlNode = doc.SelectSingleNode("//appSettings/add[@key='" + key + "']");
        //    if (xmlNode != null)
        //    {
        //        return xmlNode.Attributes!["value"]!.Value;
        //    }

        //    return string.Empty;
        //}
    }

}
