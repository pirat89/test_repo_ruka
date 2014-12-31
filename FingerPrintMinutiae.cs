using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIO.Project.FingerPrintRecognition
{
    [Serializable()]
    class FingerPrintMinutiae
    {
        int positionX;
        int positionY;
        double angle;
        public enum MinutiaeType { CROSS, ENDING, FORK };
        MinutiaeType type;

        #region Getters and Setters
        public int PositionX
        {
            get { return positionX; }
            set { positionX = value; }
        }

        public int PositionY
        {
            get { return positionY; }
            set { positionY = value; }
        }

        public double Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public MinutiaeType Type
        {
            get { return type; }
            set { type = value; }
        }
        #endregion
    }
}
