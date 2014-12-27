using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Extensions.Standard.Database.InputDatabase;
using BIO.Framework.Extensions.Emgu.InputData;

namespace BIO.Project.FingerPrintRecognition
{
    class ProjectSettings :
        BIO.Project.ProjectSettings<StandardRecord<StandardRecordData>, EmguGrayImageInputData>,
        IStandardProjectSettings<StandardRecord<StandardRecordData>>
    {
        #region IStandardProjectSettings<StandardRecord<StandardRecordData>> Members

        public int TemplateSamples
        {
            get
            {
                return 1;
            }
        }

        #endregion

        public override Framework.Core.Database.IDatabaseCreator<StandardRecord<StandardRecordData>> getDatabaseCreator()
        {
            // TODO: Vlozit spravnou slozku!!!!
            return new FingerPrintDatabaseCreator(@"..\..\jaffe");
        }

        protected override Framework.Core.Evaluation.Block.IBlockEvaluatorSettings<StandardRecord<StandardRecordData>, EmguGrayImageInputData> getEvaluatorSettings()
        {
            return new FingerPrintEvaluationSettings();
        }
    }
}
