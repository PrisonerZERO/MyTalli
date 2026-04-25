namespace My.Talli.Domain.attributes;

using System;

/// <summary>
/// An attribute used extract the string-value from an Enum
/// </summary>
public class StringValueAttribute : Attribute
{
    #region Constructor

    public StringValueAttribute(string value)
    {
        StringValue = value;
    }

    #endregion

    #region <Properties>

    public string StringValue { get; protected set; }

    #endregion
}