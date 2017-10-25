using Xlent.Lever.Libraries2.Core.Assert;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.ToDo.Models
{
    /// <summary>
    /// One table to keep all ManyToMany relations
    /// </summary>
    public class SingleManyToMany : ManyToMany
    {
        /// <inheritdoc />
        public override string TableName => "SingleManyToMany";

    }
}
