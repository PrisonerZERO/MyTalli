namespace My.Talli.Domain.extensions;

using Domain.attributes;
using System;

/// <summary>Extensions</summary>
public static class EnumExtensions
{
    #region <Methods>

    public static string ToStringValue(this Enum value)
    {
        var type = value.GetType();
        var fieldInfo = type.GetField(value.ToString());
        var result = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

        return result != null && result.Length > 0 ? result[0].StringValue : string.Empty;
    }

    #endregion
}