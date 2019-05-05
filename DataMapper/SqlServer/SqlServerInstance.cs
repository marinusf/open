using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.SqlServer
{
    /// <summary>
    ///     Represents a connectable SQL server instance
    /// </summary>

    public class SqlServerInstance : IServerInstance
    {
        private readonly SqlConnectionStringBuilder _sb;


        public SqlServerInstance(string server)
        {
            _sb = new SqlConnectionStringBuilder();
            _sb.DataSource = server;
        }

        public SqlServerInstance(SqlConnection connection)
        {
            _sb = new SqlConnectionStringBuilder(connection.ConnectionString);
            if (connection.State != ConnectionState.Open)

                connection.Open();

            Connection = connection;

        }


        public string Server
        {
            get { return _sb.DataSource; }
        }

        public IDbConnection Connection { get; set; }

        public List<string> DbNameList { get; private set; }

        /// <summary>
        ///     Connects to the Database and loads this class
        /// </summary>
        /// <param name="password"></param>
        public void Connect(string password)
        {
            _sb.Password = password;
            _sb.UserID = "sa";
            Connection = new SqlConnection(_sb.ToString());
            Connection.Open();
            LoadDbList();
        }

        public void Connect(string userId, string password)
        {
            _sb.UserID = userId;
            _sb.Password = password;
            Connection = new SqlConnection(_sb.ToString());
            Connection.Open();
            LoadDbList();
        }

        public IServerInstance Clone()
        {
            IServerInstance instance = new SqlServerInstance(Server);
            instance.Connect(_sb.UserID, _sb.Password);
            instance.Connection.ChangeDatabase(Connection.Database);
            return instance;
        }

        private void LoadDbList()
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT name FROM master.dbo.sysdatabases";

            var reader = command.ExecuteReader();

            DbNameList = new List<string>();
            while (reader.Read())
            {
                DbNameList.Add(reader["Name"].ToString());
            }
            reader.Close();
        }
    }
}