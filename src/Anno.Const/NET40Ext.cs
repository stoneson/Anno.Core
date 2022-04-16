using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
#if NET40
namespace Anno
{
    /// <summary>
    /// 
    /// </summary>
    public static class NET40Ext
    {
        public static Type GetTypeInfo(this Type typeInfo)
        {
            return typeInfo;
        }
        //public static T GetCustomAttribute<T>(this System.Reflection.MemberInfo pi) where T : Attribute
        //{
        //    return (T)Attribute.GetCustomAttribute(pi, typeof(T));
        //}
        public static T GetCustomAttribute<T>(this System.Reflection.MemberInfo element) where T : Attribute
        {
            var atrs = element.GetCustomAttributes<T>();
            return atrs?.Count > 0 ? atrs[0] : default(T);
        }
        public static List<T> GetCustomAttributes<T>(this System.Reflection.MemberInfo element) where T : Attribute
        {
            var list = new List<T>();
            var objs = element.GetCustomAttributes(typeof(T), true);
            foreach (var o in objs)
            {
                if (o is T) list.Add(o as T);
            }
            return list;
        }
        //public static T GetCustomAttribute<T>(this System.Reflection.ParameterInfo pi) where T : Attribute
        //{
        //    return (T)Attribute.GetCustomAttribute(pi, typeof(T));
        //}
        public static T GetCustomAttribute<T>(this System.Reflection.ParameterInfo element) where T : Attribute
        {
            var atrs = element.GetCustomAttributes<T>();
            return atrs?.Count > 0 ? atrs[0] : default(T);
        }
        public static List<T> GetCustomAttributes<T>(this System.Reflection.ParameterInfo element) where T : Attribute
        {
            var list = new List<T>();
            var objs = element.GetCustomAttributes(typeof(T), true);
            foreach (var o in objs)
            {
                if (o is T) list.Add(o as T);
            }
            return list;
        }
        public static object GetValue(this System.Reflection.PropertyInfo element, object obj)
        {
            return element.GetValue(obj, null);
        }
       
    }
    public class TaskEx
    {

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static System.Threading.Tasks.Task Run(System.Action action)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
            new System.Threading.Thread(() =>
            {
                try
                {
                    action?.Invoke();
                    tcs.SetResult(null);
                }
                catch (System.Exception ex)
                {
                    tcs.SetException(ex);
                }
            })
            { IsBackground = true }.Start();
            return tcs.Task;
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            var tcs = new TaskCompletionSource<TResult>();
            new Thread(() =>
            {
                try
                {
                    tcs.SetResult(function());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            })
            { IsBackground = true }.Start();
            return tcs.Task;
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            var timer = new System.Timers.Timer(milliseconds) { AutoReset = false };
            timer.Elapsed += delegate { timer.Dispose(); tcs.SetResult(null); };
            timer.Start();
            return tcs.Task;
        }
    }
}
#endif
