using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Iuf.Apps.Services.DataAccess
{
    /// <summary>
    /// Provides a base set of methods for performing model based
    /// operations against SQL Server.
    ///
    /// Todo: BulkInsert operations.
    /// </summary>
    public class SqlDataAccess
    {
        /// <summary>
        /// Connection string to the database.
        ///
        /// <para>
        /// Example:
        /// server=iu-fait-sqldev;database=apps;trusted_connection=yes;MultipleActiveResultSets=true
        /// </para>
        /// </summary>
        public string ConnectionString;

        /// <summary>
        /// Command Timeout in seconds.  Default 300 (5m).
        /// </summary>
        public int CommandTimeout = 300;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the first record via a raw SQL query.
        /// <para>Equivilent to Linq's `FirstOrDefault`.</para>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditions"></param>
        /// <param name="commandType"></param>
        /// <returns><typeparam name="T" /> or `default(T)` if not found.</returns>
        public T GetFirst<T>(string sql, object conditions, CommandType commandType = CommandType.Text)
        {
            return GetFirstAsync<T>(sql, conditions, commandType).Result;
        }

        /// <summary>
        /// Gets the first record via a raw SQL query.
        /// <para>Equivilent to Linq's `FirstOrDefault`.</para>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditions"></param>
        /// <param name="commandType"></param>
        /// <returns><typeparam name="T" /> or `default(T)` if not found.</returns>
        public async Task<T> GetFirstAsync<T>(string sql, object conditions, CommandType commandType = CommandType.Text)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                return await conn.QueryFirstOrDefaultAsync<T>(sql, conditions, commandTimeout: CommandTimeout, commandType: commandType);
            }
        }

        /// <summary>
        /// Gets a single record via a raw SQL query.
        /// <para>Equivilent to Linq's `Single`.</para>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditions"></param>
        /// <param name="commandType"></param>
        /// <returns><typeparam name="T" /> or throws an error if not found.</returns>
        public T GetSingle<T>(string sql, object conditions, CommandType commandType = CommandType.Text)
        {
            return GetSingleAsync<T>(sql, conditions, commandType).Result;
        }

        /// <summary>
        /// Gets a single record via a raw SQL query.
        /// <para>Equivilent to Linq's `Single`.</para>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditions"></param>
        /// <param name="commandType"></param>
        /// <returns><typeparam name="T" /> or throws an error if not found.</returns>
        public async Task<T> GetSingleAsync<T>(string sql, object conditions, CommandType commandType = CommandType.Text)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                return await conn.QuerySingleAsync<T>(sql, conditions, commandTimeout: CommandTimeout, commandType: commandType);
            }
        }

        /// <summary>
        /// Returns records from the database.
        /// </summary>
        /// <param name="sql">
        /// <example>select * from users where active = @activeParam</example>
        /// </param>
        /// <param name="commandType"></param>
        public IEnumerable<T> GetList<T>(string sql, CommandType commandType = CommandType.Text)
        {
            return GetListAsync<T>(sql, null, commandType).Result;
        }

        /// <summary>
        /// Returns records from the database.
        /// </summary>
        /// <param name="sql">
        /// <example>select * from users where active = @activeParam</example>
        /// </param>
        /// <param name="commandType"></param>
        public async Task<IEnumerable<T>> GetListAsync<T>(string sql, CommandType commandType = CommandType.Text)
        {
            return await GetListAsync<T>(sql, null, commandType);
        }

        /// <summary>
        /// Returns records from the database.
        /// </summary>
        /// <param name="sql">
        /// <example>select * from users where active = @activeParam</example>
        /// </param>
        /// <param name="parameters">
        /// <example>new { activeParam = true }</example>
        /// </param>
        /// <param name="commandType"></param>
        public IEnumerable<T> GetList<T>(string sql, object parameters, CommandType commandType = CommandType.Text)
        {
            return GetListAsync<T>(sql, parameters, commandType).Result;
        }

        /// <summary>
        /// Returns records from the database.
        /// </summary>
        /// <param name="sql">
        /// <example>select * from users where active = @activeParam</example>
        /// </param>
        /// <param name="parameters">
        /// <example>new { activeParam = true }</example>
        /// </param>
        /// <param name="commandType"></param>
        public async Task<IEnumerable<T>> GetListAsync<T>(string sql, object parameters, CommandType commandType = CommandType.Text)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                return await conn.QueryAsync<T>(sql, parameters, commandTimeout: CommandTimeout, commandType: commandType);
            }
        }

        /// <summary>
        /// Returns the results of a stored procedure call as a list.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <remarks>This is the same as `GetList` except it uses `CommandType.StoredProcedure` when executing.</remarks>
        public IEnumerable<T> GetStoredProcedureList<T>(string procedureName, object parameters = null)
        {
            return GetStoredProcedureListAsync<T>(procedureName, parameters).Result;
        }

        /// <summary>
        /// Returns the results of a stored procedure call as a list.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <remarks>This is the same as `GetList` except it uses `CommandType.StoredProcedure` when executing.</remarks>
        public async Task<IEnumerable<T>> GetStoredProcedureListAsync<T>(string procedureName, object parameters = null)
        {
            return await GetListAsync<T>(procedureName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Create a single record
        /// </summary>
        /// <param name="item"></param>
        public dynamic Create<T>(T item) where T : class
        {
            using (var conn = GetOpenConnection())
            {
                return conn.Insert(item);
            }
        }

        /// <summary>
        /// Inserts multiple records into the database.
        /// Todo: This should be in a transaction.
        /// </summary>
        /// <param name="items"></param>
        public void CreateMultiple<T>(IEnumerable<T> items) where T : class
        {
            using (var conn = GetOpenConnection())
            {
                foreach (var item in items)
                {
                    conn.Insert(item);
                }
            }
        }

        /// <summary>
        /// Updates one record in the database
        /// </summary>
        /// <param name="item"></param>
        public bool Update<T>(T item) where T : class
        {
            using (var conn = GetOpenConnection())
            {
                return conn.Update(item);
            }
        }

        /// <summary>
        /// Updates multiple itemects into the database.
        /// Todo: This should be in a transaction.
        /// </summary>
        /// <param name="items"></param>
        public bool UpdateMultiple<T>(IEnumerable<T> items) where T : class
        {
            using (var conn = GetOpenConnection())
            {
                foreach (var item in items)
                {
                    return conn.Update(item);
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes one record in the database.
        /// </summary>
        /// <param name="item"></param>
        public void Delete(object item)
        {
            using (var conn = GetOpenConnection())
            {
                conn.Delete(item);
            }
        }

        /// <summary>
        /// Executes a sql command.  Use for INSERT, UPDATE, DELETE type commands.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="type">Default: <see cref="CommandType.Text"/></param>
        public int Execute(string sql, object parameters, CommandType type = CommandType.Text)
        {
            using (var conn = GetOpenConnection())
            {
                return conn.Execute(sql, parameters, null, CommandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// Executes a sql command.  Use for INSERT, UPDATE, DELETE type commands.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public async Task<int> ExecuteAsync(string sql, object parameters)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                return await conn.ExecuteAsync(sql, parameters, null, CommandTimeout, CommandType.Text);
            }
        }

        /// <summary>
        /// Executes a stored procedure.  Use for INSERT, UPDATE, DELETE type commands.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        public int ExecuteProcedure(string procedureName, object parameters)
        {
            return ExecuteProcedureAsync(procedureName, parameters).Result;
        }

        /// <summary>
        /// Executes a stored procedure.  Use for INSERT, UPDATE, DELETE type commands.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        public async Task<int> ExecuteProcedureAsync(string procedureName, object parameters)
        {
            using (var conn = await GetOpenConnectionAsync())
            {
                return await conn.ExecuteAsync(procedureName, parameters, null, CommandTimeout, CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Returns an open SqlConnection
        /// </summary>
        public SqlConnection GetOpenConnection()
        {
            try
            {
                dynamic conn = new SqlConnection(ConnectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException e)
            {
                throw new DBException(e.Message, e);
            }
        }

        /// <summary>
        /// Returns an open SqlConnection
        /// </summary>
        public async Task<SqlConnection> GetOpenConnectionAsync()
        {
            try
            {
                dynamic conn = new SqlConnection(ConnectionString);
                await conn.OpenAsync();
                return conn;
            }
            catch (SqlException e)
            {
                throw new DBException(e.Message, e);
            }
        }

        /// <summary>
        /// Connects to the database and runs a select statement to test the connection.
        /// </summary>
        public bool IsDbAvailable()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    var one = conn.QuerySingle<int>("select 1");
                    return one == 1;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }
    }
}