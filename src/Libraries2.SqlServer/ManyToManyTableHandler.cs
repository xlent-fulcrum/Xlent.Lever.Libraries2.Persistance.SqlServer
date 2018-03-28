using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ManyToManyTableHandler<TManyToManyModel, TReferenceModel1, TReferenceModel2> : SimpleTableHandler<TManyToManyModel>, IManyToManyRelation<TReferenceModel1, TReferenceModel2, Guid>
        where TManyToManyModel : class, ITableItem, IValidatable
        where TReferenceModel1 : ITableItem, IValidatable
        where TReferenceModel2 : ITableItem, IValidatable
    {
        public ManyToOneTableHandler<TManyToManyModel, TReferenceModel1> OneTableHandler1 { get; }
        public ManyToOneTableHandler<TManyToManyModel, TReferenceModel2> OneTableHandler2 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName1"></param>
        /// <param name="referenceHandler1"></param>
        /// <param name="groupColumnName2"></param>
        /// <param name="referenceHandler2"></param>
        public ManyToManyTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName1, SimpleTableHandler<TReferenceModel1> referenceHandler1, string groupColumnName2, SimpleTableHandler<TReferenceModel2> referenceHandler2)
            : base(connectionString, tableMetadata)
        {
            OneTableHandler1 = new ManyToOneTableHandler<TManyToManyModel, TReferenceModel1>(connectionString, tableMetadata, groupColumnName1, referenceHandler1);
            OneTableHandler2 = new ManyToOneTableHandler<TManyToManyModel, TReferenceModel2>(connectionString, tableMetadata, groupColumnName2, referenceHandler2);
        }

        #region The reference table (the many-to-many table)
        /// <summary>
        /// Get the item that has the specified <paramref name="reference1Id"/> and <paramref name="reference2Id"/>.
        /// </summary>
        public async Task<TManyToManyModel> Read(Guid reference1Id, Guid reference2Id)
        {
            var param = new { Reference1Id = reference1Id, Reference2Id = reference2Id};
            return await SearchWhereSingle($"{OneTableHandler1.ParentColumnName} = @Reference1Id AND {OneTableHandler2.ParentColumnName}= @Reference2Id", param);
        }

        /// <summary>
        /// Find all many-to-many items that has foreign key 1 set to <paramref name="id"/>.
        /// </summary>
        public async Task<PageEnvelope<TManyToManyModel>> ReadByReference1WithPaging(Guid id, int offset = 0, int? limit = null)
        {
            return await OneTableHandler1.ReadChildrenWithPagingAsync(id, offset, limit);
        }

        /// <summary>
        /// Find all many-to-many items that has foreign key 1 set to <paramref name="id"/>.
        /// </summary>
        public async Task<IEnumerable<TManyToManyModel>> ReadByReference1(Guid id, int limit = int.MaxValue)
        {
            return await StorageHelper.ReadPages(offset => ReadByReference1WithPaging(id, offset));
        }

        /// <summary>
        /// Find all many-to-many items that has foreign key 2 set to <paramref name="id"/>.
        /// </summary>
        public async Task<PageEnvelope<TManyToManyModel>> ReadByReference2WithPaging(Guid id, int offset = 0, int? limit = null)
        {
            return await OneTableHandler2.ReadChildrenWithPagingAsync(id, offset, limit);
        }

        /// <summary>
        /// Find all many-to-many items that has foreign key 1 set to <paramref name="id"/>.
        /// </summary>
        public async Task<IEnumerable<TManyToManyModel>> ReadByReference2(Guid id, int limit = int.MaxValue)
        {
            return await StorageHelper.ReadPages(offset => ReadByReference2WithPaging(id, offset));
        }

        /// <inheritdoc />
        public async Task DeleteReferencesByReference1(Guid id)
        {
            await OneTableHandler1.DeleteChildrenAsync(id);
        }

        /// <inheritdoc />
        public async Task DeleteReferencesByReference2(Guid id)
        {
            await OneTableHandler2.DeleteChildrenAsync(id);
        }
        #endregion

        #region The referenced tables

        /// <inheritdoc />
        public async Task<PageEnvelope<TReferenceModel2>> ReadReferencedItemsByReference1(Guid id, int offset = 0, int? limit = null)
        {
            return await OneTableHandler2.ReadAllParentsInGroupAsync(
                OneTableHandler1.ParentColumnName,
                id, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TReferenceModel1>> ReadReferencedItemsByReference2(Guid id, int offset = 0, int? limit = null)
        {
            return await OneTableHandler1.ReadAllParentsInGroupAsync(
                OneTableHandler2.ParentColumnName,
                id, offset, limit);
        }

        #endregion
    }
}
