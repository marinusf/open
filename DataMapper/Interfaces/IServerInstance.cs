using System.Collections.Generic;
using System.Data;

namespace Win32DataMapper.Interfaces
{
    public interface IServerInstance
    {
        string Server { get; }
        IDbConnection Connection { get; set; }
        List<string> DbNameList { get; }

        /// <summary>
        ///     Connects to the Database and loads this class
        /// </summary>
        /// <param name="password"></param>
        void Connect(string password);

        void Connect(string userId, string password);

        IServerInstance Clone();
    }
}