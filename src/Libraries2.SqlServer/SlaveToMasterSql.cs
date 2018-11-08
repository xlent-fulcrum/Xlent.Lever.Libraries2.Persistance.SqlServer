﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Crud.Model;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Crud.Helpers;
using Xlent.Lever.Libraries2.Crud.Interfaces;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Crud.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    /// <inheritdoc cref="SlaveToMasterSql{TSlaveModelCreate, TSlaveModel,TMasterModel}" />
    public class SlaveToMasterSql<TSlaveModel, TMasterModel> :
        SlaveToMasterSql<TSlaveModel, TSlaveModel, TMasterModel>,
        ICrudSlaveToMaster<TSlaveModel, Guid>
        where TSlaveModel : IUniquelyIdentifiable<Guid>
        where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="masterTableHandler"></param>
        public SlaveToMasterSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName,
            CrudSql<TSlaveModel> slaveTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata, parentColumnName, slaveTableHandler, masterTableHandler)
        {
        }
    }

    /// <inheritdoc cref="ICrudSlaveToMaster{TModelCreate, TModel,TId}" />
    public class SlaveToMasterSql<TSlaveModelCreate, TSlaveModel, TMasterModel> :
        CrudSql<TSlaveModelCreate, TSlaveModel>,
        ICrudSlaveToMaster<TSlaveModelCreate, TSlaveModel, Guid>
            where TSlaveModel : TSlaveModelCreate, IUniquelyIdentifiable<Guid>
            where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        public string ParentColumnName { get; }
        protected CrudSql<TSlaveModelCreate, TSlaveModel> SlaveTableHandler { get; }
        protected CrudSql<TMasterModel> MasterTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SlaveToMasterSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, CrudSql<TSlaveModelCreate, TSlaveModel> slaveTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            SlaveTableHandler = slaveTableHandler;
            MasterTableHandler = masterTableHandler;
        }

        /// <inheritdoc />
        public Task<Guid> CreateAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateAndReturnAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateWithSpecifiedIdAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.ReadAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(SlaveToMasterId<Guid> id, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return SlaveTableHandler.ReadAsync(id.SlaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TSlaveModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TSlaveModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public Task UpdateAsync(Guid masterId, Guid slaveId, TSlaveModel item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.UpdateAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> UpdateAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModel item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.UpdateAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.DeleteAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(Guid parentId, CancellationToken token = default(CancellationToken))
        {
            return DeleteWhereAsync("[{ParentColumnName}] = @ParentId", new { ParentId = parentId }, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<Guid>> ClaimLockAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(Guid masterId, Guid slaveId, Guid lockId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}
