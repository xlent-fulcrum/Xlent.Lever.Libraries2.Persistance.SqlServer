using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Logic;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Interfaces;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Logic
{
    /// <summary>
    /// Many to many table
    /// </summary>
    /// <typeparam name="TFirstModel"></typeparam>
    /// <typeparam name="TFirstTable"></typeparam>
    /// <typeparam name="TManyToManyModel"></typeparam>
    /// <typeparam name="TSecondModel"></typeparam>
    /// <typeparam name="TSecondTable"></typeparam>
    public abstract class ManyToManyHandler<TFirstModel, TFirstTable, TManyToManyModel, TSecondModel, TSecondTable> : SingleTableHandler<TManyToManyModel>
        where TFirstModel : ITableItem, IValidatable, new()
        where TFirstTable : SingleTableHandler<TFirstModel>, IPartInManyToMany<TFirstModel>
        where TManyToManyModel : IManyToMany, IValidatable, new()
        where TSecondModel : ITableItem, IValidatable, new()
        where TSecondTable : SingleTableHandler<TSecondModel>, IPartInManyToMany<TSecondModel>
    {
        private static readonly string Namespace = typeof(ManyToManyHandler<TFirstModel, TFirstTable, TManyToManyModel, TSecondModel, TSecondTable>).Namespace;
        private readonly TFirstTable _firstTableLogic;
        private readonly TSecondTable _secondTableLogic;
        private readonly Guid _typeId;
        private readonly TFirstModel _firstModel;
        private readonly TSecondModel _secondModel;
        private readonly TManyToManyModel _manyToManyModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="firstTableLogic"></param>
        /// <param name="secondTableLogic"></param>
        /// <param name="typeId"></param>
        protected ManyToManyHandler(string connectionString, ISqlTableMetadata tableMetadata, TFirstTable firstTableLogic, TSecondTable secondTableLogic, Guid typeId = default(Guid))
            : base(connectionString, tableMetadata)
        {
            _firstTableLogic = firstTableLogic;
            _secondTableLogic = secondTableLogic;
            _typeId = typeId;
            _firstModel = new TFirstModel();
            _secondModel = new TSecondModel();
            _manyToManyModel = new TManyToManyModel();
        }

        private object FirstOrSecond(bool first)
        {
            return first ? "First" : "Second";
        }

        #region READ
        /// <summary>
        /// Read the highest number for a sort order.
        /// </summary>
        /// <param name="first">True means to get max for FirstSortOrder, false means SecondSortOrder</param>
        /// <param name="typeId">The id for the type of many to many relation that we want the max for.</param>
        /// <returns>The maximum number for the specified sort order.</returns>
        public virtual async Task<int> ReadMaxSortOrderAsync(bool first, Guid typeId = default(Guid))
        {
            var firstOrSecond = FirstOrSecond(first);
            var ignored = 0;
            var param = new DynamicParameters();
            param.Add("FirstOrSecondId", typeId);
            param.Add("TypeId", _typeId);
            param.Add("MaxValue", ignored, null, ParameterDirection.Output);
            await SearchAdvancedSingleAsync($"SELECT @MaxValue=MAX({firstOrSecond}SortOrder) FROM [{TableMetadata.TableName}] WHERE [{firstOrSecond}Id] = @FirstOrSecondId AND TypeId = @TypeId", param);
            return param.Get<int?>("MaxValue") ?? 0;
        }

        /// <summary>
        /// Get the item that has the specified <paramref name="firstId"/> and <paramref name="secondId"/>.
        /// </summary>
        public async Task<TManyToManyModel> ReadByFirstIdAndSecondIdAsync(Guid firstId, Guid secondId)
        {
            var param = new { FirstId = firstId, SecondId = secondId, TypeId = _typeId };
            return await SearchWhereSingle("FirstId = @FirstId AND SecondId = @SecondId AND TypeId = @TypeId", param);
        }

        #endregion

        #region CREATE
        /// <summary>
        /// Create a relationship a new relationship.
        /// </summary>
        /// <remarks>For the sort orders null means last, any other number is absolute 
        /// unless the number is larger than the current number of items, then that means last.</remarks>
        public override async Task<TManyToManyModel> CreateAsync(TManyToManyModel item)
        {
            item.FirstSortOrder = await MakeRoomForSortOrderAsync(true, item.FirstId, item.FirstSortOrder);
            item.SecondSortOrder = await MakeRoomForSortOrderAsync(true, item.SecondId, item.SecondSortOrder);
            item = await base.CreateAsync(item);
            await MaybeUpdatePrimaryIdAsync(item);
            return item;
        }

        private async Task MaybeUpdatePrimaryIdAsync(TManyToManyModel item)
        {
            // Maybe update PrimaryId
            if (item.FirstSortOrder == 1)
            {
                var existing = await _secondTableLogic.ReadAsync(item.SecondId);
                FulcrumAssert.IsNotNull(existing, $"{Namespace}: AAB0ED64-7A98-4F75-AC71-E84D3E803B04", $"Expected item {item.SecondId} to exist in table {_secondTableLogic.TableName}.");
                if (_secondTableLogic.NameOfForeignKey(_typeId) != null)
                {
                    await _secondTableLogic.UpdateForeignKey(existing, item.FirstId, _typeId);
                }
            }
            if (item.SecondSortOrder == 1)
            {
                var existing = await _firstTableLogic.ReadAsync(item.FirstId);
                FulcrumAssert.IsNotNull(existing, $"{Namespace}: AC32BD77-4188-4E0C-AB88-4844B1B9216F", $"Expected item {item.FirstId} to exist in table {_firstTableLogic.TableName}.");
                if (_firstTableLogic.NameOfForeignKey(_typeId) != null)
                {
                    await _firstTableLogic.UpdateForeignKey(existing, item.SecondId, _typeId);
                }
            }
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// Update the data for a item
        /// </summary>
        /// <param name="first">Is it the first sort order that we should update? False means update second sort order</param>
        /// <param name="firstId">The id for the first item.</param>
        /// <param name="secondId">The id for the second item.</param>
        /// <param name="newSortOrder">Where should the item be put in the sort order. null means last.</param>
        /// <returns>The updated item.</returns>
        public async Task<TManyToManyModel> UpdateSortOrderAsync(bool first, Guid firstId, Guid secondId, int? newSortOrder = null)
        {
            var doUpdate = false;
            var doCreate = false;
            var item = await ReadByFirstIdAndSecondIdAsync(firstId, secondId);
            if (item == null)
            {
                doCreate = true;
                item = new TManyToManyModel
                {
                    TypeId = _typeId,
                    FirstId = firstId,
                    SecondId = secondId,
                    FirstSortOrder =
                        await MakeRoomForSortOrderAsync(true, firstId,
                            first ? newSortOrder : null),
                    SecondSortOrder =
                        await MakeRoomForSortOrderAsync(false, secondId,
                            first ? null : newSortOrder)
                };
            }
            else //if (newSortOrder != null)
            {
                if (first)
                {
                    if (item.FirstSortOrder != newSortOrder)
                    {
                        Debug.Assert(item.FirstSortOrder != null, "item.FirstSortOrder != null");
                        var currentValue = item.FirstSortOrder.Value;
                        item.FirstSortOrder = await MakeRoomForSortOrderAsync(true, item.FirstId, newSortOrder, currentValue);
                        doUpdate = true;
                    }
                }
                else
                {
                    if (item.SecondSortOrder != newSortOrder)
                    {
                        Debug.Assert(item.SecondSortOrder != null, "item.SecondSortOrder != null");
                        var currentValue = item.SecondSortOrder.Value;
                        item.SecondSortOrder = newSortOrder;
                        item.SecondSortOrder = await MakeRoomForSortOrderAsync(false, item.SecondId, newSortOrder, currentValue);
                        doUpdate = true;
                    }
                }
            }
            if (doUpdate) item = await UpdateAsync(item);
            else if (doCreate) item = await base.CreateAsync(item);
            await MaybeUpdatePrimaryIdAsync(item);
            return item;
        }

        private async Task<int> MakeRoomForSortOrderAsync(bool first, Guid oneId, int? newValue, int? currentValue = null)
        {
            var nextFirstSortOrder = 1 + await ReadMaxSortOrderAsync(first, oneId);
            var newValueNotNull = newValue ?? nextFirstSortOrder;
            if (currentValue != null)
            {
                // Move rather than insert
                if (!newValue.HasValue) newValueNotNull--;
                if (newValueNotNull == currentValue) return newValueNotNull;
                if (newValueNotNull > currentValue)
                {
                    ShiftSortOrder(first, true, oneId, (int)currentValue, newValueNotNull);
                }
                else
                {
                    ShiftSortOrder(first, false, oneId, newValueNotNull, (int)currentValue);
                }
                return newValueNotNull;
            }
            // Insert rather than move
            ShiftSortOrder(first, false, oneId, newValueNotNull);
            return newValueNotNull;
        }

        private void ShiftSortOrder(bool first, bool shiftLeft, Guid id, int firstValue, int? lastValue = null)
        {
            var firstOrSecond = FirstOrSecond(first);
            var addOrSubstract = shiftLeft ? "-" : "+";
            var smallerThanLastValue = lastValue == null ? "" : $"AND {firstOrSecond}SortOrder <= @LastValue";
            var sqlQuery = $"UPDATE [{TableMetadata.TableName}] SET {firstOrSecond}SortOrder = {firstOrSecond}SortOrder{addOrSubstract}1, Etag=NEWID(), [RowUpdatedAt]=GETUTCDATE()" +
                $" WHERE [{firstOrSecond}Id] = @Id AND {firstOrSecond}SortOrder >= @FirstValue {smallerThanLastValue} AND TypeId = @TypeId";
            using (IDbConnection db = NewSqlConnection())
            {
                db.Execute(sqlQuery,
                    new { Id = id, FirstValue = firstValue, LastValue = lastValue, TypeId = _typeId });
            }
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Delete all relationsships that contains the specified <paramref name="id"/>,
        /// either as FirstId or SecondId.
        /// </summary>
        public void DeleteRelationshipsForDeletedId(Guid id = default(Guid))
        {
            using (IDbConnection db = NewSqlConnection())
            {
                var sqlQuery = $"DELETE FROM [{TableMetadata.TableName}] WHERE TypeId = {_typeId} AND ([FirstId] = @Id OR [SecondId] = @Id)";
                db.Execute(sqlQuery, new { Id = id });
            }
        }
        #endregion


        #region SEARCH
        /// <summary>
        /// Find the item with sort order 1 and SecondId set to <paramref name="secondId"/>.
        /// </summary>
        public async Task<TFirstModel> ReadDefaultFirstBySecondIdAsync(Guid secondId)
        {
            return await ReadDefaultByIdAsync<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId);
        }

        /// <summary>
        /// Find the item with sort order 1 and FirstId set to <paramref name="firstId"/>.
        /// </summary>
        public async Task<TSecondModel> ReadDefaultSecondByFirstIdAsync(Guid firstId)
        {
            return await ReadDefaultByIdAsync<TSecondModel, TSecondTable>(false, _secondTableLogic, firstId);
        }

        private async Task<TModel> ReadDefaultByIdAsync<TModel, TLogic>(bool first, TLogic logic, Guid id)
            where TModel : ITableItem, IValidatable, new()
            where TLogic : SingleTableHandler<TModel>
        {
            var firstOrSecond = FirstOrSecond(first);
            var selectStatement = $"SELECT r.* FROM [{logic.TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.FirstId)" +
                   $" WHERE m2m.[{firstOrSecond}Id] = @Id AND m2m.[TypeId] = @TypeId AND m2m2.[{firstOrSecond}SortOrder] = 1";
            return await logic.SearchAdvancedSingleAsync(selectStatement, new { Id = id, TypeId = _typeId });
        }

        /// <summary>
        /// Find all items with SecondId set to <paramref name="secondId"/>.
        /// </summary>
        public async Task<PageEnvelope<TFirstModel, Guid>> SearchFirstBySecondId(Guid secondId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchByOtherIdAsync<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId, offset, limit);
        }

        /// <summary>
        /// Find all items with FirstId set to <paramref name="firstId"/>.
        /// </summary>
        public async Task<PageEnvelope<TSecondModel, Guid>> SearchSecondByFirstId(Guid firstId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchByOtherIdAsync<TSecondModel, TSecondTable>(true, _secondTableLogic, firstId, offset, limit);
        }

        private async Task<PageEnvelope<TModel, Guid>> SearchByOtherIdAsync<TModel, TLogic>(bool first, TLogic logic, Guid otherId, int offset = 0, int limit = PageInfo.DefaultLimit)
            where TModel : ITableItem, IValidatable, new()
            where TLogic : SingleTableHandler<TModel>
        {
            var firstOrSecond = FirstOrSecond(first);
            var other = FirstOrSecond(!first);
            var selectRest = $"FROM [{TableMetadata.TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.{other}Id)" +
                   $" WHERE m2m.[{firstOrSecond}Id] = @OtherId AND m2m.[TypeId] = @TypeId";
            return await logic.SearchAdvancedAsync("SELECT COUNT(r.[Id])", "SELECT r.*", selectRest, $"m2m.[{firstOrSecond}SortOrder]", new { OtherId = otherId, TypeId = _typeId }, offset, limit);
        }

        /// <summary>
        /// Find all items in the first table by finding relations by the SecondId that has value <paramref name="secondId"/>.
        /// </summary>
        public async Task<PageEnvelope<TFirstModel, Guid>> SearchFirstBySecondIdAndPrimaryId(Guid secondId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchByOtherIdAndPrimaryIdAsync<TFirstModel, TFirstTable>(true, _firstTableLogic, secondId, offset, limit);
        }

        /// <summary>
        /// Find all items in the second table by finding relations by the FirstId that has value <paramref name="firstId"/>.
        /// </summary>
        public async Task<PageEnvelope<TSecondModel, Guid>> SearchSecondByFirstIdAndPrimaryId(Guid firstId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchByOtherIdAsync<TSecondModel, TSecondTable>(true, _secondTableLogic, firstId, offset, limit);
        }

        private async Task<PageEnvelope<TModel, Guid>> SearchByOtherIdAndPrimaryIdAsync<TModel, TLogic>(bool first, TLogic logic, Guid otherId, int offset = 0, int limit = PageInfo.DefaultLimit)
            where TModel : ITableItem, IValidatable, new()
            where TLogic : SingleTableHandler<TModel>, IPartInManyToMany<TModel>
        {
            var nameOfPrimaryIdColumn = logic.NameOfForeignKey(_typeId);
            if (nameOfPrimaryIdColumn == null) throw new FulcrumNotImplementedException(nameof(nameOfPrimaryIdColumn));
            var firstOrSecond = FirstOrSecond(first);
            var other = FirstOrSecond(!first);
            var selectRest = $"FROM [{TableMetadata.TableName}] AS m2m" +
                   $" JOIN [{logic.TableName}] AS r ON (r.Id = m2m.{firstOrSecond}Id) AND {nameOfPrimaryIdColumn} = @OtherId" +
                   $" WHERE m2m.[{other}Id] = @OtherId AND m2m.[TypeId] = @TypeId";
            return await logic.SearchAdvancedAsync("SELECT COUNT(r.[Id])", "SELECT r.*", selectRest, $"m2m.[{other}SortOrder]", new { OtherId = otherId, TypeId = _typeId }, offset, limit);
        }

        /// <summary>
        /// Find all relations that has FirstId set to <paramref name="firstId"/>.
        /// </summary>
        protected async Task<PageEnvelope<TManyToManyModel, Guid>> SearchByFirstId(Guid firstId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchWhereAsync("[FirstId]=@FirstId AND [TypeId]=@TypeId", "m2m.[FirstSortOrder]", new { FirstId = firstId, TypeId = _typeId }, offset, limit);
        }


        /// <summary>
        /// Find all relations that has SecondId set to <paramref name="secondId"/>.
        /// </summary>
        protected async Task<PageEnvelope<TManyToManyModel, Guid>> SearchBySecondId(Guid secondId, int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            return await SearchWhereAsync("[SecondId]=@SecondId AND [TypeId]=@TypeId", "m2m.[SecondSortOrder]", new { SecondId = secondId, TypeId = _typeId }, offset, limit);
        }
        #endregion
    }
}

