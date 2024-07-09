using UnityEngine;

public class ConditionalHideAttribute : PropertyAttribute
{
    private readonly string _comparedPropertyName;
    private readonly object _comparedValue;

    public ConditionalHideAttribute(string comparedPropertyName, object comparedValue)
    {
        _comparedPropertyName = comparedPropertyName;
        _comparedValue = comparedValue;
    }
}
