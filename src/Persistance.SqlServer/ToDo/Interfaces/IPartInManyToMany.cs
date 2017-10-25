using System;
using System.Threading.Tasks;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Interfaces
{
    /// <summary>
    /// The database columns that are expected in every facade database table
    /// </summary>
    public interface IPartInManyToMany<T>
    {
        /// <summary>
        /// Update the primary id.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newPrimaryId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        Task<T> UpdateForeignKey(T item, Guid newPrimaryId, Guid typeId = default(Guid));

        /// <summary>
        /// Get the name of the foreign key that points out the other table item in a many-to-many relationship. Return null if this is a one-to-many relationship, so no foregin key exists in this table.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        string NameOfForeignKey(Guid typeId);
    }
}
