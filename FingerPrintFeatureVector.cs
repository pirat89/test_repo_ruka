using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Core;
using BIO.Framework.Extensions.Emgu.FeatureVector;
using BIO.Framework.Extensions.Emgu.InputData;
using BIO.Framework.Core.FeatureVector;

namespace BIO.Project.FingerPrintRecognition
{
    [Serializable]
    class FingerPrintFeatureVector : IFeatureVector
    {
        public List<FingerPrintMinutiae> Minutiaes { get; set; }
    }
}
