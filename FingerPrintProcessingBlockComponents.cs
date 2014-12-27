using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Extensions.Emgu.InputData;
using BIO.Framework.Extensions.Emgu.FeatureVector;
using BIO.Framework.Extensions.Standard.Template;
using BIO.Framework.Extensions.Standard.Block;
using BIO.Framework.Extensions.Standard.Comparator;
using BIO.Framework.Core.Comparator;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintProcessingBlockComponents : InputDataProcessingBlockSettings<
        EmguGrayImageInputData,
        FingerPrintFeatureVector,
        Template<FingerPrintFeatureVector>,
        FingerPrintFeatureVector>
    {
        public FingerPrintProcessingBlockComponents() : base("fingerPrint") { 
        
        }

        protected override Framework.Core.FeatureVector.IFeatureVectorExtractor<EmguGrayImageInputData, FingerPrintFeatureVector> createTemplatedFeatureVectorExtractor() {
            return new FingerPrintFeatureVectorExtractor();
        }

        protected override Framework.Core.FeatureVector.IFeatureVectorExtractor<EmguGrayImageInputData, FingerPrintFeatureVector> createEvaluationFeatureVectorExtractor()
        {
            return new FingerPrintFeatureVectorExtractor();
        }

        protected override Framework.Core.Comparator.IComparator<FingerPrintFeatureVector, Template<FingerPrintFeatureVector>, FingerPrintFeatureVector> createComparator()
        {
            return
               new Comparator<FingerPrintFeatureVector, Template<FingerPrintFeatureVector>, FingerPrintFeatureVector>(
                   CreateFeatureVectorComparator(), CreateScoreSelector());
        }

        private static IFeatureVectorComparator<FingerPrintFeatureVector, FingerPrintFeatureVector> CreateFeatureVectorComparator()
        {
            return new FingerPrintComparator();
        }

        private static IScoreSelector CreateScoreSelector() {
            return new MinScoreSelector();
        }
    }
}
