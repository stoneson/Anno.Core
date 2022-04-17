using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Anno
{
	#region InvokeLocalHelper
	/// <summary>
	/// 
	/// </summary>
	public static class InvokeLocalHelper
	{
		/// <summary>
		/// ↓g全局变量，实例化类实体列表，反复使用的类，实例化后存放于此
		/// </summary>
		private static System.Collections.Concurrent.ConcurrentDictionary<string, EntityObject> g_DicEntityObject = new System.Collections.Concurrent.ConcurrentDictionary<string, EntityObject>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="dicParameters">参数集合</param>
		/// <returns>返回object</returns>
		public static object InvokeMethod(string psDLLName, string className, string methodName, IDictionary<string, object> dicParameters)
		{
			var parameters = GetParameters(psDLLName, className, methodName, null);
			object[] Args = CreateArgs(parameters, dicParameters);
			return InvokeMethod(psDLLName, className, methodName, Args);
		}

		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="entity">entity参数集合</param>
		/// <returns>返回object</returns>
		public static object InvokeMethodByEntity(string psDLLName, string className, string methodName, object entity)
		{
			var parameters = GetParameters(psDLLName, className, methodName, null);
			object[] Args = CreateArgs(parameters, entity);
			var ret = InvokeMethod(psDLLName, className, methodName, Args);
			return ret;
		}
		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="dicParameters">参数集合</param>
		/// <returns>返回object</returns>
		public static T InvokeMethod<T>(string psDLLName, string className, string methodName, IDictionary<string, object> dicParameters) //where T : class
		{
			//var parameters = GetParameters(psDLLName, className, methodName, null);
			//object[] Args = CreateArgs(parameters, dicParameters);
			return (T)InvokeMethod(psDLLName, className, methodName, dicParameters).ChangeType(typeof(T));
		}
		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="entity">entity参数集合</param>
		/// <returns>返回 T类型</returns>
		public static T InvokeMethodByEntity<T>(string psDLLName, string className, string methodName, object entity)// where T : class
		{
			return InvokeMethodByEntity(psDLLName, className, methodName, entity).ChangeType<T>();
		}
		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="Args">参数</param>
		/// <returns>返回 T</returns>
		public static T InvokeMethod<T>(string psDLLName, string className, string methodName, params object[] Args)// where T : class
		{
			return (T)InvokeMethod(psDLLName, className, methodName, Args).ChangeType(typeof(T));//as T;
		}
		/// <summary>
		/// 反射执行内部方法
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public static T InvokeMethod<T>(object obj, string methodName, params object[] args)// where T : class
		{
			if (obj == null) return default(T);
			return (T)InvokeMethod(obj, methodName, args).ChangeType(typeof(T));// as T;
		}
		/// <summary>
		/// 反射执行内部方法
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public static T InvokeMethod<T>(Type type, string methodName, params object[] args)// where T : class
		{
			if (type == null) return default(T);
			return (T)InvokeMethod(type, methodName, args).ChangeType(typeof(T));// as T;
		}
		/// <summary>
		/// 反射执行内部方法
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public static object InvokeMethod(object obj, string methodName, params object[] args)
		{
			if (obj == null) return null;
			return obj.FastInvoke(methodName, args);
		}

		/// <summary>
		/// 反射执行内部方法
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public static object InvokeMethod(Type type, string methodName, params object[] args)
		{
			if (type == null) return null;
			return type.FastInvoke(methodName, args);
		}

		/// <summary>
		/// 反射执行dll方法，并获取返回值
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="fullClassName">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名称</param>
		/// <param name="Args">参数</param>
		/// <returns>返回object</returns>
		public static object InvokeMethod(string psDLLName, string fullClassName, string methodName, params object[] Args)
		{
			return CreateInstance(psDLLName, fullClassName)?.DoMethod(psDLLName, fullClassName, methodName, Args);
		}
		/// <summary>
		/// 反射dll实例
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="fullClassName">类名（需要加命名空间）</param>
		/// <param name="Args">参数</param>
		/// <returns>返回object</returns>
		public static object NewInstance(string psDLLName, string fullClassName, params object[] Args)
		{
			return CreateInstance(psDLLName, fullClassName)?.DoFastInstance(psDLLName, fullClassName, Args);
		}
		/// <summary>
		/// 反射dll实例
		/// </summary>
		/// <param name="psDLLName">DLL名称</param>
		/// <param name="className">类名（需要加命名空间）</param>
		/// <param name="Args">参数</param>
		/// <returns>返回 T</returns>
		public static T NewInstance<T>(string psDLLName, string className, params object[] Args)// where T : class
		{
			return (T)NewInstance(psDLLName, className, Args).ChangeType(typeof(T));//as T;
		}
		/// <summary>
		/// 实例化插件类,放入域中和全局Object集合中
		/// </summary>
		/// <param name="psDLLName"></param>
		/// <param name="fullClassName"></param>
		/// <returns></returns>
		private static EntityObject CreateInstance(string psDLLName, string fullClassName, params object[] Args)
		{
			EntityObject _objentity;
			if (g_DicEntityObject.TryGetValue(fullClassName, out _objentity))
				if (_objentity != null) return _objentity;
			var ad = AppDomain.CurrentDomain;
			_objentity = new EntityObject();
			if (!_objentity.CreateInstance(psDLLName, fullClassName, Args)) return null;
			g_DicEntityObject[fullClassName] = _objentity;
			return _objentity;
		}

		public static object[] CreateArgs(ParameterInfo[] parameters, IDictionary<string, object> dicParameters)
		{
			object[] Args = new object[parameters != null ? parameters.Length : 0];
			if (Args.Length > 0)
			{
				for (var i = 0; i < Args.Length; i++)
				{
					var parameter = parameters[i];
					if (dicParameters.ContainsKey(parameter.Name))
					{
						Args[i] = dicParameters[parameter.Name].ChangeType(parameter.ParameterType);
					}
				}
			}
			return Args;
		}
		public static object[] CreateArgs(ParameterInfo[] parameters, params object[] args)
		{
			object[] Args = new object[parameters != null ? parameters.Length : 0];
			if (Args.Length > 0 && args?.Length > 0)
			{
				for (var i = 0; i < Args.Length && i < args.Length; i++)
				{
					Args[i] = args[i].ChangeType(parameters[i].ParameterType);
				}
			}
			return Args;
		}
		public static object[] CreateArgs(ParameterInfo[] parameters, object input)
		{
			object[] Args = new object[parameters != null ? parameters.Length : 0];
			if (Args.Length > 0 && input != null)
			{
				if (parameters?.Length == 1 && parameters[0].ParameterType.IsClass)
				{
					Args[0] = input.ChangeType(parameters[0].ParameterType);
				}
				else
				{
					if (input is Newtonsoft.Json.Linq.JObject)//JSON
					{
						foreach (var item in input as Newtonsoft.Json.Linq.JObject)
						{
							var parameter = parameters.ToList().Find(f => f.Name.ToLower() == item.Key.ToLower());
							if (parameter != null)
							{
								var index = parameters.ToList().IndexOf(parameter);
								if (index < 0) continue;
								var val = item.Value;// as Newtonsoft.Json.Linq.JValue;
								Args[index] = val != null ? val.ChangeType(parameter.ParameterType) : null;
							}
						}
					}
					else//实体
					{
						for (var i = 0; i < Args.Length; i++)
						{
							var parameter = parameters[i];
							Args[i] = input.GetPropertyValue(parameter.Name).ChangeType(parameter.ParameterType);
						}
					}
				}
			}
			return Args;
		}
		public static object[] CreateArgs(string psDLLName, string fullClassName, string methodName, object[] Args)
		{
			return CreateArgs(GetParameters(psDLLName, fullClassName, methodName, Args), Args);
		}
		/// <summary>
		/// 反射获取指定方法参数信息
		/// </summary>
		/// <param name="psDLLName">dll名称,或相对路径加文件名</param>
		/// <param name="fullClassName">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名</param>
		/// <param name="Args">参数</param>
		/// <returns>object</returns>
		private static ParameterInfo[] GetParameters(string psDLLName, string fullClassName, string methodName, object[] Args)
		{
			return CreateInstance(psDLLName, fullClassName)?.GetParameters(psDLLName, fullClassName, methodName, Args);
		}

		/// <summary>
		/// 卸载已装载到内存的dll
		/// </summary>
		/// <param name="_objentity"></param>
		/// <returns></returns>
		public static bool UnLoadDll(string fullClassName, bool IsCollect = true)
		{
			EntityObject _objentity;

			if (g_DicEntityObject.TryGetValue(fullClassName, out _objentity))
			{
				if (_objentity != null)
				{
					if (_objentity.Domain == null) { return false; }
					AppDomain.Unload(_objentity.Domain);
					g_DicEntityObject.TryRemove(fullClassName, out _objentity);
					if (IsCollect) { GC.Collect(); }
				}
			}
			return true;
		}

		/// <summary>
		/// 卸载已装载到内存的dll集合
		/// </summary>
		/// <param name="_objentity"></param>
		/// <returns></returns>
		public static void UnLoadDllList(List<string> _fullClassNamelist)
		{
			_fullClassNamelist.ForEach(p => UnLoadDll(p, false));
			GC.Collect();
		}


		/// <summary>
		/// 将一个对象转换为指定类型
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static T ChangeType<T>(this object obj)
		{
			return (T)ChangeType(obj, typeof(T));
		}

		/// <summary>
		/// 将一个对象转换为指定类型
		/// </summary>
		/// <param name="obj">待转换的对象</param>
		/// <param name="type">目标类型</param>
		/// <returns>转换后的对象</returns>
		public static object ChangeType(this object obj, Type type)
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

				if (obj is string) obj = Newtonsoft.Json.JsonConvert.DeserializeObject(obj.ToString(), type);
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
		public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
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
		public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
		{
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
		}
		public static object ChangeType2(this object value, Type type)
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
		#region GetPropertyValue/SetPropertyValue
		/// <summary>
		/// 获取对象属性值
		/// </summary>
		/// <param name="from"></param>
		/// <param name="_to"></param>
		public static object GetPropertyValue(this Object from, string _propertyName)
		{
			try
			{
				if (from == null || string.IsNullOrEmpty(_propertyName))
					return null;

				Type ctype = from.GetType();
				System.Reflection.PropertyInfo[] pis = ctype.GetProperties();
				foreach (System.Reflection.PropertyInfo cpi in pis)
				{
					try
					{
						if (_propertyName.Trim().ToLower() == cpi.Name.Trim().ToLower())
							return cpi.GetValue(from, null);
					}
					catch (Exception ex)
					{
						//LogUtil.Error("GetPropertyValue1," + ex.Message);
					}
				}
				//---------------------------------------------------------------------
				System.Reflection.FieldInfo[] fis = ctype.GetFields();
				foreach (System.Reflection.FieldInfo cfi in fis)
				{
					try
					{
						if (_propertyName.Trim().ToLower() == cfi.Name.Trim().ToLower())
							return cfi.GetValue(from);
					}
					catch (Exception ex)
					{
						//LogUtil.Error("GetPropertyValue2," + ex.Message);
					}
				}
			}
			catch (Exception ex) {// LogUtil.Error("GetPropertyValue," + ex.ToString());
			}
			return null;
		}

		/// <summary>
		/// 用索引化属性的可选索引值设置指定对象的该属性值  xjh 2016 8 3
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="propertyName"></param>
		/// <param name="val"></param>
		public static void SetPropertyValue(this object obj, string propertyName, object val)
		{
			if (obj == null || val == null || val == DBNull.Value || string.IsNullOrEmpty(val.ToString()) || string.IsNullOrEmpty(propertyName))
				return;
			try
			{
				System.Reflection.PropertyInfo property = obj.GetType().GetProperty(propertyName);//得到第i列的所有属性
				if (property != null)
				{
					if (property.CanWrite)//判断对象是否为空，属性是否为空，属性是否可写！如果都为true
					{
						try
						{
							//if(!(property.PropertyType.Equals(typeof(string) || property.PropertyType.Equals(typeof(DateTime)) && .val
							if (val.GetType().Equals(property.PropertyType))
								property.SetValue(obj, val, null);//o对象，reader[i]对象的新值，索引器空的
							else
								property.SetValue(obj, Convert.ChangeType(val, property.PropertyType, null), null);
						}
						catch (Exception ex)
						{
							//LogUtil.Error("SetPropertyValue1," + ex.Message);
						}
					}
				}
				else
				{
					System.Reflection.FieldInfo field = obj.GetType().GetField(propertyName);
					if (field != null)
					{
						try
						{
							if (val.GetType().Equals(field.FieldType))
								field.SetValue(obj, val);
							else
								field.SetValue(obj, Convert.ChangeType(val, field.FieldType, null));
						}
						catch (Exception ex)
						{
							//LogUtil.Error("SetPropertyValue2," + ex.Message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				//LogUtil.Error("SetPropertyValue," + ex.Message);
			}
		}
		/// <summary>
		/// 判断是否不为Null或者空
		/// </summary>
		/// <param name="obj">对象</param>
		/// <returns></returns>
		public static bool IsNotNullOrEmpty_(this object obj)
		{
			return !obj.IsNullOrEmpty_();
		}
		/// <summary>
		/// 判断是否不为Null或者空
		/// </summary>
		/// <param name="obj">对象</param>
		/// <returns></returns>
		public static bool IsNotNullOrEmpty_(this string obj)
		{
			return !obj.IsNullOrEmpty_();
		}
		/// <summary>验证字符串是否为 null 或为空</summary>
		/// <param name="source">要验证的字符串</param>
		/// <returns>指示字符串是否为 null 或为空</returns>
		public static bool IsNullOrEmpty_(this string source)
		{
			return string.IsNullOrEmpty(source) || null == source;
		}
		/// <summary>验证对象是否为 null 或数据库字段 null 值</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="value">要验证的对象</param>
		/// <returns>指示对象是否为 null 或数据库字段 null 值</returns>
		public static bool IsNullOrEmpty_<T>(this T value)
		{
			if (value == null || Convert.IsDBNull(value))
			{
				return true;
			}
			else if (value is T[])
			{
				return (value as T[]).IsNullOrEmptyArr_();
			}
			else if (value is System.Collections.IEnumerable)
			{
				return (value as System.Collections.IEnumerable).IsNullOrEmptyArr_();
			}
			else if (value is IEnumerable<T>)
			{
				return (value as IEnumerable<T>).IsNullOrEmptyArr_();
			}
			else
			{
				string objStr = value.ToString();
				return string.IsNullOrEmpty(objStr);
			}
		}
		/// <summary>验证 GUID 是否为 null 或为空</summary>
		/// <param name="guid">要验证的 GUID</param>
		/// <returns>指示 GUID 是否为 null 或为空</returns>
		public static bool IsNullOrEmpty_(this Guid guid)
		{
			return guid == null || guid == Guid.Empty;
		}
		/// <summary>验证数组是否为 null 或为空</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="source">要验证的数组</param>
		/// <returns>指示数组是否为 null 或为空</returns>
		public static bool IsNullOrEmptyArr_<T>(this T[] source)
		{
			return null == source || source.Length == 0;
		}
		/// <summary>验证集合是否为 null 或为空</summary>
		/// <param name="source">要验证的集合</param>
		/// <returns>指示集合是否为 null 或为空</returns>
		public static bool IsNullOrEmptyArr_(this System.Collections.IEnumerable source)
		{
			if (null == source) return true;
			var _source = source as System.Collections.ICollection;
			if (null != _source) return _source.Count == 0;
			foreach (var s in source) return false;
			return false;
		}

		/// <summary>验证集合是否为 null 或为空</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="source">要验证的集合</param>
		/// <returns>指示集合是否为 null 或为空</returns>
		public static bool IsNullOrEmptyArr_<T>(this IEnumerable<T> source)
		{
			if (null == source) return true;

			var _source = source as ICollection<T>;
			if (null != _source) return _source.Count == 0;
			var _source2 = source as System.Collections.ICollection;
			if (null != _source2) return _source2.Count == 0;
			foreach (T _item in source) return false;
			return true;
		}
		/// <summary>验证是否为虚属性</summary>
		/// <param name="property">属性信息</param>
		/// <returns>指示是否为虚属性</returns>
		public static bool IsVirtual(this PropertyInfo property)
		{
			var get = property?.GetGetMethod();
			if (get != null && get.IsVirtual) return true;
			var set = property?.GetSetMethod();
			if (set != null && set.IsVirtual) return true;
			return false;
		}
		/// <summary>
		/// 支持可以赋值为null的值类型
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public static bool IsNullable(this PropertyInfo propertyInfo)
		{
			Type unType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
			return unType != null;
		}
		/// <summary>
		/// 支持可以赋值为null的值类型
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsNullable(this Type type)
		{
			Type unType = Nullable.GetUnderlyingType(type);
			return unType != null;
		}
		/// <summary>验证是否为可 null 类型</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="value">要验证的对象</param>
		/// <returns>指示是否为可 null 类型</returns>
		public static bool IsNullable<T>(this T value)
		{
			return value?.GetType().IsNullable() == true;
		}

		public static bool IsClass(this Type thisValue)
		{
			return thisValue != typeof(string) && thisValue.IsClass;
		}

		/// <summary>验证是否为结构数据</summary>
		/// <param name="type">要验证的类型</param>
		/// <returns>指示是否为结构数据</returns>
		public static bool IsStruct(this Type type)
		{
			return ((type.IsValueType && !type.IsEnum) && (!type.IsPrimitive && !type.IsSerializable));
		}

		/// <summary>验证是否为结构数据</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="value">要验证的对象</param>
		/// <returns>指示是否为结构数据</returns>
		public static bool IsStruct<T>(this T value)
		{
			return typeof(T).IsStruct();
		}

		public static T IsNullReturnNew<T>(T returnObj) where T : new()
		{
			if (returnObj.IsNullOrEmpty_())
			{
				returnObj = FastReflection.FastInstance<T>();
			}
			return returnObj;
		}
		#endregion

		#region AssemblyLoad
		private static Dictionary<string, Assembly> m_dicAAssemblys = new Dictionary<string, Assembly>();
		/// <summary>
		/// 通过给定程序集的长格式名称加载程序集
		/// </summary>
		/// <param name="assemblyString">程序集名称的长格式</param>
		/// <param name="filePath">程序集所在目录，默认空，为当前目录</param>
		/// <returns></returns>
		public static Assembly AssemblyLoad(string assemblyString, string filePath = "")
		{
			try
			{
				Assembly assembly = null;
				Type type = null;

				if (m_dicAAssemblys.ContainsKey(assemblyString))
					return m_dicAAssemblys[assemblyString];
				//------------------------------------------------------------------
				try
				{
					type = Type.GetType(assemblyString);
				}
				catch { }
				try
				{
					if (type != null)
						assembly = Assembly.GetAssembly(type);
				}
				catch { }
				if (assembly != null)
				{
					m_dicAAssemblys[assemblyString] = assembly;
					return assembly;
				}
				//------------------------------------------------------------------
				try
				{
					assembly = Assembly.Load(assemblyString);
					if (assembly != null)
					{
						m_dicAAssemblys[assemblyString] = assembly;
						return assembly;
					}
				}
				catch { }

				if (filePath.IsNullOrEmpty_())
					filePath = AppDomain.CurrentDomain.BaseDirectory;

				if (!filePath.IsNullOrEmpty_() && !assemblyString.IsNullOrEmpty_())
				{
					var file = filePath;
					if (!System.IO.File.Exists(file))
					{
						var astr = assemblyString;
						file = System.IO.Path.Combine(filePath, astr + ".dll");
						while (true)
						{
							if (System.IO.File.Exists(file))
								break;
							if (astr.IndexOf('.') < 0)
								return null;
							astr = astr.Substring(0, astr.LastIndexOf('.'));
							file = System.IO.Path.Combine(filePath, astr + ".dll");
						}
					}
					//-------------------------------------------------------------------
					if (System.IO.File.Exists(file))
					{
						if (m_dicAAssemblys.ContainsKey(file))
							return m_dicAAssemblys[file];
						//LoggingHelper.Debug("..1111." + file);
						m_dicAAssemblys[file] = Assembly.LoadFrom(file);
						return m_dicAAssemblys[file];
						//using (System.IO.FileStream FS = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
						//{
						//    byte[] FileByteArray = new byte[(int)FS.Length];
						//    FS.Read(FileByteArray, 0, FileByteArray.Length);

						//    m_dicAAssemblys[file] = Assembly.Load(FileByteArray);
						//    return m_dicAAssemblys[file];
						//}
					}
					else
					{
						//NlogHelper.Error("filePath=" + filePath + ",assemblyString=" + assemblyString + ",不存在@AssemblyLoad"); return null;
					}
				}
				else
				{
					EntityObject.Log("filePath或assemblyString不能这空@AssemblyLoad");
				}
			}
			catch (Exception ex)
			{
				EntityObject.Log(ex.Message + "@AssemblyLoad");
			}
			return null;
		}
		///// <summary>
		///// 通过给定程序集的长格式名称加载程序集
		///// </summary>
		///// <param name="DllfilePath">程序集名称的长格式</param>
		///// <returns></returns>
		//public static Assembly AssemblyLoad(string DllfilePath)
		//{
		//    try
		//    {
		//        if (!string.IsNullOrEmpty_(DllfilePath))
		//        {
		//            if (System.IO.File.Exists(DllfilePath))
		//            {
		//                string filePath = System.IO.Path.GetDirectoryName(DllfilePath);
		//                filePath = string.IsNullOrEmpty_(filePath) ? PubFun.BaseDirectory : filePath;
		//                string assemblyString = System.IO.Path.GetFileNameWithoutExtension(DllfilePath);
		//                return AssemblyLoad(filePath, assemblyString);
		//            }
		//            else
		//            {
		//                LoggingHelper.Info("DllfilePath=" + DllfilePath + ",不存在@AssemblyLoad");
		//                return null;
		//            }
		//        }
		//        else
		//        {
		//            LoggingHelper.Info("DllfilePath不能这空@AssemblyLoad");
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        LoggingHelper.Error(ex.Message + "@AssemblyLoad");
		//    }
		//    return null;
		//}
		#endregion
	}
	#region EntityObject
	/// <summary>
	/// 加载dll,调用内部方法实现类
	/// </summary>
	internal class EntityObject : MarshalByRefObject
	{
		public AppDomain Domain { get; set; }
		private Assembly assembly { get; set; }
		private Object obj { get; set; }
		private Type type { get; set; }
		public string FullClassName { get; set; }

		/// <summary>
		/// 是否已经实例化
		/// </summary>
		public bool IsInit { get { return type != null && obj != null; } }

		/// <summary>
		/// 实例化插件类
		/// </summary>
		/// <param name="dllPath"></param>
		/// <param name="FullClassName"></param>
		/// <returns></returns>
		public virtual bool CreateInstance(string dllPath, string FullClassName, params object[] args)
		{
			this.FullClassName = FullClassName;
			try
			{
				if (type == null)
				{
					var ab = Assembly.GetEntryAssembly();
					type = Type.GetType(FullClassName, true, true);
					if (type == null)
						type = ab.GetType(FullClassName, true, true);
					if (type != null)
					{
						assembly = ab;
					}
				}
			}
			catch (Exception ex)
			{
				Log("插件：" + dllPath + FullClassName + "初始化失败！\r\t" + ex);
			}
			try
			{
				if (assembly == null)
					assembly = InvokeLocalHelper.AssemblyLoad(FullClassName, dllPath);
				if (type == null && assembly != null)
					type = assembly.GetType(FullClassName, true, true);
				if (obj == null && type != null)
					obj = FastReflection.FastInstance(type, args);// Activator.CreateInstance(type);

				return IsInit;
			}
			catch (Exception ex)
			{
				Log("插件：" + dllPath + FullClassName + "初始化失败！\r\t" + ex);
				return false;
			}
		}

		/// <summary>
		/// 反射执行dll内部方法
		/// </summary>
		/// <param name="dllPath">dll名称,或相对路径加文件名</param>
		/// <param name="FullClassName">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public virtual object DoMethod(string dllPath, string FullClassName, string methodName, object[] args)
		{
			if (!CreateInstance(dllPath, FullClassName)) return null;
			args = InvokeLocalHelper.CreateArgs(GetParameters(methodName, args), args);
			return DoMethod(methodName, args);
		}
		public virtual object DoFastInstance(string dllPath, string FullClassName, object[] args)
		{
			if (!CreateInstance(dllPath, FullClassName, args)) return null;
			return obj;
		}
		/// <summary>
		/// 反射执行dll内部方法
		/// </summary>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public virtual object DoMethod(string methodName, object[] args)
		{
			if (!IsInit) return null;
			return type.FastInvoke(methodName, args);
		}
		/// <summary>
		/// 反射获取指定方法参数信息
		/// </summary>
		/// <param name="dllPath">dll名称,或相对路径加文件名</param>
		/// <param name="FullClassName">类名（需要加命名空间）</param>
		/// <param name="methodName">方法名</param>
		/// <param name="args">参数</param>
		/// <returns>object</returns>
		public virtual ParameterInfo[] GetParameters(string dllPath, string FullClassName, string methodName, object[] args)
		{
			if (!CreateInstance(dllPath, FullClassName)) return null;
			return GetParameters(methodName, args);
		}
		/// <summary>
		/// 反射获取指定方法参数信息
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public virtual ParameterInfo[] GetParameters(string methodName, object[] args)
		{
			if (!IsInit) return null;
			var _methodInfof = type.GetMethodInfo(methodName, args); //type.GetMethod(methodName);//
			if (_methodInfof != null) return _methodInfof.GetParameters();

			return null;
		}

		#region//日志
		/// <summary>
		/// 记录本地插件日志
		/// </summary>
		/// <param name="Text"></param>
		public static void Log(string Text)
		{
			//NlogHelper.Error(Text);
			//string LogFold = LogNet.BaseDirectory + @"\Log\PluginLog";
			//string gLogFileName = LogFold + @"\" + "PluginLog_" + DateTime.Today.ToString("yyyyMMdd") + ".Log";
			//InitLogFile(gLogFileName);
			//if (gLogFileName.Trim() != "")
			//{
			//    System.IO.StreamWriter sw = System.IO.File.AppendText(gLogFileName);
			//    Text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + Text;
			//    sw.WriteLine(Text);
			//    sw.Close();
			//}
		}
		///// <summary>
		///// 初始化日志文件,当前目录下新建文件夹 \HisCALog,下面按日期建日志
		///// </summary>
		//private static void InitLogFile(string gLogFileNameNext)
		//{
		//    string LogFold = LogNet.BaseDirectory + @"\Log\5\PluginLog";

		//    try
		//    {
		//        //检查路径是否存在,若不存在则创6建
		//        if (System.IO.Directory.Exists(LogFold) == false)
		//            System.IO.Directory.CreateDirectory(LogFold);

		//        //检查当天的日志文件是否存在,若不存在则创建
		//        if (System.IO.File.Exists(gLogFileNameNext) == false)
		//            System.IO.File.Delete(gLogFileNameNext);

		//        //日志文件名称 HisCALog_20060908
		//        string gLogFileName = LogFold + @"\" + "PluginLog_" + DateTime.Today.ToString("yyyyMMdd") + ".Log";

		//        //检查当天的日志文件是否存在,若不存在则创建
		//        if (System.IO.File.Exists(gLogFileName) == false)
		//        {
		//            System.IO.File.Create(gLogFileName).Close();
		//            Log("*****" + " Start Loging HisCA Info "
		//                + DateTime.Now.ToString("yyyy-MM-dd HH:mm:fff") + "***");
		//        }
		//    }
		//    catch 
		//    {
		//    }
		//}
		#endregion

		/// <summary>
		/// 获取dll类型
		/// </summary>
		/// <returns></returns>
		public virtual string GetDllType()
		{
			if (obj == null) { return null; }
			return type.InvokeMember("GetType", BindingFlags.InvokeMethod, null, obj, null).ToString();
		}

		/// <summary>
		/// 无限生存期
		/// </summary>
		/// <returns></returns>
		public override object InitializeLifetimeService()
		{
			return null; //Remoting对象 无限生存期
		}

	}
	#endregion
	#endregion

	#region FastReflection
	#region class Dict<TKey, TValue> 
	public class Dict<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
	{
		/// <summary>初始化一个新 <see cref="Dict{TKey, TValue}"/> 实例。</summary>
		public Dict() : base()
		{
		}
		/// <summary>初始化一个新 <see cref="Dict{TKey, TValue}"/> 实例。</summary>
		/// <param name="capacity">可包含的初始元素数</param>
		public Dict(int capacity) : base(1, capacity)
		{
		}

		/// <summary>初始化一个新 <see cref="Dict{TKey, TValue}"/> 实例。</summary>
		/// <param name="comparer">比较键时要使用的比较器</param>
		public Dict(IEqualityComparer<TKey> comparer) : base(comparer)
		{
		}

		/// <summary>初始化一个新 <see cref="Dict{TKey, TValue}"/> 实例。</summary>
		/// <param name="capacity">可包含的初始元素数</param>
		/// <param name="comparer">比较键时要使用的比较器</param>
		public Dict(int capacity, IEqualityComparer<TKey> comparer) : base(1, capacity, comparer)
		{
		}
		/// <summary>获取值</summary>
		/// <param name="key">键名</param>
		/// <param name="func">用于在值不存在时添加值的委托</param>
		/// <returns>值</returns>
		public TValue Get(TKey key, Func<TValue> func)
		{
			TValue val;
			if (base.TryGetValue(key, out val))
				return val;

			val = func();
			if (!val.IsNullOrEmpty_<TValue>())
				base[key] = val;
			return val;
		}

		/// <summary>移除指定键的值</summary>
		/// <param name="key">要移除的键</param>
		/// <returns>返回是否移除成功</returns>
		public bool Del(TKey key)
		{
			TValue val;

			return base.TryRemove(key, out val);
		}

	}
	/// <summary>访问器类型</summary>
	public enum AccessorType
	{
		/// <summary>属性</summary>
		Property,
		/// <summary>字段</summary>
		Field
	}
	/// <summary>字段/属性访问器</summary>
	public class Accessor
	{
		/// <summary>访问器名称</summary>
		public string Name;
		/// <summary>成员信息</summary>
		public MemberInfo Member;
		/// <summary>访问器数据类型</summary>
		public Type DataType;
		/// <summary>访问器类型</summary>
		public AccessorType AccessorType;
		/// <summary>是否为可读属性</summary>
		public bool CanRade;
		/// <summary>是否为可写属性</summary>
		public bool CanWrite;
		/// <summary>是否为虚属性</summary>
		public bool IsVirtual;

		/// <summary>设置访问器的值</summary>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public void SetValue(object instance, object value)
		{
			switch (this.AccessorType)
			{
				case AccessorType.Property:
					((PropertyInfo)this.Member).FastSetValue(instance, value);
					break;
				case AccessorType.Field:
					((FieldInfo)this.Member).FastSetValue(instance, value);
					break;
			}
		}

		/// <summary>获取访问器的值</summary>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>访问器的值</returns>
		public object GetValue(object instance)
		{
			object val = null;
			switch (this.AccessorType)
			{
				case AccessorType.Property:
					val = ((PropertyInfo)this.Member).FastGetValue(instance);
					break;
				case AccessorType.Field:
					val = ((FieldInfo)this.Member).FastGetValue(instance);
					break;
			}
			return val;
		}

		/// <summary>获取访问器的值</summary>
		/// <typeparam name="T">值的数据类型</typeparam>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>访问器的值</returns>
		public T GetValue<T>(object instance)
		{
			return (T)this.GetValue(instance);
		}
	}
	#endregion
	/// <summary>
	/// 快速反射类
	/// </summary>
	public static class FastReflection
	{
		#region 创建实例
		private static Dict<ConstructorInfo, Func<object[], object>> OC;

		/// <summary>通过类的构造器信息创建对象实例</summary>
		/// <param name="constructorInfo">构造器信息</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static object FastInstance(this ConstructorInfo constructorInfo, params object[] parameters)
		{
			if (OC == null) OC = new Dict<ConstructorInfo, Func<object[], object>>();

			var exec = OC.Get(constructorInfo, () =>
			{
				var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");
				var parameterExpressions = new List<Expression>();
				var paramInfos = constructorInfo.GetParameters();

				for (int i = 0, l = paramInfos.Length; i < l; i++)
				{
					var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
					var valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

					parameterExpressions.Add(valueCast);
				}

				var instanceCreate = Expression.New(constructorInfo, parameterExpressions);
				var instanceCreateCast = Expression.Convert(instanceCreate, typeof(object));
				var lambda = Expression.Lambda<Func<object[], object>>(instanceCreateCast, parametersParameter);

				return lambda.Compile();
			});
			return exec(parameters);
		}

		/// <summary>通过类的构造器信息创建对象实例</summary>
		/// <typeparam name="T">返回对象类型</typeparam>
		/// <param name="constructorInfo">构造器信息</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static T FastInstance<T>(this ConstructorInfo constructorInfo, params object[] parameters)
		{
			return (T)constructorInfo.FastInstance(parameters);
		}

		/// <summary>通过对象类型创建对象实例</summary>
		/// <param name="type">对象类型</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static object FastInstance(this Type type, params object[] parameters)
		{
			//var paramLen = parameters.Length;
			//var types = new Type[paramLen];
			//for(int i = 0; i < paramLen; i++)
			//	types[i] = parameters[i].GetType();
			var consInfo = GetConstructor(type, parameters);// type.GetConstructor(types);
			return consInfo.FastInstance(parameters);
		}

		/// <summary>通过对象类型创建对象实例</summary>
		/// <typeparam name="T">返回对象类型</typeparam>
		/// <param name="type">对象类型</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static T FastInstance<T>(this Type type, params object[] parameters)
		{
			return (T)type.FastInstance(parameters);
		}

		/// <summary>创建对象实例</summary>
		/// <typeparam name="T">返回对象类型</typeparam>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static T FastInstance<T>(params object[] parameters)
		{
			return FastInstance<T>(typeof(T), parameters);
		}

		/// <summary>通过对象类型名称创建对象实例</summary>
		/// <param name="typeName">对象类型名称，格式为“类型名, 程序集名”，如：NetRube.Plugin.Mail, NetRube.Plugin</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static object FastInstance(this string typeName, params object[] parameters)
		{
			return FastInstance(FastGetType(typeName), parameters);
		}

		/// <summary>通过对象类型名称创建对象实例</summary>
		/// <typeparam name="T">返回对象类型</typeparam>
		/// <param name="typeName">对象类型名称，格式为“类型名, 程序集名”，如：NetRube.Plugin.Mail, NetRube.Plugin</param>
		/// <param name="parameters">参数</param>
		/// <returns>新对象实例</returns>
		public static T FastInstance<T>(string typeName, params object[] parameters)
		{
			return (T)FastInstance(typeName, parameters);
		}
		#endregion

		#region 方法
		private static Dict<MethodInfo, Func<object, object[], object>> MC;

		/// <summary>快速调用对象实例的方法</summary>
		/// <param name="methodInfo">要调用的方法</param>
		/// <param name="instance">对象实例</param>
		/// <param name="parameters">参数</param>
		/// <returns>方法执行的结果</returns>
		public static object FastInvoke(this MethodInfo methodInfo, object instance, params object[] parameters)
		{
			if (methodInfo == null) return null;

			if (MC == null) MC = new Dict<MethodInfo, Func<object, object[], object>>();
			var exec = MC.Get(methodInfo, () =>
			{
				var objectType = typeof(object);
				var objectsType = typeof(object[]);

				var instanceParameter = Expression.Parameter(objectType, "instance");
				var parametersParameter = Expression.Parameter(objectsType, "parameters");

				var parameterExpressions = new List<Expression>();
				var paramInfos = methodInfo.GetParameters();
				for (int i = 0, l = paramInfos.Length; i < l; i++)
				{
					var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
					var valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

					parameterExpressions.Add(valueCast);
				}

				var instanceCast = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.ReflectedType);
				var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

				if (methodCall.Type == typeof(void))
				{
					var lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);

					var act = lambda.Compile();
					return (inst, para) =>
					{
						act(inst, para);
						return null;
					};
				}
				else
				{
					var castMethodCall = Expression.Convert(methodCall, objectType);
					var lambda = Expression.Lambda<Func<object, object[], object>>(
						castMethodCall, instanceParameter, parametersParameter);

					return lambda.Compile();
				}
			});
			return exec(instance, parameters);
		}

		/// <summary>通过方法名称快速调用对象实例的方法</summary>
		/// <param name="methodName">方法名称</param>
		/// <param name="instance">对象实例</param>
		/// <param name="parameters">参数</param>
		/// <returns>方法执行的结果</returns>
		public static object FastInvoke(this object instance, string methodName, params object[] parameters)
		{
			if (instance == null)
				throw new Exception("执行对象 instance 不能为NULL");
			if (methodName.IsNullOrEmpty_()) return null;
			//int paramsLen = parameters.Length;
			//var typeArray = new Type[paramsLen];
			//for(int i = 0; i < paramsLen; i++)
			//	typeArray[i] = parameters[i].GetType();
			//var method = instance.GetType().GetMethod(methodName, typeArray);
			var method = GetMethodInfo(instance.GetType(), methodName, parameters);

			var parameterinfos = method?.GetParameters();
			if (parameterinfos?.Length == 1 && parameterinfos[0].ParameterType == typeof(object[]))
				parameters = new object[] { parameters };

			return method?.FastInvoke(instance, parameters);
		}
		/// <summary>
		/// 反射对象类型,执行对象内部方法
		/// </summary>
		/// <param name="type">对象类型</param>
		/// <param name="methodName">方法名</param>
		/// <param name="paramters">参数</param>
		/// <returns>object</returns>
		public static object FastInvoke(this Type type, string methodName, params object[] paramters)
		{
			if (type == null)
				throw new Exception("对象类型 type 不能为NULL");
			var target = FastInstance(type);//Activator.CreateInstance(type);
			return FastInvoke(target, methodName, paramters);
		}
		/// <summary>
		/// 反射对象类型,执行对象内部方法
		/// </summary>
		/// <param name="type">对象类型</param>
		/// <param name="methodName">方法名</param>
		/// <param name="paramters">参数</param>
		/// <returns>object</returns>
		public static object FastInvoke(this string typeName, string methodName, params object[] paramters)
		{
			if (typeName.IsNullOrEmpty_())
				throw new Exception("对象类型 type 不能为NULL");
			var target = FastInstance(typeName);// Activator.CreateInstance(type);
			return FastInvoke(target, methodName, paramters);
		}
		/// <summary>
		/// 反射对象类型,执行对象内部方法
		/// </summary>
		/// <param name="type">对象类型</param>
		/// <param name="methodName">方法名</param>
		/// <param name="paramters">参数</param>
		/// <returns>object</returns>
		public static object FastInvoke(this MethodInfo methodInfo, Type type, params object[] paramters)
		{
			if (type == null)
				throw new Exception("对象类型 type 不能为NULL");
			var target = FastInstance(type);//Activator.CreateInstance(type);
			return FastInvoke(methodInfo, target, paramters);
		}
		#endregion

		#region 属性
		#region 获取
		private static Dict<PropertyInfo, Func<object, object>> PGC;

		/// <summary>快速获取对象属性值</summary>
		/// <param name="propertyInfo">要获取的对象属性</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static object FastGetValue(this PropertyInfo propertyInfo, object instance)
		{
			if (propertyInfo == null) return null;
			if (!propertyInfo.CanRead) return null;

			if (PGC == null) PGC = new Dict<PropertyInfo, Func<object, object>>();
			var exec = PGC.Get(propertyInfo, () =>
			{
				if (propertyInfo.GetIndexParameters().Length > 0)
				{
					// 对索引属性直接返回默认值
					return (inst) =>
					{
						return null;
					};
				}
				else
				{
					var objType = typeof(object);
					var instanceParameter = Expression.Parameter(objType, "instance");
					var instanceCast = propertyInfo.GetGetMethod(true).IsStatic ? null : Expression.Convert(instanceParameter, propertyInfo.ReflectedType);
					var propertyAccess = Expression.Property(instanceCast, propertyInfo);
					var castPropertyValue = Expression.Convert(propertyAccess, objType);
					var lambda = Expression.Lambda<Func<object, object>>(castPropertyValue, instanceParameter);

					return lambda.Compile();
				}
			});
			return exec(instance);
		}

		/// <summary>通过属性名称快速获取对象属性值</summary>
		/// <param name="propertyName">要获取的对象属性名称</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static object FastGetPropertyValue(string propertyName, object instance)
		{
			if (propertyName.IsNullOrEmpty_()) return null;
			var propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			if (propertyInfo != null)
				return propertyInfo.FastGetValue(instance);
			return null;
		}


		/// <summary>快速获取对象属性值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="propertyInfo">要获取的对象属性</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static T FastGetValue<T>(this PropertyInfo propertyInfo, object instance)
		{
			return (T)propertyInfo.FastGetValue(instance);
		}

		/// <summary>通过属性名称快速获取对象属性值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="propertyName">要获取的对象属性名称</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static T FastGetValue<T>(string propertyName, object instance)
		{
			return (T)FastGetValue(propertyName, instance);
		}

		public static object FastGetValue(string propertyName, object instance)
		{
			if (propertyName.IsNullOrEmpty_() || instance.IsNullOrEmpty_()) return null;

			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
			var propertyInfo = instance.GetType().GetProperty(propertyName, bindingFlags);
			if (propertyInfo != null)
				return propertyInfo.FastGetValue(instance);

			var fieldInfo = instance.GetType().GetField(propertyName, bindingFlags);
			if (fieldInfo != null)
				return fieldInfo.FastGetValue(instance);
			return null;
		}

		private static Dict<PropertyInfo, Func<object>> SPGC;
		/// <summary>
		/// 快速获取对象静态属性值
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public static object FastGetStaticValue(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) return null;
			if (!propertyInfo.CanRead) return null;

			if (SPGC == null) SPGC = new Dict<PropertyInfo, Func<object>>();
			var exec = SPGC.Get(propertyInfo, () =>
			{
				var fieldExp = Expression.Property(null, propertyInfo);
				var lambda = Expression.Lambda<Func<object>>(
					Expression.Convert(fieldExp, typeof(object))
				);
				return lambda.Compile();
			});
			return exec();
		}

		/// <summary>通过属性名称快速获取对象静态属性值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="propertyName">要获取的对象属性名称</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticPropertyValue(string propertyName, Type type)
		{
			if (propertyName.IsNullOrEmpty_() || type.IsNullOrEmpty_()) return null;
			var propertyInfo = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			return FastGetStaticValue(propertyInfo);
		}
		/// <summary>通过属性名称快速获取对象静态属性值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="propertyName">要获取的对象属性名称</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticPropertyValue<T>(string propertyName)
		{
			return FastGetStaticPropertyValue(propertyName, typeof(T));
		}
		/// <summary>通过属性名称快速获取对象静态属性值</summary>
		/// <param name="typeName">返回的数据类型</typeparam>
		/// <param name="propertyName">要获取的对象属性名称</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticPropertyValue(string propertyName, string typeName)
		{
			return FastGetStaticPropertyValue(propertyName, FastGetType(typeName));
		}
		public static object FastGetStaticValue(string propertyName, string typeName)
		{
			if (propertyName.IsNullOrEmpty_() || typeName.IsNullOrEmpty_()) return null;

			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
			var type = FastGetType(typeName);
			if (type == null) return null;
			var propertyInfo = type.GetProperty(propertyName, bindingFlags);
			if (propertyInfo != null)
				return propertyInfo.FastGetStaticValue();

			var fieldInfo = type.GetField(propertyName, bindingFlags);
			if (fieldInfo != null)
				return fieldInfo.FastGetStaticValue();
			return null;
		}
		#endregion

		#region 设置
		private static Dict<PropertyInfo, Action<object, object>> PSC;

		/// <summary>快速设置对象属性值</summary>
		/// <param name="propertyInfo">要设置的对象属性</param>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public static void FastSetValue(this PropertyInfo propertyInfo, object instance, object value)
		{
			if (propertyInfo == null) return;
			if (!propertyInfo.CanWrite) return;

			if (PSC == null) PSC = new Dict<PropertyInfo, Action<object, object>>();
			var exec = PSC.Get(propertyInfo, () =>
			{
				if (propertyInfo.GetIndexParameters().Length > 0)
				{
					// 对索引属性直接无视
					return (inst, para) => { };
				}
				else
				{
					var objType = typeof(object);
					var instanceParameter = Expression.Parameter(objType, "instance");
					var valueParameter = Expression.Parameter(objType, "value");
					var instanceCast = propertyInfo.GetSetMethod(true).IsStatic ? null : Expression.Convert(instanceParameter, propertyInfo.ReflectedType);
					var valueCast = Expression.Convert(valueParameter, propertyInfo.PropertyType);
					var propertyAccess = Expression.Property(instanceCast, propertyInfo);
					var propertyAssign = Expression.Assign(propertyAccess, valueCast);
					var lambda = Expression.Lambda<Action<object, object>>(propertyAssign, instanceParameter, valueParameter);

					return lambda.Compile();
				}
			});
			exec(instance, value);
		}

		/// <summary>通过属性名称快速设置对象属性值</summary>
		/// <param name="propertyName">要设置的对象属性名称</param>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public static void FastSetPropertyValue(string propertyName, object instance, object value)
		{
			if (propertyName.IsNullOrEmpty_()) return;
			var propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			if (propertyInfo != null)
				propertyInfo.FastSetValue(instance, value);
		}

		/// <summary>通过属性名称快速设置对象属性值</summary>
		/// <param name="propertyName">要设置的对象属性名称</param>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public static void FastSetValue(string propertyName, object instance, object value)
		{
			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

			if (propertyName.IsNullOrEmpty_()) return;
			var propertyInfo = instance.GetType().GetProperty(propertyName, bindingFlags);
			if (propertyInfo != null)
			{
				propertyInfo.FastSetValue(instance, value);
				return;
			}
			var fieldInfo = instance.GetType().GetField(propertyName, bindingFlags);
			if (fieldInfo != null)
				fieldInfo.FastSetValue(instance, value);
		}
		public static void FastSetValue(string propertyName, string typeName, object value)
		{
			if (propertyName.IsNullOrEmpty_() || typeName.IsNullOrEmpty_()) return;

			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
			var type = FastGetType(typeName);
			if (type == null) return;
			var obj = type.FastInstance();
			var propertyInfo = type.GetProperty(propertyName, bindingFlags);
			if (propertyInfo != null)
				propertyInfo.FastSetValue(obj, value);

			var fieldInfo = type.GetField(propertyName, bindingFlags);
			if (fieldInfo != null)
				fieldInfo.FastSetValue(obj, value);
		}
		#endregion
		#endregion

		#region 字段
		#region 读取
		private static Dict<FieldInfo, Func<object, object>> FGC;

		/// <summary>快速获取对象字段值</summary>
		/// <param name="fieldInfo">要获取的对象字段</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static object FastGetValue(this FieldInfo fieldInfo, object instance)
		{
			if (fieldInfo == null) return null;

			if (FGC == null) FGC = new Dict<FieldInfo, Func<object, object>>();
			var exec = FGC.Get(fieldInfo, () =>
			{
				var objType = typeof(object);
				var instanceParameter = Expression.Parameter(objType, "instance");
				var instanceCast = fieldInfo.IsStatic ? null : Expression.Convert(instanceParameter, fieldInfo.ReflectedType);
				var fieldAccess = Expression.Field(instanceCast, fieldInfo);
				var castFieldValue = Expression.Convert(fieldAccess, objType);
				var lambda = Expression.Lambda<Func<object, object>>(castFieldValue, instanceParameter);

				return lambda.Compile();
			});
			return exec(instance);
		}

		/// <summary>通过字段名称快速获取对象字段值</summary>
		/// <param name="fieldName">要获取的对象字段名称</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static object FastGetFieldValue(string fieldName, object instance)
		{
			if (fieldName.IsNullOrEmpty_()) return null;
			var fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			if (fieldInfo != null)
				return fieldInfo.FastGetValue(instance);
			return null;
		}

		/// <summary>快速获取对象字段值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="fieldInfo">要获取的对象字段</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static T FastGetValue<T>(this FieldInfo fieldInfo, object instance)
		{
			return (T)fieldInfo.FastGetValue(instance);
		}

		/// <summary>通过字段名称快速获取对象字段值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="fieldName">要获取的对象字段名称</param>
		/// <param name="instance">要获取的对象实例</param>
		/// <returns>属性值</returns>
		public static T FastGetFieldValue<T>(string fieldName, object instance)
		{
			return (T)FastGetFieldValue(fieldName, instance);
		}

		private static Dict<FieldInfo, Func<object>> SFGC;
		/// <summary>
		/// 快速获取对象静态字段值
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public static object FastGetStaticValue(this FieldInfo field)
		{
			if (field == null) return null;

			if (SFGC == null) SFGC = new Dict<FieldInfo, Func<object>>();
			var exec = SFGC.Get(field, () =>
			{
				var fieldExp = Expression.Field(null, field);
				var lambda = Expression.Lambda<Func<object>>(
					Expression.Convert(fieldExp, typeof(object))
				);
				return lambda.Compile();
			});
			return exec();
		}
		/// <summary>通过字段名称快速获取对象静态字段值</summary>
		/// <typeparam name="T">返回的数据类型</typeparam>
		/// <param name="fieldInfo">要获取的对象字段</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticFieldValue<T>(string fieldName)
		{
			return FastGetStaticFieldValue(fieldName, typeof(T));
		}
		/// <summary>通过字段名称快速获取对象静态字段值</summary>
		/// <param name="type">返回的数据类型</typeparam>
		/// <param name="fieldInfo">要获取的对象字段</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticFieldValue(string fieldName, Type type)
		{
			if (fieldName.IsNullOrEmpty_() || type.IsNullOrEmpty_()) return null;
			var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			return FastGetStaticValue(field);
		}
		/// <summary>通过字段名称快速获取对象静态字段值</summary>
		/// <param name="typeName">返回的数据类型</typeparam>
		/// <param name="fieldInfo">要获取的对象字段</param>
		/// <returns>属性值</returns>
		public static object FastGetStaticFieldValue(string fieldName, string typeName)
		{
			return FastGetStaticFieldValue(fieldName, FastGetType(typeName));
		}
		#endregion

		#region 设置
		private static Dict<FieldInfo, Action<object, object>> FSC;

		/// <summary>快速设置对象字段值</summary>
		/// <param name="fieldInfo">要设置的对象字段</param>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public static void FastSetValue(this FieldInfo fieldInfo, object instance, object value)
		{
			if (fieldInfo == null) return;

			if (FSC == null) FSC = new Dict<FieldInfo, Action<object, object>>();
			var exec = FSC.Get(fieldInfo, () =>
			{
				var objType = typeof(object);
				var instanceParameter = Expression.Parameter(objType, "instance");
				var valueParameter = Expression.Parameter(objType, "value");
				var instanceCast = fieldInfo.IsStatic ? null : Expression.Convert(instanceParameter, fieldInfo.ReflectedType);
				var valueCast = Expression.Convert(valueParameter, fieldInfo.FieldType);
				var fieldAccess = Expression.Field(instanceCast, fieldInfo);
				var fieldAssign = Expression.Assign(fieldAccess, valueCast);
				var lambda = Expression.Lambda<Action<object, object>>(fieldAssign, instanceParameter, valueParameter);

				return lambda.Compile();
			});
			exec(instance, value);
		}

		/// <summary>通过字段名称快速设置对象字段值</summary>
		/// <param name="fieldName">要设置的对象字段名称</param>
		/// <param name="instance">要设置的对象实例</param>
		/// <param name="value">要设置的值</param>
		public static void FastSetFieldValue(string fieldName, object instance, object value)
		{
			if (fieldName.IsNullOrEmpty_()) return;
			var fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			if (fieldInfo != null)
				fieldInfo.FastSetValue(instance, value);
		}
		#endregion
		#endregion

		#region 类型相关
		#region 获取类型
		private static Dict<string, Type> NTC;

		/// <summary>通过类型的程序集限定名称获取类型</summary>
		/// <param name="typeName">要获取的类型的程序集限定名称</param>
		/// <returns>类型</returns>
		/// <exception cref="ArgumentNullOrEmptyException">类型的程序集限定名称为 null 或为空</exception>
		public static Type FastGetType(this string typeName)
		{
			if (typeName.IsNullOrEmpty_())
				throw new ArgumentException("值不能爲 null 或 empty。");

			if (NTC == null) NTC = new Dict<string, Type>(StringComparer.OrdinalIgnoreCase);
			typeName = typeName.Trim();
			return NTC.Get(typeName, () =>
			{
				try
				{
					var type = GetTypeByString(typeName);
					if (type != null)
					{
						return type;
					}
					type = Type.GetType(typeName, true, true);
					if (type == null) type = GetType(typeName);
					return type;
				}
				catch { var type = GetType(typeName); return type; }
			});
		}
		private static Type GetType(string typeName)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var allTypes = assembly.GetTypes();
				var type = allTypes.FirstOrDefault(t => t.Name == typeName || t.FullName.Contains(typeName));
				if (type != null)
				{
					return type;
				}
			}
			throw new KeyNotFoundException("Can't find type " + typeName);
		}
		private static Type GetTypeByString(string type)
		{
			switch (type.ToLower())
			{
				case "bool":
					return Type.GetType("System.Boolean", false, true);
				case "byte":
					return Type.GetType("System.Byte", false, true);
				case "sbyte":
					return Type.GetType("System.SByte", false, true);
				case "char":
					return Type.GetType("System.Char", false, true);
				case "decimal":
					return Type.GetType("System.Decimal", false, true);
				case "double":
					return Type.GetType("System.Double", false, true);
				case "single":
				case "float":
					return Type.GetType("System.Single", false, true);
				case "int32":
				case "int":
					return Type.GetType("System.Int32", false, true);
				case "uint32":
				case "uint":
					return Type.GetType("System.UInt32", false, true);
				case "int64":
				case "long":
					return Type.GetType("System.Int64", false, true);
				case "uint64":
				case "ulong":
					return Type.GetType("System.UInt64", false, true);
				case "object":
					return Type.GetType("System.Object", false, true);
				case "int16":
				case "short":
					return Type.GetType("System.Int16", false, true);
				case "uint16":
				case "ushort":
					return Type.GetType("System.UInt16", false, true);
				case "string":
					return Type.GetType("System.String", false, true);
				case "date":
				case "datetime":
					return Type.GetType("System.DateTime", false, true);
				case "guid":
					return Type.GetType("System.Guid", false, true);
				default:
					return Type.GetType(type, false, true);
			}
		}
		#endregion

		#region 获取类型属性
		private static Dict<string, Dict<string, PropertyInfo>> PI;

		/// <summary>获取类型的属性集合</summary>
		/// <param name="type">要获取的类型</param>
		/// <returns>类型的属性集合</returns>
		public static Dict<string, PropertyInfo> FastGetPropertys(this Type type)
		{
			if (PI == null) PI = new Dict<string, Dict<string, PropertyInfo>>(StringComparer.OrdinalIgnoreCase);

			var typeName = type.FullName;
			return PI.Get(typeName, () =>
			{
				var ls = new Dict<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var p in props) ls[p.Name] = p;
				return ls;
			});
		}

		/// <summary>获取类型的属性集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的属性集合</returns>
		public static Dict<string, PropertyInfo> FastGetPropertys<T>()
		{
			return typeof(T).FastGetPropertys();
		}
		/// <summary>获取类型的属性集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的属性集合</returns>
		public static List<PropertyInfo> FastGetPropertyList<T>()
		{
			return FastGetPropertyList(typeof(T));
		}
		/// <summary>获取类型的属性集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的属性集合</returns>
		public static List<PropertyInfo> FastGetPropertyList(Type ctype)
		{
			var dic = ctype.FastGetPropertys();
			return dic?.Select(s => s.Value).ToList();
		}
		#endregion

		#region 获取类型公共字段
		private static Dict<string, Dictionary<string, FieldInfo>> FI;

		/// <summary>获取类型的公共字段集合</summary>
		/// <param name="type">要获取的类型</param>
		/// <returns>类型的公共字段集合</returns>
		public static Dictionary<string, FieldInfo> FastGetFields(this Type type)
		{
			if (FI == null) FI = new Dict<string, Dictionary<string, FieldInfo>>(StringComparer.OrdinalIgnoreCase);

			var typeName = type.FullName;
			return FI.Get(typeName, () =>
			{
				var ls = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
				var props = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in props) ls[f.Name] = f;
				return ls;
			});
		}

		/// <summary>获取类型的公共字段集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的公共字段集合</returns>
		public static Dictionary<string, FieldInfo> FastGetFields<T>()
		{
			return typeof(T).FastGetFields();
		}

		/// <summary>获取类型的公共字段集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的公共字段集合</returns>
		public static List<FieldInfo> FastGetFieldList<T>()
		{
			var dic = typeof(T).FastGetFields();
			return dic?.Select(s => s.Value).ToList();
		}
		#endregion

		#region 获取类型访问器
		private static Dict<string, Dictionary<string, Accessor>> AC;

		/// <summary>获取类型的访问器集合</summary>
		/// <param name="type">要获取的类型</param>
		/// <returns>类型的访问器集合</returns>
		public static Dictionary<string, Accessor> FastGetAccessors(this Type type)
		{
			if (AC == null) AC = new Dict<string, Dictionary<string, Accessor>>(StringComparer.OrdinalIgnoreCase);

			var typeName = type.FullName;
			return AC.Get(typeName, () =>
			{
				var ls = new Dictionary<string, Accessor>(StringComparer.OrdinalIgnoreCase);
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var name = string.Empty;
				foreach (var p in props)
				{
					name = p.Name;
					var a = new Accessor
					{
						Name = name,
						Member = p,
						DataType = p.PropertyType,
						AccessorType = AccessorType.Property,
						CanRade = p.CanRead,
						CanWrite = p.CanWrite,
						IsVirtual = p.IsVirtual()
					};
					ls.Add(name, a);
				}

				var fis = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in fis)
				{
					name = f.Name;
					var a = new Accessor
					{
						Name = name,
						Member = f,
						DataType = f.FieldType,
						AccessorType = AccessorType.Field,
						CanRade = true,
						CanWrite = true,
						IsVirtual = false
					};
					ls.Add(name, a);
				}

				return ls;
			});
		}

		/// <summary>获取类型的访问器集合</summary>
		/// <typeparam name="T">要获取的类型</typeparam>
		/// <returns>类型的访问器集合</returns>
		public static Dictionary<string, Accessor> FastGetAccessors<T>()
		{
			return typeof(T).FastGetAccessors();
		}
		#endregion
		#endregion

		#region GetConstructor
		/// <summary>
		/// 获取构造函数
		/// </summary>
		/// <param name="_Type"></param>
		/// <param name="paramters"></param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructor(this Type _Type, object[] paramters)
		{
			bool argExistsNull;

			Type[] argsTypes = GetTypeArray(paramters, out argExistsNull);
			try
			{
				var gct = _Type.GetConstructor(argsTypes);
				if (gct == null)
				{
					var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
					gct = _Type.GetConstructors(bindingAttr)?.FirstOrDefault(m => m.GetParameters()?.Length == argsTypes.Length);
				}
				return gct;
			}
			catch (ArgumentNullException)
			{
				return GetConstructor(_Type, argsTypes, argExistsNull);
			}
			catch (ArgumentException)
			{
				return GetConstructor(_Type, argsTypes, argExistsNull);
			}
		}
		/// <summary>
		/// 获取构造函数
		/// </summary>
		/// <param name="_Type"></param>
		/// <param name="argsTypes"></param>
		/// <param name="argExistsNull"></param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructor(this Type _Type, Type[] types, bool argExistsNull = false)
		{
			int _iParamCount = types == null ? 0 : types.Length;
			var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
			if (argExistsNull)//如果入参存在空值
			{
				//通过名称获取所有配置的方法
				var _listConstructorInfo = _Type.GetConstructors(bindingAttr).ToList();
				if (_listConstructorInfo?.Count == 1)//唯一直接返回
				{
					return _listConstructorInfo[0];
				}
				else if (_listConstructorInfo?.Count > 1)
				{
					var _listConstructorInfoc = _listConstructorInfo.FindAll(p => p.GetParameters().Length.Equals(_iParamCount));
					if (_listConstructorInfoc?.Count == 1)//通过入参数量来配置方法，唯一直接返回
					{
						return _listConstructorInfoc[0];
					}
					else if (_listConstructorInfoc?.Count > 1)
					{
						//寻找参数类型匹配最多的方法
						var mathCount = new List<int>(new int[_listConstructorInfoc.Count]);
						for (var i = 0; i < _listConstructorInfoc.Count; i++)
						{
							var prs = _listConstructorInfoc[i].GetParameters();
							mathCount[i] = 0;
							for (var k = 0; k < prs.Length; k++)
							{
								if ((types[k] != null
										 && (types[k] == prs[k].ParameterType || types[k] == Nullable.GetUnderlyingType(prs[k].ParameterType)))
									 || (types[k] == null
										 && (!prs[k].ParameterType.IsValueType || prs[k].ParameterType.IsNullable())))
									mathCount[i]++;
							}
						}
						return _listConstructorInfoc[mathCount.IndexOf(mathCount.Max())];
					}
				}
				return null;
			}
			//------------------------------------------------------------------------------------------------------------------
			if (_iParamCount > 0)
			{
				return _Type.GetConstructor(bindingAttr,//筛选条件
						 Type.DefaultBinder,//绑定
						 types,//参数类型
						 new ParameterModifier[] { new ParameterModifier(_iParamCount) }//参数个数
						  );
			}
			else
			{
				try
				{
					return _Type.GetConstructor(types);
				}
				catch (AmbiguousMatchException)
				{
					return _Type.GetConstructors(bindingAttr)?.FirstOrDefault(m => m.GetParameters()?.Length == 0);
				}
			}
		}
		#endregion

		#region GetMethodInfo
		///// <summary>
		///// 移除控件eventhandler方法
		///// </summary>
		///// <param name="target"></param>
		///// <param name="eventName"></param>
		//public static void RemoveEventHandler(this Control target, string eventName)
		//{
		//	if (target == null) return;
		//	if (string.IsNullOrEmpty(eventName)) return;

		//	BindingFlags bPropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;//筛选
		//	BindingFlags fieldFlags = BindingFlags.Static | BindingFlags.NonPublic;
		//	Type ctrlType = typeof(System.Windows.Forms.Control);
		//	PropertyInfo pInfo = ctrlType.GetProperty("Events", bPropertyFlags);
		//	EventHandlerList evtHLst = (EventHandlerList)pInfo.GetValue(target, null);//事件列表
		//	FieldInfo fieldInfo = (typeof(Control).GetField("Event" + eventName, fieldFlags));
		//	Delegate d = evtHLst[fieldInfo.GetValue(target)];

		//	if (d == null) return;
		//	EventInfo eventInfo = ctrlType.GetEvent(eventName);

		//	foreach (Delegate dx in d.GetInvocationList())
		//	{
		//		//移除已订阅的eventName类型事件
		//		eventInfo.RemoveEventHandler(target, dx);
		//	}
		//}

		/// <summary>
		/// 获取对象方法
		/// </summary>
		/// <param name="_Type"></param>
		/// <param name="_sMethodName"></param>
		/// <param name="paramters"></param>
		/// <returns></returns>
		public static MethodInfo GetMethodInfo(this Type _Type, string _sMethodName, params object[] paramters)
		{
			try
			{
				var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
				var md = _Type.GetMethod(_sMethodName, bindingAttr);
				if (md == null)
				{
					bool argExistsNull;
					Type[] argsTypes = GetTypeArray(paramters, out argExistsNull);
					return GetMethodInfo(_Type, _sMethodName, argsTypes, true);
				}
				else
				{
					return md;
				}
			}
			catch (AmbiguousMatchException)
			{
				bool argExistsNull;
				Type[] argsTypes = GetTypeArray(paramters, out argExistsNull);
				return GetMethodInfo(_Type, _sMethodName, argsTypes, argExistsNull);
			}
		}

		/// <summary>
		/// 获取指定数组中对象的类型
		/// </summary>
		/// <param name="paramters"></param>
		/// <param name="argExistsNull"></param>
		/// <returns></returns>
		public static Type[] GetTypeArray(object[] paramters, out bool argExistsNull)
		{
			argExistsNull = false;
			Type[] argsTypes = new Type[0];
			try
			{
				if (paramters?.Length > 0)
				{
					argsTypes = new Type[paramters.Length]; //Type.GetTypeArray(paramters);
					for (var i = 0; i < paramters.Length; i++)
					{
						if (paramters[i] != null)
						{
							argsTypes[i] = paramters[i].GetType();
						}
						else
						{
							argsTypes[i] = null;
							argExistsNull = true;
						}
					}
				}
			}
			catch
			{
				argsTypes = new Type[0];
			}
			return argsTypes;
		}
		/// <summary>
		/// 获取对象方法
		/// </summary>
		/// <param name="_Type"></param>
		/// <param name="_sMethodName"></param>
		/// <param name="argsTypes"></param>
		/// <param name="argExistsNull"></param>
		/// <returns></returns>
		public static MethodInfo GetMethodInfo(Type _Type, string _sMethodName, Type[] argsTypes, bool argExistsNull = false)
		{
			int _iParamCount = argsTypes == null ? 0 : argsTypes.Length;
			var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			if (argExistsNull)//如果入参存在空值
			{
				//通过名称获取所有配置的方法
				var _listMethodInfo = _Type.GetMethods(bindingAttr).ToList().FindAll(p => p.Name.Equals(_sMethodName, StringComparison.OrdinalIgnoreCase));
				if (_listMethodInfo?.Count == 1)//唯一直接返回
				{
					return _listMethodInfo[0];
				}
				else if (_listMethodInfo?.Count > 1)
				{
					var _listMethodInfoc = _listMethodInfo.FindAll(p => p.GetParameters().Length.Equals(_iParamCount));
					if (_listMethodInfoc?.Count == 1)//通过入参数量来配置方法，唯一直接返回
					{
						return _listMethodInfoc[0];
					}
					else if (_listMethodInfoc?.Count > 1)
					{
						//寻找参数类型匹配最多的方法
						var mathCount = new List<int>(new int[_listMethodInfoc.Count]);
						for (var i = 0; i < _listMethodInfoc.Count; i++)
						{
							var prs = _listMethodInfoc[i].GetParameters();
							mathCount[i] = 0;
							for (var k = 0; k < prs.Length; k++)
							{
								if ((argsTypes[k] != null
										 && (argsTypes[k] == prs[k].ParameterType || argsTypes[k] == Nullable.GetUnderlyingType(prs[k].ParameterType)))
									 || (argsTypes[k] == null
										 && (!prs[k].ParameterType.IsValueType || prs[k].ParameterType.IsNullable())))
									mathCount[i]++;
							}
						}
						return _listMethodInfoc[mathCount.IndexOf(mathCount.Max())];
					}
				}
				return null;
			}
			//------------------------------------------------------------------------------------------------------------------
			if (_iParamCount > 0)
			{
				return _Type.GetMethod(_sMethodName, bindingAttr,//筛选条件
						 Type.DefaultBinder,//绑定
						 argsTypes,//参数类型
						 new ParameterModifier[] { new ParameterModifier(_iParamCount) }//参数个数
						  );
			}
			else
			{
				try
				{
					return _Type.GetMethod(_sMethodName, bindingAttr);
				}
				catch (AmbiguousMatchException)
				{
					return _Type.GetMethods(bindingAttr)?.FirstOrDefault(m => m.Name.Equals(_sMethodName, StringComparison.OrdinalIgnoreCase) && m.GetParameters()?.Length == 0);
				}
			}
		}
		#endregion

	}
	#endregion
}