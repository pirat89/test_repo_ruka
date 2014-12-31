using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintPair
    {
        int posX;
        int posY;
        int posX_2;
        int posY_2;
        double distance;

        #region Getters and Setters
        public int PosX
        {
            get { return posX; }
            set { posX = value; }
        }

        public int PosY
        {
            get { return posY; }
            set { posY = value; }
        }

        public int PosX_2
        {
            get { return posX_2; }
            set { posX_2 = value; }
        }

        public int PosY_2
        {
            get { return posY_2; }
            set { posY_2 = value; }
        }

        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }
        #endregion
    }
}
