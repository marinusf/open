using System;
using System.Collections.Generic;
using GongSolutions.Shell;
using GongSolutions.Shell.Interop;
using Novtel.ORM;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.PervasiveSql
{
    public class PsqlServerContext : IServerContext
    {
        /// <summary>
        /// Loads all the available Pervasive connections
        /// </summary>
        public PsqlServerContext()
        {

            //loads all computers on network as pervasive engine pcc.exe connects to computer name instead of ip
            ShellItem folder = new ShellItem((Environment.SpecialFolder)CSIDL.NETWORK);
            IEnumerator<ShellItem> e = folder.GetEnumerator(SHCONTF.FOLDERS);
            ServerInstances = new List<IServerInstance>();
            while (e.MoveNext())
            {
                //TODO:add testing to see if valid pervasive engine is running on target network computer by trying to connect first
                ServerInstances.Add(new PsqlServerInstance(e.Current.DisplayName));
            }


        }
        /// <summary>
        /// List of pervasive possible engines
        /// </summary>
        public List<IServerInstance> ServerInstances
        {

            get;
            set;
        }
    }
}


