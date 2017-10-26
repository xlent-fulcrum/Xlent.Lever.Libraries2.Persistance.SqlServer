using System.Collections.Generic;
using System.Linq;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Logic
{
    internal static class SqlHelper
    {
        public static string Create(ISqlTableMetadata tableMetadata) => $"INSERT INTO dbo.[{tableMetadata.TableName}] ({ColumnList(tableMetadata)}) values ({ArgumentList(tableMetadata)})";

        public static string Read(ISqlTableMetadata tableMetadata, string where) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where}";

        public static string Read(ISqlTableMetadata tableMetadata, string where, string orderBy) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where} ORDER BY {orderBy}";
        

        public static string Update(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND ETag == '{oldEtag}'";

        public static string DeleteBasedOnColumnValue(ISqlTableMetadata tableMetadata, string columnName, string variableName = null) => $"DELETE FROM [{tableMetadata.TableName}] WHERE {columnName} = @{variableName ?? columnName}";

        public static string ColumnList(ISqlTableMetadata item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]"));

        public static string ArgumentList(ISqlTableMetadata item) => string.Join(", ", AllColumnNames(item).Select(name => $"@{name}"));

        public static string UpdateList(ISqlTableMetadata item) => string.Join(", ", AllColumnNames(item).Select(name => $"[{name}]=@{name}"));

        public static IEnumerable<string> NonCustomColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> {"Id", "ETag"};
            var timeStamped = tableMetadata as ITimeStamped;
            if (timeStamped == null) return list;
            list.AddRange(new [] {"CreatedAt", "UpdatedAt"});
            return list;
        }

        public static IEnumerable<string> AllColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = NonCustomColumnNames(tableMetadata).ToList();
            list.AddRange(tableMetadata.CustomColumnNames);
            return list;
        }
    }
}
