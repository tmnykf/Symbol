﻿/*  
 *  author：symbolspace
 *  e-mail：symbolspace@outlook.com
 */
using System;

namespace Symbol.Data.Binding {
    
    /// <summary>
    /// 数据绑定特性基类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class DataBinderAttribute : Attribute, Symbol.Data.Binding.IDataBinder {

        #region fields
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Generic.List<BindItem>> _list_key=new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Generic.List<BindItem>>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, PreSelectBuilderFunc> _list_condition_func = new System.Collections.Concurrent.ConcurrentDictionary<string, PreSelectBuilderFunc>();
        #endregion

        #region properties
        /// <summary>
        /// 获取源名称。
        /// </summary>
        public string SourceName { get; private set; }
        /// <summary>
        /// 获取过虑规则。
        /// </summary>
        public Symbol.Data.NoSQL.Condition Condition { get; private set; }
        /// <summary>
        /// 获取排序规则。
        /// </summary>
        public Symbol.Data.NoSQL.Sorter Sorter { get; private set; }
        /// <summary>
        /// 获取或设置输出字段。
        /// </summary>
        public string Field { get; set; }
        ///// <summary>
        ///// 获取或设置元素Path，设置后将对最终输出的值，调用一次Path。
        ///// </summary>
        //public string ElementPath { get; set; }
        /// <summary>
        /// 获取或设置允许缓存，默认为true。
        /// </summary>
        public bool AllowCache { get; set; }

        #endregion


        #region ctor
        /// <summary>
        /// 创建DataBinderAttribute实例。
        /// </summary>
        /// <param name="sourceName">源名称。</param>
        /// <param name="condition">过虑规则。</param>
        /// <param name="field">输出字段，输出为单值时。</param>
        /// <param name="sort">排序规则。</param>
        public DataBinderAttribute(string sourceName, string condition, string field = "*", string sort = "{}") {
            AllowCache = true;
            SourceName = sourceName;
            Field = field;
            Condition = Symbol.Data.NoSQL.Condition.Parse(condition);
            Sorter = Symbol.Data.NoSQL.Sorter.Parse(sort);
        }
        #endregion

        #region methods

        #region Bind
        /// <summary>
        /// 绑定数据。
        /// </summary>
        /// <param name="dataContext">数据上下文对象。</param>
        /// <param name="dataReader">数据读取对象。</param>
        /// <param name="entity">当前实体对象。</param>
        /// <param name="type">类型。</param>
        /// <param name="cache">缓存。</param>
        public static void Bind(IDataContext dataContext, System.Data.IDataReader dataReader, object entity, System.Type type, IDataBinderObjectCache cache) {
            if (entity == null)
                return;
            var list = TryGetBind(type);
            if (list == null || list.Count == 0)
                return;
            for (int i = 0; i < list.Count; i++) {
                list[i].Bind(dataContext, dataReader, entity, cache);
            }
        }


        /// <summary>
        /// 绑定数据。
        /// </summary>
        /// <param name="dataContext">数据上下文对象。</param>
        /// <param name="dataReader">数据读取对象。</param>
        /// <param name="entity">当前实体对象。</param>
        /// <param name="field">当前字段。</param>
        /// <param name="type">实体中字段的类型。</param>
        /// <param name="cache">缓存。</param>
        /// <returns>返回绑定的数据。</returns>
        public abstract object Bind(IDataContext dataContext, System.Data.IDataReader dataReader, object entity, string field, Type type, IDataBinderObjectCache cache);
        #endregion

        #region PreSelectBuilder
        /// <summary>
        /// select命令构造器预处理（处理$this.xx值引用）。
        /// </summary>
        /// <param name="dataContext">数据上下文对象。</param>
        /// <param name="dataReader">数据读取对象。</param>
        /// <param name="entity">当前实体对象。</param>
        /// <param name="builder">select命令构造器。</param>
        /// <param name="cache">缓存。</param>
        protected virtual void PreSelectBuilder(IDataContext dataContext, System.Data.IDataReader dataReader, object entity, Symbol.Data.ISelectCommandBuilder builder, IDataBinderObjectCache cache) {
            var baseAddCommandParameter = builder.AddCommandParameter;
            builder.AddCommandParameter = (p) => {
                string p10 = p as string;
                if (p10 != null && p10[0] == '$' && p10.Length > 1) {
                    var value = CacheFunc(cache, entity, p10, () => {
                        var func = GetPreSelectBuilderFunc(entity.GetType(), p10);
                        return func?.Invoke(dataContext, dataReader, entity);
                    });
                    //var value = GetPreSelectBuilderFunc(entity.GetType(), p10)?.Invoke(dataContext, dataReader, entity);
                    return baseAddCommandParameter(value);
                }
                return baseAddCommandParameter(p);
            };
        }
        //_list_condition_func
        PreSelectBuilderFunc GetPreSelectBuilderFunc(System.Type type, string value) {
            PreSelectBuilderFunc func;
            string key = type.AssemblyQualifiedName + "|" + value;
            if (!_list_condition_func.TryGetValue(key, out func)) {
                ThreadHelper.Block(_list_condition_func, () => {
                    if (!_list_condition_func.TryGetValue(value, out func)) {
                        if (value.StartsWith("$this.", StringComparison.OrdinalIgnoreCase)) {
                            string path = value.Substring("$this.".Length);
                            func = (dataContext, dataReader, entity) => {
                                return FastObject.Path(entity, path);
                            };
                        }
                        if (value.StartsWith("$reader.", StringComparison.OrdinalIgnoreCase)) {
                            string p10 = value.Substring("$reader.".Length);
                            if (p10.IndexOf('.') > -1) {
                                string name = p10.Split('.')[0];
                                string path = p10.Substring(name.Length + 1);
                                func = (dataContext, dataReader, entity) => {
                                    return FastObject.Path(DataReaderHelper.Current(dataReader, name), path);
                                };
                            } else {
                                func = (dataContext, dataReader, entity) => {
                                    return DataReaderHelper.Current(dataReader, p10);
                                };
                            }
                        }
                        _list_condition_func.TryAdd(key, func);
                    }
                });
                
            }
            return func;
        }
        delegate object PreSelectBuilderFunc(IDataContext dataContext, System.Data.IDataReader dataReader, object entity);
        #endregion
        #region BuildCacheKey
        /// <summary>
        /// 构造缓存键值。
        /// </summary>
        /// <param name="builder">select命令构造器。</param>
        /// <param name="tag">标记。</param>
        /// <param name="type">类型。</param>
        /// <returns>返回缓存键值。</returns>
        protected virtual string BuildCacheKey(Symbol.Data.ISelectCommandBuilder builder, string tag, Type type) {
            return string.Concat(
                    tag, ":",
                    type.AssemblyQualifiedName,"|",
                    builder.CommandText, "/",
                    Symbol.Serialization.Json.ToString(builder.Parameters, true)
                  );
        }
        /// <summary>
        /// 缓存键值操作。
        /// </summary>
        /// <param name="cache">缓存对象。</param>
        /// <param name="entity">当前实体对象。</param>
        /// <param name="field">当前字段。</param>
        /// <param name="func">缓存求值委托。</param>
        /// <returns>返回</returns>
        protected virtual object CacheFunc(IDataBinderObjectCache cache, object entity, string field, CacheValueFunc func) {
            if (cache != null && AllowCache) {
                object value;
                if (!cache.Get(entity, field, out value)) {
                    value = func();
                    cache.Set(entity, field, value);
                }
                return value;
            } else {
                return func();
            }
        }

        /// <summary>
        /// 缓存键值操作。
        /// </summary>
        /// <param name="cache">缓存对象。</param>
        /// <param name="builder">select命令构造器。</param>
        /// <param name="tag">标记。</param>
        /// <param name="type">类型。</param>
        /// <param name="func">缓存求值委托。</param>
        /// <returns>返回</returns>
        protected virtual object CacheFunc(IDataBinderObjectCache cache, Symbol.Data.ISelectCommandBuilder builder, string tag, Type type, CacheValueFunc func) {
            if (cache != null && AllowCache) {
                object value;
                string key = BuildCacheKey(builder, tag, type);
                if (!cache.Get(key, out value)) {
                    value = func();
                    cache.Set(key, value);
                }
                return value;
            } else {
                return func();
            }
        }
        #endregion

        #region TryGetBind
        static System.Collections.Generic.List<BindItem> TryGetBind(System.Type entityType) {
            string key = entityType.AssemblyQualifiedName;
            System.Collections.Generic.List<BindItem> list;
            if (!_list_key.TryGetValue(key, out list)) {
                ThreadHelper.Block(_list_key, () => {
                    if (!_list_key.TryGetValue(key, out list)) {
                        if (entityType.IsValueType || entityType == typeof(string) || entityType==typeof(object) || TypeExtensions.IsNullableType(entityType)) {
                            _list_key.TryAdd(key, null);
                            return;
                        }

                        list = new System.Collections.Generic.List<BindItem>();
                        foreach (System.Reflection.PropertyInfo propertyInfo in entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty)) {
                            var binder = AttributeExtensions.GetCustomAttribute<DataBinderAttribute>(propertyInfo);
                            if (binder == null)
                                continue;
                            BindItem bindItem = new BindItem() {
                                propertyInfo = propertyInfo,
                                binder = binder,
                                bindAction = binder.Bind,
                            };
                            list.Add(bindItem);
                        }
                        _list_key.TryAdd(key, list.Count == 0 ? null : list);
                    }
                });
            }
            return list;
        }
        #endregion

        #endregion

        #region types
        /// <summary>
        /// 缓存求值委托。
        /// </summary>
        /// <returns>返回缓存值。</returns>
        protected delegate object CacheValueFunc();
        delegate object BindAction(IDataContext dataContext, System.Data.IDataReader dataReader, object entity, string field, Type type, IDataBinderObjectCache cache);
        class BindItem {
            public System.Reflection.PropertyInfo propertyInfo;
            public DataBinderAttribute binder;
            public BindAction bindAction;

            public void Bind(IDataContext dataContext, System.Data.IDataReader dataReader, object entity, IDataBinderObjectCache cache) {
                if (cache != null && binder.AllowCache) {
                    object value;
                    if (!cache.Get(entity, propertyInfo.Name, out value)) {
                        value = binder.Bind(dataContext, dataReader, entity, propertyInfo.Name, propertyInfo.PropertyType, cache);
                        cache.Set(entity, propertyInfo.Name, value);
                    }
                    propertyInfo.SetValue(entity, value, null);
                } else {
                    var value2 = binder.Bind(dataContext, dataReader, entity, propertyInfo.Name, propertyInfo.PropertyType, cache);
                    propertyInfo.SetValue(entity, value2, null);
                }
            }
        }
        #endregion

    }
}