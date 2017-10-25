using System;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Logic;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Interfaces;
using Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Models;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Logic
{
    /// <summary>
    /// One table to keep all ManyToMany relations
    /// </summary>
    public abstract class SingleManyToManyHandler<TFirstModel, TFirstTable, TSecondModel, TSecondTable> : ManyToManyHandler<TFirstModel, TFirstTable, ManyToMany, TSecondModel, TSecondTable>
        where TFirstModel : ITableItem, IValidatable, new()
        where TFirstTable : SingleTableHandler<TFirstModel>, IPartInManyToMany<TFirstModel>
        where TSecondModel : ITableItem, IValidatable, new()
        where TSecondTable : SingleTableHandler<TSecondModel>, IPartInManyToMany<TSecondModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="firstTableLogic"></param>
        /// <param name="secondTableLogic"></param>
        /// <param name="typeId"></param>
        protected SingleManyToManyHandler(string connectionString, ISqlTableMetadata tableMetadata, TFirstTable firstTableLogic, TSecondTable secondTableLogic, Guid typeId = default(Guid))
            :base(connectionString, tableMetadata, firstTableLogic, secondTableLogic, typeId)
        {
        }
    }
}
