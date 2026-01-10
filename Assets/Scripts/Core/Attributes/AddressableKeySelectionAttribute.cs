using System;
using UnityEngine;

namespace Santa.Core.Attributes
{
    /// <summary>
    /// Attribute to display a string field as a dropdown of constant values from a specified class.
    /// Useful for selecting Addressable Keys from a constants file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AddressableKeySelectionAttribute : PropertyAttribute
    {
        public Type ConstantsType { get; private set; }

        public AddressableKeySelectionAttribute(Type constantsType)
        {
            ConstantsType = constantsType;
        }
    }
}
