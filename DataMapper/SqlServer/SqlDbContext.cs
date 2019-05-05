using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Win32DataMapper.Attributes;
using Win32DataMapper.Core;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.SqlServer
{
    /// <summary>
    /// works for sql server . but would you really?
    /// </summary>
    public class SqlDbContext : DbContext
    {
        private readonly IServerInstance _instance;

        public SqlDbContext(IServerInstance instance)
        {
            _instance = instance;
        }

        protected override Dictionary<string, object> UpdateObject<T>(IServerInstance instance, T @new, T old)
        {
            var command = instance.Connection.CreateCommand();

            var update = "UPDATE " + base.Table<T>() + " SET ";


            var count = 0;

            var changedPairs = CompareAndChange<T>(@new, old);
            if (changedPairs.Count == 0)
            {
                return null;
            }
            foreach (var changedPair in changedPairs)
            {
                string paramName = string.Empty;


                paramName = "@" + changedPair.Key;

                if (changedPairs.Count - 1 == count)
                {
                    update += changedPair.Key + "=@" + changedPair.Key;
                }
                else
                {
                    update += changedPair.Key + "=@" + changedPair.Key + ",";
                }


                count++;

                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = paramName;
                parameter.Value = changedPair.Value;
                command.Parameters.Add(parameter);


                //command.Parameters.AddWithValue("@" + changedPair.Key, changedPair.Value);
            }
            string where = ""; // = " WHERE " + Key + "=@Key";
            IDbDataParameter whereParam = command.CreateParameter();


            where = " WHERE " + Key<T>() + "=@Key";
            whereParam.ParameterName = "@Key";


            whereParam.Value = @new.GetType().GetProperty(Key<T>()).GetValue(@new);
            command.CommandText = update + where;
            command.Parameters.Add(whereParam);

            command.ExecuteNonQuery();
            return changedPairs;
        }

        public override void Create<T>(T obj)
        {
            InsertObject(_instance, obj);

        }

        public override Dictionary<string, object> Update<T>(T obj)
        {
            PropertyInfo info = obj.GetType().GetProperty(Key<T>());
            object o = info.GetValue(obj);
            T old = Activator.CreateInstance<T>(); ;
            if (o is int)
                old = GetById<T>((int)o);
            else if (o is string)
                old = GetById<T>((string)o);

            return UpdateObject(_instance, obj, old);
        }


        public override int Execute(string query)
        {
            var dbCommand = _instance.Connection.CreateCommand();
            dbCommand.CommandText = query;
            return dbCommand.ExecuteNonQuery();

        }


        public override T GetById<T>(int id)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT * FROM " + Table<T>() + " WHERE " + Key<T>() + "=" + id;

            IDataReader reader = command.ExecuteReader();

            T o = Activator.CreateInstance<T>();


            while (reader.Read())
            {
                MatchTypeToReader(reader, o);
            }

            reader.Close();
            return o;

        }
        public override T GetById<T>(string id)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + Key<T>() + "='" + id + "'";

            IDataReader reader = command.ExecuteReader();

            T o = Activator.CreateInstance<T>();
            while (reader.Read())
            {
                MatchTypeToReader(reader, o);
            }

            reader.Close();
            return o;

        }

        public override T GetById<T>(string val, string field)
        {

            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + field + "='" + val + "'";

            IDataReader reader = command.ExecuteReader();


            T o = Activator.CreateInstance<T>();


            while (reader.Read())
            {
                MatchTypeToReader(reader, o);
            }

            reader.Close();
            return o;
        }

        protected override void InsertObject<T>(IServerInstance instance, T obj)
        {
            var command = instance.Connection.CreateCommand();

            command.CommandText = "INSERT INTO " + Table<T>() + " ";

            string columns = "(", values = " VALUES (";

            var infos = obj.GetType().GetProperties();

            for (var i = 0; i < infos.Length; i++)
            {
                IEnumerable<Identity> fields = infos[i].GetCustomAttributes(false).OfType<Identity>();

                bool ignore = fields.Any(f => f.IgnoreField);

                if (ignore)
                {
                    continue;
                }

                var o = infos[i].GetValue(obj);

                if (o == null) continue;

                if (o is DateTime && (DateTime)o == DateTime.MinValue)
                {
                    continue;
                }

                string paramName = string.Empty;

                paramName = infos[i].Name;

                if (i == infos.Length - 1)
                {
                    columns += "[" + infos[i].Name + "])";
                    values += "@" + infos[i].Name + ")";
                }
                else
                {
                    columns += "[" + infos[i].Name + "],";
                    values += "@" + infos[i].Name + ",";
                }



                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = paramName;

                parameter.Value = o;

                command.Parameters.Add(parameter);
            }

            columns = ReplaceTrailingComma(columns);
            values = ReplaceTrailingComma(values);


            command.CommandText += columns + values;

            command.ExecuteNonQuery();
        }

        public override IEnumerable<T> GetAll<T>()
        {

            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT * FROM " + Table<T>();
            List<T> items = new List<T>();


            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();
                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;
        }

        public override void Delete<T>(int id)
        {
            var command = _instance.Connection.CreateCommand();
            command.CommandText = "DELETE  FROM " + Table<T>() + " WHERE " + Key<T>() + "=" + id;
            command.ExecuteNonQuery();


        }

        public override void Dispose()
        {
            if (_instance.Connection != null && _instance.Connection.State != ConnectionState.Closed)
                _instance.Connection.Close();

        }

        public override IEnumerable<T> GetAll<T>(string id)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + Key<T>() + "='" + id + "'";
            List<T> items = new List<T>();
            //have to use clone connection because im creating a new instace of PsqlContext inside LoadMappings



            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();
                MatchTypeToReader(reader, item);
                items.Add(item);
            }

            reader.Close();
            return items;
        }

        public override IEnumerable<T> GetAll<T>(int id)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + Key<T>() + "=" + id;
            List<T> items = new List<T>();

            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();

                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;
        }

        public override IEnumerable<T> GetAll<T>(int id, string field)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + field + "=" + id;
            List<T> items = new List<T>();

            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();
                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;

        }

        public override IEnumerable<T> GetAll<T>(string id, string field)
        {

            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT  " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + field + "='" + id + "'";
            List<T> items = new List<T>();

            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();

                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;

        }

        public override IEnumerable<T> GetAll<T>(string id, string field, string andQuery)
        {
            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + field + "='" + id + "' " + andQuery;
            List<T> items = new List<T>();

            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                T item = Activator.CreateInstance<T>();

                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;
        }

        public override IEnumerable<T> GetAll<T>(int id, string field, string andQuery)
        {

            var command = _instance.Connection.CreateCommand();

            command.CommandText = "SELECT " + GetTypeSelectStatementColumns<T>() + " FROM " + Table<T>() + " WHERE " + field + "=" + id + " " + andQuery;
            List<T> items = new List<T>();

            IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {


                T item = Activator.CreateInstance<T>();
                MatchTypeToReader(reader, item);

                items.Add(item);
            }

            reader.Close();
            return items;
        }

        public override void LoadMappings<T>(T item)
        {
            foreach (PropertyInfo info in item.GetType().GetProperties())
            {

                foreach (var attribute in info.CustomAttributes)
                {
                    if (attribute.AttributeType == typeof(Mapping))
                    {
                        IEnumerable<Mapping> links = info.GetCustomAttributes().OfType<Mapping>();

                        Mapping link = links.First();

                        string fieldFrom = link.FieldFrom;
                        string fieldTo = link.FieldTo;
                        string andQuery = link.AndQuery;


                        object value = item.GetType().GetProperty(fieldFrom)?.GetValue(item);
                        object propertyClass = Activator.CreateInstance(info.PropertyType);


                        CreateMappings(item, propertyClass, this, andQuery, value, fieldTo, info, _instance);
                    }
                }
            }
        }
    }
}