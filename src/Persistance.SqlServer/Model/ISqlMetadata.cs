using System.Collections.Generic;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statmements
    /// </summary>
    public interface ISqlMetadata
    {
        /// <summary>
        /// The name of the database table that will hold this type of items.
        /// </summary>
        /// <remarks>Will be surrounded with dbo.[{TableName}}, i.e. just specify the name, no brackets needed.</remarks>
        string TableName { get; }

        /// <summary>
        /// The list of columns that you have added, not including the columns in <see cref="IStorableItem{TId}"/> or in <see cref="ITimeStamped"/>.
        /// </summary>
        IEnumerable<string> CustomColumnNames { get; }

        /// <summary>
        /// Will be used as "ORDER BY {OrderBy}" when selecting multiple rows from the table.
        /// </summary>
        string OrderBy { get; }
    }
}