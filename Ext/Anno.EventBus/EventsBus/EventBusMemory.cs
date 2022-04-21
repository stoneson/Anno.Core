using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Anno.EventBus
{
    /// <summary>
    /// 本地执行消息总线
    /// </summary>
    public class EventBusMemory : IEventBus
    {
        protected static object _lock = new object();

        protected static IEventBus _instance;

        protected static ConcurrentDictionary<Type, List<object>> _dicHandlers = new ConcurrentDictionary<Type, List<object>>();

        protected Func<object, object, bool> _Equals = (object o1, object o2) => o1.GetType() == o2.GetType();

        /// <summary>
        /// 实例
        /// </summary>
        public static IEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new EventBusMemory();
                        }
                    }
                }

                return _instance;
            }
        }

        public virtual Model.Enums.MQTypeEnum MQType => Model.Enums.MQTypeEnum.None;

        public ConsumeErrorNotice ErrorNotice { get; set; }

        public virtual void PublishAsync<T>(T eneity) where T : IEventData
        {
            Task.Run(delegate
            {
                Publish(eneity);
            });
        }

        public virtual void Publish<T>(T eneity) where T : IEventData
        {
            Type type = eneity.GetType();
            if (!_dicHandlers.ContainsKey(type) || _dicHandlers[type] == null)
            {
                return;
            }

            _dicHandlers[type].ForEach(delegate (object o)
            {
                IEventHandler<T> eventHandler = o as IEventHandler<T>;
                try
                {
                    eventHandler?.Handler(eneity);
                }
                catch (Exception exception)
                {
                    Type type2 = eventHandler.GetType();
                    this.ErrorNotice?.Invoke(type2.Namespace, type2.Name, exception, JsonConvert.SerializeObject(eneity));
                }
            });
        }

        public virtual void Subscribe(Type type, object data)
        {
            lock (_lock)
            {
                if (_dicHandlers.ContainsKey(type))
                {
                    List<object> list = _dicHandlers[type];
                    if (!list.Any((object o) => _Equals(o, data)))
                    {
                        list.Add(data);
                    }
                }
                else
                {
                    _dicHandlers[type] = new List<object>
                    {
                        data
                    };
                }
            }
        }

        public virtual void SubscribeAll(params Assembly[] assemblys)
        {
            if (assemblys == null || assemblys.Length == 0)
            {
                return;
            }
            var assemblies = new List<Assembly>(assemblys);

            EventBusSetting.Default.BusAssemblyName.ForEach(delegate (string assembleName)
            {
                Assembly item = Assembly.Load(new AssemblyName(assembleName));
                assemblies.Add(item);
            });
            assemblies.Distinct().ToList().ForEach(delegate (Assembly assembly)
            {
                (from x in assembly.GetTypes()
                 where x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface
                 select x).ToList().ForEach(delegate (Type x)
                 {
                     if (IsAssignableFrom(x, typeof(IEventHandler<>)))
                     {
                         object data = Activator.CreateInstance(x);
                         foreach (Type item2 in from i in x.GetInterfaces()
                                                where i.Name == "IEventHandler`1" && i.Namespace == typeof(IEventHandler<IEventData>).Namespace
                                                select i)
                         {
                             Type type = item2.GetGenericArguments().FirstOrDefault();
                             Subscribe(type, data);
                         }
                     }
                 });
            });
        }

        protected virtual bool IsAssignableFrom(Type type, Type abs)
        {
            if ((abs.GetTypeInfo().IsAbstract || abs.GetTypeInfo().IsInterface) && abs.IsAssignableFrom(type))
            {
                return true;
            }

            if (type.GetInterfaces().Any((Type o) => o.GetTypeInfo().IsGenericType && o.GetGenericTypeDefinition() == abs))
            {
                return true;
            }

            return false;
        }

        protected virtual void HandleEvent(string eventName, string message)
        {
            Type eventType = GetEventTypeByName(eventName);
            if (!(eventType != null))
            {
                return;
            }

            IEventData eneity = JsonConvert.DeserializeObject(message, eventType) as IEventData;
            if (_dicHandlers.ContainsKey(eventType) && _dicHandlers[eventType] != null)
            {
                _dicHandlers[eventType].ForEach(delegate (object o)
                {
                    typeof(IEventHandler<>).MakeGenericType(eventType)?.GetMethod("Handler")?.Invoke(o, new object[1]
                    {
                        eneity
                    });
                });
            }
        }

        protected virtual Type GetEventTypeByName(string eventName)
        {
            return _dicHandlers.Keys.FirstOrDefault((Type eh) => eh.Name == eventName);
        }
    }

}
