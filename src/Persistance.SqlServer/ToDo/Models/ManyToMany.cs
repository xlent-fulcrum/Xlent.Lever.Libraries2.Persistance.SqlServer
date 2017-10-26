using System;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Interfaces;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Models
{
    /// <summary>
    /// The database columns that are expected in every facade database table
    /// </summary>
    public class ManyToMany: TableItem, IManyToMany
    {
        /// <inheritdoc />
        public Guid TypeId { get; set; }
        /// <inheritdoc />
        public Guid FirstId { get; set; }
        /// <inheritdoc />
        public Guid SecondId { get; set; }
        /// <inheritdoc />
        public int? FirstSortOrder { get; set; }
        /// <inheritdoc />
        public int? SecondSortOrder { get; set; }
    }
}
