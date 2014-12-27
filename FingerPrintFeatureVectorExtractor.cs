using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

using BIO.Framework.Core;
using BIO.Framework.Extensions.Emgu.FeatureVector;
using BIO.Framework.Extensions.Emgu.InputData;
using BIO.Framework.Core.FeatureVector;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintFeatureVectorExtractor : IFeatureVectorExtractor<EmguGrayImageInputData, FingerPrintFeatureVector>
    {
        public FingerPrintFeatureVector extractFeatureVector(EmguGrayImageInputData input)
        {
            // TODO: Implementovat extrakci rysu
            var featureVector = new FingerPrintFeatureVector();

            var processedImg = input.Image.SmoothGaussian(3);

            return featureVector;
        }
    }
}
