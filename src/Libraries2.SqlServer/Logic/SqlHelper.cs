using System;
using System.Collections.Generic;
using System.Linq;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer.Logic
{
    internal static class SqlHelper
    {
        public static string Create(ISqlTableMetadata tableMetadata) => $"INSERT INTO dbo.[{tableMetadata.TableName}] ({ColumnList(tableMetadata)}) values ({ArgumentList(tableMetadata)})";

        public static string Read(ISqlTableMetadata tableMetadata, string where) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where}";

        public static string Read(ISqlTableMetadata tableMetadata, string where, string orderBy) => $"SELECT {ColumnList(tableMetadata)} FROM [{tableMetadata.TableName}] WHERE {where} ORDER BY {orderBy}";

        public static string Update(ISqlTableMetadata tableMetadata, string oldEtag) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id AND Etag == '{oldEtag}'";

        public static string Update(ISqlTableMetadata tableMetadata) => $"UPDATE [{tableMetadata.TableName}] SET {UpdateList(tableMetadata)} WHERE Id = @Id";

        public static string DeleteBasedOnColumnValue(ISqlTableMetadata tableMetadata, string columnName, string variableName = null) => $"DELETE FROM [{tableMetadata.TableName}] WHERE {columnName} = @{variableName ?? columnName}";

        public static string ColumnList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"[{name}]"));

        public static string ArgumentList(ISqlTableMetadata tableMetadata) => string.Join(", ", AllColumnNames(tableMetadata).Select(name => $"@{name}"));

        public static string UpdateList(ISqlTableMetadata tableMetadata) => string.Join(", ", tableMetadata.CustomColumnNames.Select(name => $"[{name}]=@{name}"), UpdateNonCustomColumns(tableMetadata));

        private static IEnumerable<string> UpdateNonCustomColumns(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> {$"Etag={Guid.NewGuid()}"};
            if (tableMetadata.UpdatedAtColumnName != null) list.Add($"{tableMetadata.UpdatedAtColumnName}='{DateTimeOffset.Now}'");
            return list;
        }

        public static IEnumerable<string> NonCustomColumnNames(ISqlTableMetadata tableMetadata)
        {
            var list = new List<string> {"Id", "Etag"};
            if (tableMetadata.CreatedAtColumnName != null) list.Add(tableMetadata.CreatedAtColumnName);
            if (tableMetadata.UpdatedAtColumnName != null) list.Add(tableMetadata.UpdatedAtColumnName);
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
