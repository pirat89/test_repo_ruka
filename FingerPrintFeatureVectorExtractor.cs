﻿using System;
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


        public FingerPrintFeatureVector extractFeatureVector(EmguGrayImageInputData input)
        {
            // TODO: Implementovat extrakci rysu
            var featureVector = new FingerPrintFeatureVector();

            Bitmap binarizedImg = Binarization(input);

            return featureVector;
        }
    }
}
