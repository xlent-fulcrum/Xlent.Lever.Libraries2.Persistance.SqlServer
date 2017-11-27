using System.Data.SqlClient;

namespace Xlent.Lever.Libraries2.SqlServer.Logic
{
    /// <summary>
    /// Base class for common Database knowledge
    /// </summary>
    public abstract class Database
    {
        private readonly string _connectionString;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="connectionString">How to connect to the database.</param>
        protected Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Get a new SQL Connection
        /// </summary>
        /// <returns>A new <see cref="SqlConnection"/></returns>
        protected virtual SqlConnection NewSqlConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
