using System;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.SqlServer.Model
{
    /// <summary>
    /// Inhertis from <see cref="TableItem"/> and adds time stamp columns.
    /// </summary>
    /// <remarks>Please note it is not mandatory to inherit from this class to use the functionality in this package. It is only provided as a convenience class.</remarks>
    public abstract class TimeStampedTableItem : TableItem, ITimeStamped
    {
        /// <inheritdoc />
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset UpdatedAt { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocaction, string propertyPath = "")
        {
            base.Validate(errorLocaction, propertyPath);
            var now = DateTimeOffset.Now;
            FulcrumValidate.IsTrue(CreatedAt < now, errorLocaction, $"Expected {nameof(CreatedAt)} ({CreatedAt}) to have a value less than the current time ({now}).");
            FulcrumValidate.IsTrue(UpdatedAt < now, errorLocaction, $"Expected {nameof(UpdatedAt)} ({UpdatedAt}) to have a value less than the current time ({now}).");
        }
    }
}
