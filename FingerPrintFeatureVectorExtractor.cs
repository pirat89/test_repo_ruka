using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;

using BIO.Framework.Core;
using BIO.Framework.Extensions.Emgu.FeatureVector;
using BIO.Framework.Extensions.Emgu.InputData;
using BIO.Framework.Core.FeatureVector;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintFeatureVectorExtractor : IFeatureVectorExtractor<EmguGrayImageInputData, FingerPrintFeatureVector>
    {
        /// <summary>
        /// Funkce otsu() vypocte hodnotu prahu na zaklade histogramu pixelu snimku.
        /// </summary>
        /// <param name="hist">histogram</param>
        /// <param name="n">pocet polozek histogramu</param>
        /// <returns>Spoctena hodnota prahu</returns>
        int otsu(int[] hist, int n)
        {

            int total = 0;
            float sum = 0;
            float sumB = 0, varMax = 0, varBetween;
            int wB = 0, wF = 0, threshold = 0;
            float mB, mF;
            int t;

            for (t = 0; t < n; t++)
            {
                sum += t * hist[t];
                total += hist[t];
            }

            for (t = 0; t < n; t++)
            {

                wB += hist[t];             /* Weight Background */
                if (wB == 0) continue;

                wF = total - wB;           /* Weight Foreground */
                if (wF == 0) break;

                sumB += (float)(t * hist[t]);

                mB = sumB / wB;            /* Mean Background */
                mF = (sum - sumB) / wF;    /* Mean Foreground */

                /* Calculate Between Class Variance */
                varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                /* Check if new maximum found */
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t + 1;
                }
            }

            return threshold;
        }

        /// <summary>
        /// Inicializace okna
        /// </summary>
        /// <param name="window">okno</param>
        /// <param name="input">vstupni obraz</param>
        void InitWindow(byte[] window,EmguGrayImageInputData input)
        {
            window[4] = Convert.ToByte(input.Image[0, 0].Intensity);
            window[5] = Convert.ToByte(input.Image[1, 0].Intensity);

            window[7] = Convert.ToByte(input.Image[0, 1].Intensity);
            window[8] = Convert.ToByte(input.Image[1, 1].Intensity);
        }

        /// <summary>
        /// Procedura clip_window() provadi clipping tj. doplnuje krajni hodnoty okenka
        /// 3x3 na okrajich snimku, kde nejsou pixely k dispozici.
        /// </summary>
        /// <param name="r">aktualni cislo radku</param>
        /// <param name="c">aktualni cislo sloupce</param>
        /// <param name="r_max">maximalni cislo radku</param>
        /// <param name="c_max">maximalni cislo sloupce</param>
        /// <param name="window">okno</param>
        void ClipWindow(int r, int c, int r_max, int c_max, byte[] window)
        {
            int first_row, last_row, first_col, last_col;
            int test1, test2, test3, test4;


            first_row = Convert.ToInt32((r == 0));
            first_col = Convert.ToInt32((c == 0));
            last_row = Convert.ToInt32((r == r_max));
            last_col = Convert.ToInt32((c == c_max));

            window[1] = Convert.ToBoolean(first_col) ? window[4] : window[1];
            window[5] = Convert.ToBoolean(last_row) ? window[4] : window[5];
            window[7] = Convert.ToBoolean(last_col) ? window[4] : window[7];
            window[3] = Convert.ToBoolean(first_row) ? window[4] : window[3];

            test1 = first_row | (first_col << 1);
            switch (test1)
            {
                case 3: window[0] = window[4]; break; /* first_row, first_col */
                case 1: window[0] = window[1]; break; /* first_row, not first_col */
                case 2: window[0] = window[3]; break; /* not first_row, first_col */
                default: window[0] = window[0]; break; /* not first_row, not first_col */
            }

            test2 = first_row | (last_col << 1);
            switch (test2)
            {
                case 3: window[6] = window[4]; break; /* first_row, last_col */
                case 1: window[6] = window[7]; break; /* first_row, not last_col */
                case 2: window[6] = window[3]; break; /* not first_row, last_col */
                default: window[6] = window[6]; break; /* not first_row, not last_col */
            }

            test3 = last_row | (first_col << 1);
            switch (test3)
            {
                case 3: window[2] = window[4]; break; /* last_row, first_col */
                case 1: window[2] = window[1]; break; /* last_row, not first_col */
                case 2: window[2] = window[5]; break; /* not last_row, first_col */
                default: window[2] = window[2]; break; /* not last_row, not first_col */
            }

            test4 = last_row | (last_col << 1);
            switch (test4)
            {
                case 3: window[8] = window[4]; break; /* last_row, last_col */
                case 1: window[8] = window[7]; break; /* last_row, not last_col */
                case 2: window[8] = window[5]; break; /* not last_row, last_col */
                default: window[8] = window[8]; break; /* not last_row, not last_col */
            }
        }

        
        /// <summary>
        /// Funkce median() vraci hodnotu medianu ze zadaneho okenka hodnot 3x3 pixelu.
        /// </summary>
        /// <param name="window">okno 3x3</param>
        /// <returns>hodnota medianu</returns>
        byte Median(byte[] window)
        {
           int         i, j, max;
           byte[] R = new byte[9];

           for(i=0;i<9;i++)
              R[i] = window[i];

           for(j=0;j<4;j++){
              max=j;
              for(i=j+1;i<9;i++)
                 if(R[i]>R[max]) max=i;
              R[max]=R[j];
           }

           max=4;
           for(i=5;i<9;i++)
              if(R[i]>R[max]) max=i;

           return R[max];
        }

        /// <summary>
        /// Funkce avg() vraci prumernou hodnotu ze zadaneho okenka hodnot 3x3 pixelu.
        /// </summary>
        /// <param name="window">okno 3x3</param>
        /// <returns>prumerna hodnota</returns>
        byte Avg(byte[] window)
        {
            int sum = 0;

            for (var i = 0; i < 9; ++i)
                sum += window[i];

            return (Convert.ToByte(sum / 9));
        }

        /// <summary>
        /// Procedura shift_window() provadi posun okenka 3x3 o jednu pozici do prava.
        /// </summary>
        /// <param name="window">okno 3x3</param>
        /// <param name="r">aktualni radek</param>
        /// <param name="c">aktualni sloupec</param>
        /// <param name="input">vstupni obraz</param>
        void ShiftWindow(byte[] window, int r, int c, EmguGrayImageInputData input)
        {
            int new_c = c;

            // last pixel
            if ((r == input.Image.Height - 1) && (c == input.Image.Width - 1))
                return;

            new_c = (c + 1) % input.Image.Width;

            // new line
            if (new_c == 0)
            {
                window[3] = Convert.ToByte(input.Image[r, 0].Intensity);
                window[4] = Convert.ToByte(input.Image[r + 1, 0].Intensity);

                if(r + 2 <= input.Image.Height - 1)
                    window[5] = Convert.ToByte(input.Image[r + 2, 0].Intensity);

                window[6] = Convert.ToByte(input.Image[r, 1].Intensity);
                window[7] = Convert.ToByte(input.Image[r + 1, 1].Intensity);

                if (r + 2 <= input.Image.Height - 1)
                    window[8] = Convert.ToByte(input.Image[r + 2, 1].Intensity);
            }
            else
            {
                window[2] = window[5];
                window[1] = window[4];
                window[0] = window[3];

                window[5] = window[8];
                window[4] = window[7];
                window[3] = window[6];

                if (new_c != input.Image.Width - 1)
                {
                    if (r != input.Image.Height - 1)
                        window[8] = Convert.ToByte(input.Image[r + 1, new_c].Intensity);

                    window[7] = Convert.ToByte(input.Image[r, new_c].Intensity);

                    if (r != 0)
                        window[6] = Convert.ToByte(input.Image[r - 1, new_c].Intensity);
                }
            }
        }

        /// <summary>
        /// Funkce provadi binarizaci vstupniho obrazu metodou otsu
        /// </summary>
        /// <param name="input">vstupni obraz</param>
        /// <returns>Binarizovany obraz</returns>
        Bitmap Binarization(EmguGrayImageInputData input)
        {
            int[] histogram = new int[256];
            int threshold = 128;
            byte pix_filtered;
            byte[] window = new byte[9];
            byte max_pixel = 255;
            byte min_pixel = 0;
            byte[,] filtered = new byte[input.Image.Height, input.Image.Width];
            byte[] filteredArr = new byte[input.Image.Height * input.Image.Width];
            int sum = 0;

            InitWindow(window, input);

            // Histogram
            for (var r = 0; r < input.Image.Height; ++r)
            {
                for (var c = 0; c < input.Image.Width; ++c)
                {
                    ClipWindow(r, c, input.Image.Height - 1, input.Image.Width - 1, window);

                    // Filtrace medianem
                    pix_filtered = Median(window);

                    // Filtrace prumerem
                    //pix_filtered = Avg(window);

                    // Aktualizace histogramu
                    histogram[pix_filtered]++;

                    // Hodnota bude pouzita v dalsi iteraci pro prahovani
                    filtered[r, c] = pix_filtered;

                    ShiftWindow(window, r, c, input);

                    sum += Convert.ToInt32(input.Image[r, c].Intensity);
                }
            }

            // Otsu metoda
            threshold = otsu(histogram, 256);

            // Konstanta
            //threshold = 220;

            // AVG all
            //threshold = sum / (input.Image.Height * input.Image.Width);

            var bitMap = new Bitmap(input.Image.Width, input.Image.Height);

            for (var r = 0; r < input.Image.Height; ++r)
            {
                for (var c = 0; c < input.Image.Width; ++c)
                {
                    if (filtered[r, c] >= threshold)
                        bitMap.SetPixel(c, r, Color.FromArgb(max_pixel, max_pixel, max_pixel));
                    else
                        bitMap.SetPixel(c, r, Color.FromArgb(min_pixel, min_pixel, min_pixel));
                }
            }

            return bitMap;
        }

        /// <summary>
        /// Funkce provadi extrakci markantu. Extrahuji se pouze vidlice,
        /// protoze se jedna o spolehlivejsi rys.
        /// </summary>
        /// <param name="input">otisk prstu binarizovany a se ztentenymi
        /// pap. liniemi</param>
        /// <returns>Vektor markantu</returns>
        FingerPrintFeatureVector GetMinutaes(Bitmap input)
        {
            int border_value = 5;
            int sum;
            int sub_val = 0;
            int fork = 6;
            Color tmp;
            Color tmp2;

            var featureVector = new FingerPrintFeatureVector();
            featureVector.Minutiaes = new List<FingerPrintMinutiae>();

            // TODO: Odstranit, jen pro debug!!!
            var bitMap = new Bitmap(input.Width, input.Height);
            for (var r = border_value; r < input.Height - border_value; ++r)
            {
                for (var c = border_value; c < input.Width - border_value; ++c)
                {
                    bitMap.SetPixel(c, r, input.GetPixel(c,r));
                }
            }

            // Pruchod 8-okoli
            int[] r_arr = {0, -1, -1, -1,  0,  1, 1, 1};
            int[] c_arr = {1,  1,  0, -1, -1, -1, 0, 1};

            int[] r_arr2 = {-1, -1, -1,  0,  1, 1, 1, 0};
            int[] c_arr2 = { 1,  0, -1, -1, -1, 0, 1, 1};

            // Pruchod okoli 9-24, kde 25 = 9
            int[] r_arr3 = {0, -1, -2, -2, -2, -2, -2, -1,  0,  1,  2,  2, 2, 2, 2, 1};
            int[] c_arr3 = {2,  2,  2,  1,  0, -1, -2, -2, -2, -2, -2, -1, 0, 1, 2, 2};

            int[] r_arr4 = {-1, -2, -2, -2, -2, -2, -1,  0,  1,  2,  2, 2, 2, 2, 1, 0};
            int[] c_arr4 = { 2,  2,  1,  0, -1, -2, -2, -2, -2, -2, -1, 0, 1, 2, 2, 2};

            for (var r = border_value; r < input.Height - border_value; ++r)
            {
                for (var c = border_value; c < input.Width - border_value; ++c)
                {
                    sum = 0;
                    // Projiti 8-okoli
                    for (var i = 0; i < 8; ++i)
                    {
                        tmp = input.GetPixel(c + c_arr[i], r + r_arr[i]);
                        tmp2 = input.GetPixel(c + c_arr2[i], r + r_arr2[i]);
                        sub_val = Math.Abs(tmp2.R - tmp.R);

                        if(sub_val > 0)
                            sum++;
                    }

                    // Nalezeni vidlicky
                    if (sum == fork)
                    {
                        sum = 0;

                        // Rozsireni plochy okoli
                        for (var i = 0; i < 16; ++i)
                        {
                            tmp = input.GetPixel(c + c_arr3[i], r + r_arr3[i]);
                            tmp2 = input.GetPixel(c + c_arr4[i], r + r_arr4[i]);
                            sub_val = Math.Abs(tmp2.R - tmp.R);

                            if (sub_val > 0)
                                sum++;
                        }

                        // Potvrzeni nalezeni vidlicky
                        if (sum == fork)
                        {
                            //TODO: ODSTRANIT, jen pro debugg
                            bitMap.SetPixel(c, r, Color.FromArgb(255, 0, 0));
                            bitMap.SetPixel(c+1, r, Color.FromArgb(255, 0, 0));
                            bitMap.SetPixel(c-1, r, Color.FromArgb(255, 0, 0));
                            bitMap.SetPixel(c, r+1, Color.FromArgb(255, 0, 0));
                            bitMap.SetPixel(c, r-1, Color.FromArgb(255, 0, 0));

                            FingerPrintMinutiae minutae = new FingerPrintMinutiae();
                            minutae.PositionX = c;
                            minutae.PositionY = r;
                            // TODO: Vypocet uhlu!!
                            minutae.Angle = 0;
                            minutae.Type = FingerPrintMinutiae.MinutiaeType.FORK;

                            featureVector.Minutiaes.Add(minutae);
                        }
                    }
                }
            }

            return featureVector;
        }

        int CountTrue(params bool[] args)
        {
            return args.Count( t => t);
        }

        bool tgh(bool[,] image ,Boolean odd)
        {
            bool p1,p2,p3,p4,p5,p6,p7,p8, p9, tmp, changed = false;
            int C, n, n1, n2;
                
            for(int i = 1; i < image.GetLength(1) -1; i++) {
                for(int j = 1; j < image.GetLength(0) -1; j++) {
                    p1 = image[j, i];
                    if (p1 == true) continue;
                    p2 = image[j-1,i];
                    p3 = image[j-1,i+1];
                    p4 = image[j,i+1];
                    p5 = image[j+1,i+1];
                    p6 = image[j+1,i];
                    p7 = image[j+1,i-1];
                    p8 = image[j,i-1];
                    p9 = image[j+1,i-1];

                    C = CountTrue((!p2 && (p3 || p4)), (!p4 && (p5 || p6)), (!p6 && (p7 || p8)), (!p8 && (p9 || p2)));
                    n1 = CountTrue((p9 || p2), (p3 || p4), (p5 || p6), (p7 || p8));
                    n2 = CountTrue((p2 || p3), (p4 || p5), (p6 || p7), (p8 || p9));
                    n = (n1 < n2)? n1 : n2;
                    tmp = (odd)? ((p2 || p3 || !p5) && p4) : ((p6 || p7 || !p9) && p8);

                    if (C == 1 && (n >= 2 && n <= 3) && tmp == false)
                    {
                        changed = true;
                        image[j, i] =  true;
                    }
                }
            }

            return changed;
        }

        void ThinningGuoHall(bool[,] image)
        {
            //Boolean changed = new Boolean();
            bool changed;

            do 
            {
                changed = tgh(image, true);
                changed = changed || tgh(image, false);
            } while (changed);

        }

        // z dizertacni prace Ing (Ph.D.) Michala Dobese, z roku 1996
        bool tmdeu(bool[,] image, bool odd)
        {
            bool changed = false;
            bool[] p = new bool[10];
            bool c1, c2, c3, c4;
            int X;
            p[0] = false; // nema zde byt, ale diky tomu neni treba predelavat cislovani 

            for (int i = 1; i < image.GetLength(1) - 1; i++)
            {
                for (int j = 1; j < image.GetLength(0) - 1; j++)
                {
                    if (image[j, i] == false)
                        continue;
                    X = 0;
                    p[1] = image[j + 1, i];
                    p[2] = image[j + 1, i - 1];
                    p[3] = image[j, i - 1];
                    p[4] = image[j - 1, i - 1];
                    p[5] = image[j - 1, i];
                    p[6] = image[j - 1, i + 1];
                    p[7] = image[j, i + 1];
                    p[8] = image[j + 1, i + 1];
                    p[9] = p[1];

                    // calculation
                    for (int k = 1; k < 9; k++)
                    {
                        if (p[k] == p[k + 1])
                            X += 1;
                    }
                    p[9] = false; // tahle pozice kopiruje jednicku a nemela by tu tedy ani byt
                    if (X != 0 && X != 2 && X != 4)
                        continue;
                    if (CountTrue(p) == 1)
                        continue;
                    if (odd)
                    {
                        c1 = (p[1] && p[3] && p[5]) == false;
                        c2 = (p[1] && p[3] && p[7]) == false;
                        c3 = ((p[1] && p[7]) == true) && ((p[2] || p[6]) == true) && ((p[3] && p[4] && p[5] && p[8]) == false);
                        c4 = ((p[1] && p[3]) == true) && ((p[4] || p[8]) == true) && ((p[2] && p[5] && p[6] && p[7]) == false);
                    }
                    else
                    {
                        c1 = (p[1] && p[5] && p[7]) == false;
                        c2 = (p[3] && p[5] && p[7]) == false;
                        c3 = ((p[3] && p[5]) == true) && ((p[2] || p[6]) == true) && ((p[3] && p[4] && p[7] && p[8]) == false);
                        c4 = ((p[5] && p[7]) == true) && ((p[4] || p[8]) == true) && ((p[2] && p[5] && p[3] && p[6]) == false);
                    }

                    if ((c1 || c2) == false)
                        continue;
                    if (X == 4 && (c3 || c4) == false)
                        continue;

                    // muzeme smaznout
                    changed = true;
                    image[j, i] = false;
                }
            }
            return changed;
        }

        void ThinningModDeutsh(bool[,] image)
        {
            bool changed;

            do
            {
                changed = tmdeu(image, true);
                changed = changed || tmdeu(image, false);
                Bitmap oo = bool2bitmap(image, true);
            } while (changed);
        }

        public Bitmap bool2bitmap(bool[,] image, bool invert)
        {
            Bitmap bImg = new Bitmap(image.GetLength(0),image.GetLength(1));
            for (int x = 0; x < bImg.Width; x++)
            {
                for (int y = 0; y < bImg.Height; y++)
                {
                    if (image[x,y] == invert)
                    {
                        bImg.SetPixel(x, y, Color.Black);
                    }
                    else bImg.SetPixel(x, y, Color.White);
                }
            }
                return bImg;
        }

        public bool[,] bitmap2bool(Bitmap bImg, bool invert)
        {
            bool[,] image = new bool[bImg.Width, bImg.Height];

            for (int x = 0; x < bImg.Width; x++)
            {
                for (int y = 0; y < bImg.Height; y++)
                {
                    image[x, y] = (bImg.GetPixel(x, y).R == 0) == invert;
                    /*if ((bImg.GetPixel(x,y).R == 0) == invert)
                    {
                        image[x,y] = true;
                    }
                    else image[x,y] = false;
                     */
                }
            }

            return image;
        }

        public FingerPrintFeatureVector extractFeatureVector(EmguGrayImageInputData input)
        {
            // TODO: Implementovat extrakci rysu
            //var featureVector = new FingerPrintFeatureVector();

            Bitmap binarizedImg = Binarization(input);
            bool[,] workImg = bitmap2bool(binarizedImg, false);
            ThinningGuoHall(workImg);

            Bitmap thinnImg = bool2bitmap(workImg, true);
            // it's not functional
            //workImg = bitmap2bool(binarizedImg, true);
            //ThinningModDeutsh(workImg);
            //Bitmap thinnImg2 = bool2bitmap(workImg, true);

            // TODO: Zmenit binarizedImg na ztencenyImg!!!
            var featureVector = GetMinutaes(thinnImg);

            return featureVector;
        }
    }
}
