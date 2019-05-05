using System.Collections.Generic;
using System.Data;
using DTOLib2;
using Pervasive.Data.SqlClient;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.PervasiveSql
{
    public class PsqlServerInstance : IServerInstance
    {
        private readonly PsqlConnectionStringBuilder _sb;


        public PsqlServerInstance(string server)
        {
            _sb = new PsqlConnectionStringBuilder();
            _sb.Host = server;
        }

        public PsqlServerInstance(PsqlConnection connection)
        {
            _sb = new PsqlConnectionStringBuilder(connection.ConnectionString);
            if (connection.State != ConnectionState.Open)

                connection.Open();

            Connection = connection;
        }
        public string Server
        {
            get { return _sb.Host; }
        }

        public IDbConnection Connection { get; set; }

        public List<string> DbNameList
        {
            get;
            private set;
        }

        public void Connect(string password)
        {
            _sb.Password = password;
            Connection = new PsqlConnection(_sb.ToString());
            Connection.Open();
            LoadDbList();
        }

        public void Connect(string userId, string password)
        {
            _sb.UserID = "";
            _sb.Password = "";
            Connection = new PsqlConnection(_sb.ToString());
            Connection.Open();
            LoadDbList();

        }

        public IServerInstance Clone()
        {

            IServerInstance instance = new PsqlServerInstance(Server);
            instance.Connect(_sb.UserID, _sb.Password);
            instance.Connection.ChangeDatabase(Connection.Database);
            return instance;
        }

        private void LoadDbList()
        {
            DbNameList = new List<string>();

            DtoSession session = new DtoSession();
            session.Connect(_sb.Host, _sb.UserID, _sb.Password);
            foreach (DtoDatabase dtoDatabase in session.Databases)
            {

                DbNameList.Add(dtoDatabase.Name);
            }
        }
    }
}
