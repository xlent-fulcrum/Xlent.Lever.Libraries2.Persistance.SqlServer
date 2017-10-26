using System.Collections.Generic;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statmements
    /// </summary>
    public interface ISqlTableMetadata
    {
        /// <summary>
        /// The name of the database table that will hold this type of items.
        /// </summary>
        /// <remarks>Will be surrounded with dbo.[{TableName}}, i.e. just specify the name, no brackets needed.</remarks>
        string TableName { get; }

        /// <summary>
        /// The name of column that has the foreign key or null.
        /// </summary>
        string ForeignKeyColumnName { get; }

        /// <summary>
        /// The name of column that has the time stamp for when the record was created.
        /// </summary>
        string CreatedAtColumnName { get; }

        /// <summary>
        /// The name of column that has the time stamp for when the record was last updated.
        /// </summary>
        string UpdatedAtColumnName { get; }

        /// <summary>
        /// The list of columns that you have added, not including the columns in <see cref="IStorableItem{TId}"/> or in <see cref="ITimeStamped"/>.
        /// </summary>
        IEnumerable<string> CustomColumnNames { get; }

        /// <summary>
        /// Will be used as "ORDER BY {OrderBy}" when selecting multiple rows from the table.
        /// </summary>
        string OrderBy(string columnPrefix = null);
    }
}