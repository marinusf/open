using System;

namespace Win32DataMapper.Attributes
{

    /// <summary>
    /// Tells the datacontext that the property is not used to map to the database
    /// </summary>
    [AttributeUsage(
         AttributeTargets.Enum | AttributeTargets.Constructor |
         AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event |
         AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]



    public class DontMap : Attribute
    {
    }
}