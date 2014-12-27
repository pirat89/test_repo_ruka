using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Core;
using BIO.Framework.Extensions.Emgu.InputData;
using BIO.Framework.Extensions.Standard.Database.InputDatabase;
using BIO.Framework.Core.InputData;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintInputDataCreator : IInputDataCreator<StandardRecord<StandardRecordData>, EmguGrayImageInputData>
    {
        public EmguGrayImageInputData createInputData(StandardRecord<StandardRecordData> record)
        {
            return new EmguGrayImageInputData(record.BiometricData.Data);
        }
    }
}
