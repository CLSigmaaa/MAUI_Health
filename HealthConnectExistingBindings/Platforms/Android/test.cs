using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidX.Health.Connect.Client.Records;
using AndroidX.Health.Connect.Client.Records.Metadata;

namespace HealthConnectExistingBindings.Platforms.Android
{
    internal class test : Java.Lang.Object, IRecord
    {
        private readonly StepsRecord _stepRecord;

        // IRecord implementation
        public Metadata Metadata 
        { 
            get => _stepRecord.Metadata; 
            set => throw new NotSupportedException("Cannot set metadata on wrapper class");
        }

        public test(StepsRecord stepsRecord)
        {
            _stepRecord = stepsRecord ?? throw new ArgumentNullException(nameof(stepsRecord));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stepRecord?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
