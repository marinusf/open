using System;

namespace Win32DataMapper.Attributes
{
    [AttributeUsage(
        AttributeTargets.Enum | AttributeTargets.Constructor |
        AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event |
        AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    /// <summary>
    /// Specify how this property links to an one-to-many child class  
    /// </summary>                                                                               
    public class Mapping : Attribute
    {

        /// <summary>
        /// Specify how this property links to an one-to-many child class  
        /// </summary>
        /// <param name="fieldFrom">Field linking to FieldTo</param>
        /// <param name="fieldTo">Field link from FieldFrom</param>
        /// <param name="andQuery">Always start with AND For example 'AND Field='Value'</param>

        public Mapping(string fieldFrom, string fieldTo, string andQuery)
        {

            AndQuery = andQuery;
            FieldFrom = fieldFrom;
            FieldTo = fieldTo;
        }
        public Mapping(string fieldFrom, string fieldTo)
        {

            FieldFrom = fieldFrom;
            FieldTo = fieldTo;
        }
        public string FieldFrom { get; set; }

        public string FieldTo { get; set; }
        /// <summary>
        ///Always start with AND For example 'AND Field='Value'
        /// </summary>
        public string AndQuery { get; set; }

    }



}