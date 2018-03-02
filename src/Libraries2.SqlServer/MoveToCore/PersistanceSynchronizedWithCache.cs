using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.SqlServer.MoveToCore
{
    public class PersistanceSynchronizedWithCache<TItem, TId> : CrudBase<TItem, TId>
    {
        private readonly ICrud<TItem, TId> _persistance;
        private readonly ICrud<TItem, TId> _cache;
        private readonly ICrud<PageEnvelope<TItem>, int> _readAllCache;
        private int? _lastLimit;

        public PersistanceSynchronizedWithCache(ICrud<TItem, TId> persistance, ICrud<TItem, TId> cache, ICrud<PageEnvelope<TItem>, int> readAllCache = null)
        {
            _cache = cache;
            _readAllCache = readAllCache;
            _persistance = persistance;
        }

        public override async Task CreateWithSpecifiedIdAsync(TId id, TItem item)
        {
            var task1 = _persistance.CreateWithSpecifiedIdAsync(id, item);
            var task2 = _cache.CreateWithSpecifiedIdAsync(id, item);
            var task3 = _readAllCache?.DeleteAllAsync();
            await Task.WhenAll(task1, task2, task3);
        }

        public override async Task<TItem> ReadAsync(TId id)
        {
            var item = await _cache.ReadAsync(id);
            if (item != null) return item;
            item = await _persistance.ReadAsync(id);
            await _cache.CreateWithSpecifiedIdAsync(id, item);
            return item;
        }

        public override async Task DeleteAsync(TId id)
        {
            var task1 = _persistance.DeleteAsync(id);
            var task2 = _cache.DeleteAsync(id);
            var task3 = _readAllCache?.DeleteAllAsync();
            await Task.WhenAll(task1, task2, task3);
        }

        public override async Task<PageEnvelope<TItem>> ReadAllAsync(int offset = 0, int? limit = null)
        {
            Task task = null;
            PageEnvelope<TItem> page;
            if (limit == _lastLimit && _readAllCache != null)
            {
                page = await _readAllCache.ReadAsync(offset);
                if (page != null) return page;
            }
            else
            {
                _lastLimit = limit;
                task = _readAllCache?.DeleteAllAsync();
            }
            page = await _persistance.ReadAllAsync(offset, limit);
            if (_readAllCache == null) return page;
            await Task.WhenAll(task);
            await _readAllCache.CreateWithSpecifiedIdAsync(offset, page);
            return page;
        }

        public override async Task UpdateAsync(TId id, TItem item)
        {
            var task1 = _cache.UpdateAsync(id, item);
            var task2 = _persistance.UpdateAsync(id, item);
            var task3 = _readAllCache?.DeleteAllAsync();
            await Task.WhenAll(task1, task2, task3);
        }
    }
}
