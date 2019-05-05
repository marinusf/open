/*
 * Works with Pastel and Novtel type database 
 * Classes are used to map to database tables
 * See UnitTest1 for examples.
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Win32DataMapper.Attributes;
using Win32DataMapper.Interfaces;

namespace Win32DataMapper.Core
{
    /// <summary>
    /// A data layer that manages a object with the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DbContext : IDisposable
    {


        /// <summary>
        /// An attribute of Table("TableName") above you class 
        /// </summary>
        protected virtual string Table<T>()
        {

            try
            {
                return ((Table)typeof(T).GetCustomAttributes(typeof(Table)).ToList()[0]).Name;
            }
            catch
            {
                throw new Exception("Class " + typeof(T).Name + " has no Table attribute assigned");
            }

        }

        /// <summary>
        /// An attribute of Table("TableName","Key") above your class
        /// </summary>
        protected virtual string Key<T>()
        {
            return ((Table)typeof(T).GetCustomAttributes(typeof(Table)).ToList()[0]).Key;
        }

        /// <summary>
        /// Inserts a object to a database
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="obj"></param>
        protected abstract void InsertObject<T>(IServerInstance instance, T obj);

        protected string ReplaceTrailingComma(string s)
        {
            if (s.EndsWith(","))
            {
                s = s.TrimEnd(',');
                s += ")";

            }
            return s;
        }


        /// <summary>
        /// Compares two objects and updates the record in the database
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="new">The new changed object</param>
        /// <param name="old">The old object to compare and be changed and updated</param>
        /// <returns></returns>
        protected abstract
            Dictionary<string, object> UpdateObject<T>(IServerInstance instance, T @new, T old);
        /// <summary>
        /// Executes a sql statement on the database.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns rows affected</returns>
        public abstract int Execute(string query);



        protected virtual string GetTypeSelectStatementColumns<T>()
        {
            string select = "";
            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                if (info.GetCustomAttributes().OfType<Mapping>().Any() || info.GetCustomAttributes().OfType<DontMap>().Any()) continue;

                select += info.Name + ",";


            }
            select = select.TrimEnd(',');

            return select;
        }

        protected virtual Dictionary<string, object> CompareAndChange<T>(object objComp, object objChange)
        {
            var changes = new Dictionary<string, object>();
            var old = objChange.GetType();
            var @new = objComp.GetType();


            foreach (var propertyInfo in old.GetProperties())
            {
                IEnumerable<Identity> fields = propertyInfo.GetCustomAttributes(false).OfType<Identity>();

                if (fields.Any(x => x.IgnoreField)) continue;

                var newValue = @new.GetProperty(propertyInfo.Name)?.GetValue(objComp);
                var oldValue = old.GetProperty(propertyInfo.Name)?.GetValue(objChange);

                if (newValue == null)
                {
                    old.GetProperty(propertyInfo.Name)?.SetValue(objChange, newValue);
                    continue;

                }
                if (!newValue.Equals(oldValue))
                {
                    if (newValue is DateTime)
                    {
                        if ((DateTime)newValue == DateTime.MinValue && oldValue == null) continue;

                        var seconds = ((DateTime)newValue - (DateTime)oldValue).Seconds;
                        if (seconds == 0)
                        {
                            continue;
                        }
                    }


                    old.GetProperty(propertyInfo.Name).SetValue(objChange, newValue);
                    changes.Add(propertyInfo.Name, newValue);
                }
            }

            return changes;
        }
        /// <summary>
        /// Binds your object from a datareader, used for GetById
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        protected virtual void MatchTypeToReader<T>(IDataReader reader, T obj)
        {

            for (var i = 0; i < reader.FieldCount; i++)
            {

                var info = obj.GetType().GetProperty(reader.GetName(i));


                if (info == null) continue;
                if (info.GetCustomAttributes().OfType<Mapping>().Any())
                    continue;
                info.SetValue(obj, reader[i] == DBNull.Value ? null : reader[i]);
            }

        }

        protected virtual void CreateMappings<T>(T item, object linkProperty, object context, string andQuery,
            object value, string fieldTo, PropertyInfo info, IServerInstance instance)
        {
            if (linkProperty.GetType().Name.Contains("List"))
            {
                Type argument = linkProperty.GetType().GetGenericArguments()[0];



                object objReturned = null;



                object[] parameters;

                Type[] types = null;

                if (andQuery == null)
                {
                    if (value is int)
                    {
                        types = new[] { typeof(int), typeof(string) };

                    }
                    else
                    {
                        types = new[] { typeof(string), typeof(string) };
                    }

                    parameters = new[] { value, fieldTo };
                }
                else
                {
                    if (value is int)
                    {
                        types = new[] { typeof(int), typeof(string), typeof(string) };
                    }
                    else
                    {
                        types = new[] { typeof(string), typeof(string), typeof(string) };
                    }
                    parameters = new[] { value, fieldTo, andQuery };

                }



                if (value is int)
                {
                    objReturned =
                   context.GetType().GetMethod("GetAll", types).MakeGenericMethod(argument)
                                              .Invoke(context, parameters);
                }
                if (value is string)
                {
                    var genericMethod = context.GetType()
                        .GetMethod("GetAll", types).MakeGenericMethod(argument);

                    objReturned =
                        genericMethod
                            .Invoke(context, parameters);
                }

                info.SetValue(item, objReturned);
            }
            else
            {



                object objReturned = null;

                if (value is int)
                    objReturned = context.GetType()
                        .GetMethod("GetById", new[] { typeof(int) }).MakeGenericMethod(linkProperty.GetType())
                        .Invoke(context, new[] { value });
                if (value is string)
                    objReturned = context.GetType()
                        .GetMethod("GetById", new[] { typeof(string) }).MakeGenericMethod(linkProperty.GetType())
                        .Invoke(context, new[] { value });

                info.SetValue(item, objReturned);
            }
        }

        public abstract void Create<T>(T obj);
        /// <summary>
        /// updates object on Table.Key value attribute
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> Update<T>(T obj);


        /// <summary>
        ///<para>Lazy Loads properties with the Link attribute of object.</para>
        /// <para>This method creates an instance of DbContext and calls the getAll</para>
        /// <para>or getById in the context so it can expensive and i chose to remove it being called automaticly and went for a lazy loading instead.</para>
        /// <para>Use context.LoadMapping(T) to load i.e Customer.Addresses  if a mapping was made between the two classes </para>
        /// </summary>
        /// <param name="item"></param>
        public abstract void LoadMappings<T>(T item);
        public abstract T GetById<T>(int id);
        public abstract T GetById<T>(string id);

        public abstract T GetById<T>(string val, string field);
        /// <summary>
        /// searches with the key of Table attribute of the class 
        /// </summary>
        ///
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>();
        /// <summary>
        /// searches with the key of Table attribute of the class 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(string id);

        /// <summary>
        /// searches with the key of Table attribute of the class 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(int id);

        /// <summary>
        /// searches with the specified field 
        /// </summary>
        /// <param name="id">Id to search with</param>
        /// <param name="field">Field of the table to search on</param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(int id, string field);
        /// <summary>
        /// searches with the specified field 
        /// </summary>
        /// <param name="id">Id to search with</param>
        /// <param name="field">Field of the table to search on</param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(string id, string field);

        /// <summary>
        /// searches with the specified field and a AND clause in SQL statement
        /// </summary>
        /// <param name="id">Id to search with</param>
        /// <param name="field">Field in table to search with</param>
        /// <param name="andQuery">And query that can be used (Must start with AND for example ' AND name=value'</param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(string id, string field, string andQuery);
        /// <summary>
        /// searches with the specified field and a AND clause in SQL statement
        /// </summary>
        /// <param name="id">Id to search with</param>
        /// <param name="field">Field in table to search with</param>
        /// <param name="andQuery">And query that can be used (Must start with AND for example ' AND name=value'</param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetAll<T>(int id, string field, string andQuery);


        public abstract void Delete<T>(int id);
        /// <summary>
        /// Closes the connection and disposes it
        /// </summary>
        public abstract void Dispose();



    }
}