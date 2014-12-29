using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using BIO.Framework.Core.Database;
using BIO.Framework.Extensions.Standard.Database.InputDatabase;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintDatabaseCreator : Framework.Core.Database.IDatabaseCreator<StandardRecord<StandardRecordData>>
    {
        string databasePath;

        public FingerPrintDatabaseCreator(string databasePath)
        {
            this.databasePath = databasePath;
        }

        public Database<StandardRecord<StandardRecordData>> createDatabase()
        {
            Database<StandardRecord<StandardRecordData>> database = new Database<StandardRecord<StandardRecordData>>();

            DirectoryInfo di = new DirectoryInfo(this.databasePath);
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("Curr dir:  {0}", path);

            // TODO: Vlozit spravnou priponu!!!!
            FileInfo[] files = di.GetFiles("*.tiff");
            //FileInfo[] files = di.GetFiles("*.png");

            foreach (FileInfo f in files)
            {
                string[] parts = f.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                BiometricID bioID = new BiometricID(parts[0], "fingerPrint");
                StandardRecordData data = new StandardRecordData(f.FullName);
                StandardRecord<StandardRecordData> record = new StandardRecord<StandardRecordData>(f.Name, bioID, data);

                database.addRecord(record);
            }
            return database;
        }
    }
}
