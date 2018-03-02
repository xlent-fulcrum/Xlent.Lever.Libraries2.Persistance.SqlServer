using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer.Cache
{
    public class CacheItem : TimeStampedTableItem
    {
        public string SerializedItem { get; set; }
    }
}
