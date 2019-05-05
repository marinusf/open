using Novtel.ORM;
using System.Collections.Generic;
using System.Data.Sql;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.SqlServer
{
    /// <summary>
    ///     Loads all Sql server instances
    /// </summary>
    public class SqlServerContext : IServerContext
    {
        public SqlServerContext()
        {
            var instance =
                SqlDataSourceEnumerator.Instance;
            var table = instance.GetDataSources();


            ServerInstances = new List<IServerInstance>();

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var server = table.Rows[i][0].ToString();
                var type = table.Rows[i][1].ToString();

                if (type != "{}" && type != "")
                {
                    server += "\\" + type;
                }
                ServerInstances.Add(new SqlServerInstance(server));
            }
            // Display the contents of the table.
        }

        /// <summary>
        ///     List of Database Services to connect to and get info from
        /// </summary>
        public List<IServerInstance> ServerInstances { get; set; }
    }
}