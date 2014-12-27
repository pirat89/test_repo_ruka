using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Core.Evaluation;
using BIO.Framework.Core.Template.Persistence;
using BIO.Framework.Extensions.Standard.Template.Persistence;
using BIO.Framework.Extensions.Standard.Database.InputDatabase;
using BIO.Framework.Extensions.Emgu.InputData;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintEvaluationSettings :
        BIO.Framework.Extensions.Standard.Evaluation.Block.BlockEvaluationSettings<
        StandardRecord<StandardRecordData>, 
        EmguGrayImageInputData>
    {
        public FingerPrintEvaluationSettings() {
            
            {
                var value = new FingerPrintProcessingBlockComponents();
                this.addBlockToEvaluation(value.createBlock());
            }
           
            
        }

        protected override Framework.Core.InputData.IInputDataCreator<StandardRecord<StandardRecordData>, EmguGrayImageInputData> createInputDataCreator() {
            return new FingerPrintInputDataCreator();
        }
    }
}
