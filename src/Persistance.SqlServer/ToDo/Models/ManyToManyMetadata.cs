using System;
using System.Collections.Generic;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Interfaces;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Models
{
    /// <summary>
    /// The database columns that are expected in every facade database table
    /// </summary>
    public abstract class ManyToManyMetadata: ISqlTableMetadata
    {
        /// <inheritdoc />
        public abstract string TableName { get; }
        /// <inheritdoc />
        public virtual IEnumerable<string> CustomColumnNames => new[] { "TypeId", "FirstId", "SecondId", "FirstSortOrder", "SecondSortOrder" };
        /// <inheritdoc />
        public virtual string OrderBy => "FirstId, FirstSortOrder";
    }
}
