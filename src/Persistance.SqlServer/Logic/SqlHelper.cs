using System.Collections.Generic;
using System.Linq;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Logic
{
    internal static class SqlHelper
    {
        public static string Create(ITableItem item) => $"INSERT INTO dbo.[{item.TableName}] ({ColumnList(item)}) values ({ArgumentList(item)})";

        public static string Read(ITableItem item, string where) => $"SELECT {ColumnList(item)} FROM [{item.TableName}] WHERE {where}";

        public static string Read(ITableItem item, string where, string orderBy) => $"SELECT {ColumnList(item)} FROM [{item.TableName}] WHERE {where} ORDER BY {orderBy}";
        

        public static string Update(ITableItem item, string oldEtag) => $"UPDATE [{item.TableName}] SET {UpdateList(item)} WHERE Id = @Id AND ETag == '{oldEtag}'";

        public static string Delete(ITableItem item) => $"DELETE FROM [{item.TableName}] WHERE Id = @Id";

        public static string ColumnList(ITableItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]"));

        public static string ArgumentList(ITableItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"@{name}"));

        public static string UpdateList(ITableItem item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]=@{name}"));

        public static IEnumerable<string> NonCustomColumnNames(ITableItem item)
        {
            var list = new List<string> {"Id", "ETag"};
            var timeStamped = item as ITimeStamped;
            if (timeStamped == null) return list;
            list.AddRange(new [] {"CreatedAt", "UpdatedAt"});
            return list;
        }

        public static IEnumerable<string> AllColumnNames(ITableItem item)
        {
            var list = NonCustomColumnNames(item).ToList();
            list.AddRange(item.CustomColumnNames);
            return list;
        }
    }
}
