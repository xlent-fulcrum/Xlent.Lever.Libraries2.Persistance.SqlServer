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
    public abstract class ManyToMany: TableItem, IManyToMany, IValidatable
    {
        /// <summary>
        /// This field makes it possible to store many different many-to-many relationships in the same table.
        /// </summary>
        /// <remarks>If you only plan to use it for one type of relationship, then set it to <see cref="Guid.Empty"/></remarks>
        public Guid TypeId { get; set; }
        /// <summary>
        /// The Id to one of the two rows of the many-to-many relationship.
        /// </summary>
        /// <remarks>We recommend to refer to the table that comes first alphabetically as a rule of thumb.</remarks>
        public Guid FirstId { get; set; }
        /// <summary>
        /// The Id to the other of the two rows of the many-to-many relationship.
        /// </summary>
        /// <remarks>We recommend to refer to the table that comes last alphabetically as a rule of thumb.</remarks>
        public Guid SecondId { get; set; }
        /// <summary>
        /// This integer should be a consequtive integer serie, starting with 1 within a tuple of <see cref="TypeId"/> and <see cref="FirstId"/>.
        /// </summary>
        public int? FirstSortOrder { get; set; }
        /// <summary>
        /// This integer should be a consequtive integer serie, starting with 1 within a tuple of <see cref="TypeId"/> and <see cref="SecondId"/>.
        /// </summary>
        public int? SecondSortOrder { get; set; }

        /// <inheritdoc />
        public override IEnumerable<string> CustomColumnNames => new[] { "TypeId", "FirstId", "SecondId", "FirstSortOrder", "SecondSortOrder" };

        /// <inheritdoc />
        public override string OrderBy => "FirstId, FirstSortOrder";
    }
}
