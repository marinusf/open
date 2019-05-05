using System.Collections.Generic;
using Win32DataMapper.Interfaces;

namespace Novtel.ORM
{
    public interface IServerContext
    {
        List<IServerInstance> ServerInstances { get; set; }
    }
}