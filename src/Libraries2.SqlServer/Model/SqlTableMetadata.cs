using System.Collections.Generic;

namespace Xlent.Lever.Libraries2.SqlServer.Model
{
    public class SqlTableMetadata : ISqlTableMetadata
    {
        /// <inheritdoc />
        public string TableName { get; set; }
        /// <inheritdoc />
        public string ForeignKeyColumnName { get; set; }
        /// <inheritdoc />
        public string CreatedAtColumnName { get; set; }
        /// <inheritdoc />
        public string UpdatedAtColumnName { get; set; }
        /// <inheritdoc />
        public IEnumerable<string> CustomColumnNames { get; set; }
        /// <inheritdoc />
        public string OrderBy(string columnPrefix = null)
        {
            throw new System.NotImplementedException();
        }
    }
}