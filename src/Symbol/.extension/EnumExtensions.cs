﻿/*  
 *  author：symbolspace
 *  e-mail：symbolspace@outlook.com
 */
using System;

/// <summary>
/// Enum扩展类
/// </summary>
public static class EnumExtensions {

    #region methods

    #region HasFlag2
    /// <summary>
    /// 判断current的值是否包含value（逻辑操作： (current &amp; value)==value  ）
    /// </summary>
    /// <param name="current">当前值</param>
    /// <param name="value">判断值</param>
    /// <returns>返回是否包含在内。</returns>
    public static bool HasFlag2(
#if !net20
        this
#endif
        Enum current, Enum value) {
        return HasFlag2(TypeExtensions.Convert<long>(current), TypeExtensions.Convert<long>(value));
    }
    /// <summary>
    /// 判断current的值是否包含value（逻辑操作： (current &amp; value)==value  ）
    /// </summary>
    /// <param name="current">当前值</param>
    /// <param name="value">判断值</param>
    /// <returns>返回是否包含在内。</returns>
    public static bool HasFlag2(
#if !net20
        this
#endif
        Enum current, long value) {
        return HasFlag2(TypeExtensions.Convert<long>(current), value);
    }
    /// <summary>
    /// 判断current的值是否包含value（逻辑操作： (current &amp; value)==value  ）
    /// </summary>
    /// <param name="current">当前值</param>
    /// <param name="value">判断值</param>
    /// <returns>返回是否包含在内。</returns>
    public static bool HasFlag2(
#if !net20
        this
#endif
        long current, Enum value) {
        return HasFlag2(current, TypeExtensions.Convert<long>(value));
    }
    /// <summary>
    /// 判断current的值是否包含value（逻辑操作： (current &amp; value)==value  ）
    /// </summary>
    /// <param name="current">当前值</param>
    /// <param name="value">判断值</param>
    /// <returns>返回是否包含在内。</returns>
    public static bool HasFlag2(
#if !net20
        this
#endif
        long current, long value) {
        return (current & value) == value;
    }
    #endregion

    #region ToValues

    /// <summary>
    /// 将当前枚举的值，变成数组，通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <typeparam name="T">任意枚举类型。</typeparam>
    /// <param name="value">当前枚举值。</param>
    /// <returns>返回一个值的数组。</returns>
    public static T[] ToValues<T>(
#if !net20
        this
#endif
        T value) where T:Enum {
        string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        return TypeExtensions.Convert<T[]>(values);
    }
    #endregion

    #region ToNames
    /// <summary>
    /// 将当前枚举的值，变成名称数组（特性），通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <returns>返回一个值的数组。</returns>
    public static string[] ToNames(
#if !net20
        this
#endif
        Enum value) {
        return ToNames(value, false);
    }
    /// <summary>
    /// 将当前枚举的值，变成名称数组，通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <param name="defineName">是否为定义名称，为false时表示特性名称。</param>
    /// <returns>返回一个值的数组。</returns>
    public static string[] ToNames(
#if !net20
        this
#endif
        Enum value, bool defineName) {
        string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        if (!defineName) {
            var type = value.GetType();
            for (int i = 0; i < values.Length; i++) {
                string name = ConstAttributeExtensions.Const(type.GetField(values[i]));
                if (!string.IsNullOrEmpty(name))
                    values[i] = name;
            }
        }
        return values;
    }
    #endregion

    #region ToName
    /// <summary>
    /// 将当前枚举的值，变成名称串（特性），通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <returns>返回一个值的数组。</returns>
    public static string ToName(
#if !net20
        this
#endif
        Enum value) {
        return ToName(value, false);
    }
    /// <summary>
    /// 将当前枚举的值，变成名称串，通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <param name="defineName">是否为定义名称，为false时表示特性名称。</param>
    /// <returns>返回一个值的数组。</returns>
    public static string ToName(
#if !net20
        this
#endif
        Enum value, bool defineName) {
        string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        if (!defineName) {
            var type = value.GetType();
            for (int i = 0; i < values.Length; i++) {
                string name = ConstAttributeExtensions.Const(type.GetField(values[i]));
                if (!string.IsNullOrEmpty(name))
                    values[i] = name;
            }
        }
        return string.Join(defineName ? "," : "，", values);
    }
    /// <summary>
    /// 将当前枚举的值，变成名称串，通常用于多值的枚举。比如将 Abc.A | Abc.B 变成Abc[]{ Abc.A,Abc.B }。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <param name="key">指定属性名称。</param>
    /// <returns>返回一个值的数组。</returns>
    public static string ToName(
#if !net20
        this
#endif
        Enum value, string key) {
        if (string.IsNullOrEmpty(key))
            key = "Text";

        string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        var type = value.GetType();
        for (int i = 0; i < values.Length; i++) {
            string name = ConstAttributeExtensions.Const(type.GetField(values[i]), key);
            if (!string.IsNullOrEmpty(name))
                values[i] = name;
        }
        return string.Join(key == "Text" ? "," : "，", values);
    }
    #endregion

    #region GetProperty
    /// <summary>
    /// 获取某一个特定的属性值。
    /// </summary>
    /// <param name="value">当前枚举值。</param>
    /// <param name="key">属性名称。</param>
    /// <returns>返回属性的值，未找到时，将是string.Empty。</returns>
    public static string GetProperty(
#if !net20
        this
#endif
        Enum value, string key) {

        if (string.IsNullOrEmpty(key))
            key = "Text";
        string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        var type = value.GetType();
        for (int i = 0; i < values.Length; i++) {
            string name = ConstAttributeExtensions.Const(type.GetField(values[i]), key);
            if (!string.IsNullOrEmpty(name))
                return name;
        }
        return "";
    }
    #endregion

    #endregion

}