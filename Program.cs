using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Extensions.Standard.Database.InputDatabase;

namespace BIO.Project.FingerPrintRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            ProjectSettings settings = new ProjectSettings();

            var project = new StandardProject<StandardRecord<StandardRecordData>>(settings);

            BIO.Framework.Core.Evaluation.Results.Results results = project.run();
        }
    }
}
