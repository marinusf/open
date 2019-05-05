using System;

namespace Win32DataMapper.Attributes
{
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct)]
    public class Table : Attribute
    {
        public Table(string name, string key)
        {
            Name = name;
            Key = key;
        }

        public string Key { get; set; }

        public string Name { get; set; }
    }
}