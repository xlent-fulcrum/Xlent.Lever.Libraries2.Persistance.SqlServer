﻿using System;
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
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocaction, string propertyPath = "")
        {
            base.Validate(errorLocaction, propertyPath);
            var now = DateTimeOffset.Now;
            FulcrumValidate.IsTrue(RecordCreatedAt < now, errorLocaction, $"Expected {nameof(RecordCreatedAt)} ({RecordCreatedAt}) to have a value less than the current time ({now}).");
            FulcrumValidate.IsTrue(RecordUpdatedAt < now, errorLocaction, $"Expected {nameof(RecordUpdatedAt)} ({RecordUpdatedAt}) to have a value less than the current time ({now}).");
        }
    }
}
