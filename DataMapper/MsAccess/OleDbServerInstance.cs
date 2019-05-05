using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.MsAccess
{
    public class OleDbServerInstance : IServerInstance
    {
        private OleDbConnection _connection;
        private OleDbConnectionStringBuilder _sb;

        public OleDbServerInstance(string path)
        {

            _sb = new OleDbConnectionStringBuilder();
            _sb.Provider = "Provider=Microsoft.Jet.OLEDB.4.0";
            _sb.DataSource = path;

            _connection = new OleDbConnection();

        }

        public OleDbServerInstance(OleDbConnection connection)
        {
            _sb = new OleDbConnectionStringBuilder(connection.ConnectionString);
            if (connection.State != ConnectionState.Open)

                connection.Open();

            _connection = connection;
        }

        public string Server
        {
            get { return _sb.DataSource; }
        }

        public IDbConnection Connection
        {
            get { return _connection; }
            set { _connection = (OleDbConnection)value; }
        }

        public List<string> DbNameList
        {
            get { throw new NotImplementedException(); }
        }

        public void Connect(string password)
        {
            _sb["Jet OLEDB:Database Password"] = password;

            _connection.ConnectionString = _sb.ToString();
            _connection.Open();
        }

        public void Connect(string userId, string password)
        {
            _sb["Jet OLEDB:Database Password"] = password;
            _connection.ConnectionString = _sb.ToString();
            _connection.Open();

        }

        public IServerInstance Clone()
        {
            throw new NotImplementedException();
        }
    }
}
