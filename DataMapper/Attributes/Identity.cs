using System;

namespace Win32DataMapper.Attributes
{

    /// <summary>
    /// Tell the datacontext to ignore a property in update or insert operations
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum | AttributeTargets.Constructor |
        AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event |
        AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public class Identity : Attribute
    {
        public bool IgnoreField { get; private set; }

        public Identity(bool ignore)
        {
            IgnoreField = ignore;
        }
    }
}
