using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BIO.Framework.Core.Comparator;
using BIO.Framework.Extensions.Emgu.FeatureVector;

namespace BIO.Project.FingerPrintRecognition
{
    class FingerPrintComparator : IFeatureVectorComparator<FingerPrintFeatureVector, FingerPrintFeatureVector>
    {
        double[,] GetInverse(double[,] a)
        {
            var s0 = a[0, 0] * a[1, 1] - a[1, 0] * a[0, 1];
            var s1 = a[0, 0] * a[1, 2] - a[1, 0] * a[0, 2];
            var s2 = a[0, 0] * a[1, 3] - a[1, 0] * a[0, 3];
            var s3 = a[0, 1] * a[1, 2] - a[1, 1] * a[0, 2];
            var s4 = a[0, 1] * a[1, 3] - a[1, 1] * a[0, 3];
            var s5 = a[0, 2] * a[1, 3] - a[1, 2] * a[0, 3];

            var c5 = a[2, 2] * a[3, 3] - a[3, 2] * a[2, 3];
            var c4 = a[2, 1] * a[3, 3] - a[3, 1] * a[2, 3];
            var c3 = a[2, 1] * a[3, 2] - a[3, 1] * a[2, 2];
            var c2 = a[2, 0] * a[3, 3] - a[3, 0] * a[2, 3];
            var c1 = a[2, 0] * a[3, 2] - a[3, 0] * a[2, 2];
            var c0 = a[2, 0] * a[3, 1] - a[3, 0] * a[2, 1];

            // Should check for 0 determinant
            var invdet = 1.0 / (s0 * c5 - s1 * c4 + s2 * c3 + s3 * c2 - s4 * c1 + s5 * c0);

            var b = new double[4, 4];

            b[0, 0] = (a[1, 1] * c5 - a[1, 2] * c4 + a[1, 3] * c3) * invdet;
            b[0, 1] = (-a[0, 1] * c5 + a[0, 2] * c4 - a[0, 3] * c3) * invdet;
            b[0, 2] = (a[3, 1] * s5 - a[3, 2] * s4 + a[3, 3] * s3) * invdet;
            b[0, 3] = (-a[2, 1] * s5 + a[2, 2] * s4 - a[2, 3] * s3) * invdet;

            b[1, 0] = (-a[1, 0] * c5 + a[1, 2] * c2 - a[1, 3] * c1) * invdet;
            b[1, 1] = (a[0, 0] * c5 - a[0, 2] * c2 + a[0, 3] * c1) * invdet;
            b[1, 2] = (-a[3, 0] * s5 + a[3, 2] * s2 - a[3, 3] * s1) * invdet;
            b[1, 3] = (a[2, 0] * s5 - a[2, 2] * s2 + a[2, 3] * s1) * invdet;

            b[2, 0] = (a[1, 0] * c4 - a[1, 1] * c2 + a[1, 3] * c0) * invdet;
            b[2, 1] = (-a[0, 0] * c4 + a[0, 1] * c2 - a[0, 3] * c0) * invdet;
            b[2, 2] = (a[3, 0] * s4 - a[3, 1] * s2 + a[3, 3] * s0) * invdet;
            b[2, 3] = (-a[2, 0] * s4 + a[2, 1] * s2 - a[2, 3] * s0) * invdet;

            b[3, 0] = (-a[1, 0] * c3 + a[1, 1] * c1 - a[1, 2] * c0) * invdet;
            b[3, 1] = (a[0, 0] * c3 - a[0, 1] * c1 + a[0, 2] * c0) * invdet;
            b[3, 2] = (-a[3, 0] * s3 + a[3, 1] * s1 - a[3, 2] * s0) * invdet;
            b[3, 3] = (a[2, 0] * s3 - a[2, 1] * s1 + a[2, 2] * s0) * invdet;

            return b;
        }

        double[,] MatrixMultiply(double[,] a, double[,] b)
        {
            int row_size = a.GetLength(0);
            int col_size = b.GetLength(1);
            int inner_size = a.GetLength(1);

            // TODO: Osetrit, zda se da nasobit!!!

            double[,] res = new double[row_size, col_size];

            for (var row = 0; row < row_size; ++row)
            {
                for (var col = 0; col < col_size; ++col)
                {
                    for (var inner = 0; inner < inner_size; ++inner)
                    {
                        res[row, col] += a[row, inner] * b[inner, col];
                    }
                }
            }

            return res;
        }

        double[,] MatrixSum(double[,] a, double[,] b)
        {
            int row = a.GetLength(0);
            int col = a.GetLength(1);

            double[,] res = new double[row, col];

            for (var i = 0; i < row; ++i)
            {
                for (var j = 0; j < col; ++j)
                {
                    res[i, j] = a[i, j] + b[i, j];
                }
            }

            return res;
        }

        
        // TODO: Zmenit na spravny datovy typ!!!!
        List<FingerPrintPair> FindCoincidentPoints(FingerPrintFeatureVector extracted, FingerPrintFeatureVector templated)
        {
            // A vzor
            // B obraz
            // TODO: Na tuhle hodnotu pozor, muze zpusobovat chybu!!!!
            double d_min = 175; // Minimalni delka dvojice bodu pro zaraazeni do seznamu dvojic
            double d_a;
            double d_b;

            FingerPrintMinutiae a_i;
            FingerPrintMinutiae a_l;

            FingerPrintMinutiae b_j;
            FingerPrintMinutiae b_k;

            List<FingerPrintPair> D_a = new List<FingerPrintPair>();
            List<FingerPrintPair> D_b = new List<FingerPrintPair>();

            FingerPrintPair pair_a;
            FingerPrintPair pair_b;

            if (templated.Minutiaes.Count > 50 || extracted.Minutiaes.Count > 50)
            {
                List<FingerPrintPair> err = new List<FingerPrintPair>();
                return err;
            }

            // Krok 1: Vytvoreni seznamu dvojic bodu D_a a D_b
            for (var i = 0; i < templated.Minutiaes.Count - 1; ++i)
            {
                for (var l = i + 1; l < templated.Minutiaes.Count; ++l)
                {
                    a_i = templated.Minutiaes.ElementAt(i);
                    a_l = templated.Minutiaes.ElementAt(l);

                    d_a = Math.Sqrt(Math.Pow(a_i.PositionX - a_l.PositionX, 2) + Math.Pow(a_i.PositionY - a_l.PositionY, 2));

                    if (d_a > d_min)
                    {
                        pair_a = new FingerPrintPair();

                        pair_a.PosX = a_i.PositionX;
                        pair_a.PosY = a_i.PositionY;

                        pair_a.PosX_2 = a_l.PositionX;
                        pair_a.PosY_2 = a_l.PositionY;

                        pair_a.Distance = d_a;

                        D_a.Add(pair_a);
                    }
                }
            }

            for (var j = 0; j < extracted.Minutiaes.Count - 1; ++j)
            {
                for (var k = j + 1; k < extracted.Minutiaes.Count; ++k)
                {
                    b_j = extracted.Minutiaes.ElementAt(j);
                    b_k = extracted.Minutiaes.ElementAt(k);

                    d_b = Math.Sqrt(Math.Pow(b_j.PositionX - b_k.PositionX, 2) + Math.Pow(b_j.PositionY - b_k.PositionY, 2));

                    if (d_b > d_min)
                    {
                        pair_b = new FingerPrintPair();

                        pair_b.PosX = b_j.PositionX;
                        pair_b.PosY = b_j.PositionY;

                        pair_b.PosX_2 = b_k.PositionX;
                        pair_b.PosY_2 = b_k.PositionY;

                        pair_b.Distance = d_b;

                        D_b.Add(pair_b);
                    }
                }
            }

            // Krok 2: Nalezeni nejlepsi dvojice bodu (x_1, y_1), (x_2, y_2) a (xi_1, eta_1), (xi_2, eta_2)
            //         a naplneni seznamu shodnych bodu S_best pro dalsi krok transformace.
            int eps = 15; // Tolerance vzdalenosti je 15px
            int n_best = 0;
            int p = 0;

            int x_1;
            int y_1;
            int x_2;
            int y_2;

            int xi_1;
            int eta_1;
            int xi_2;
            int eta_2;

            //int[,] Beta = new int[4, 1];
            //int[,] X = new int[4, 4];
            //int[,] K = new int[4, 1];
            double[,] Beta;
            double[,] X = new double[4, 4];
            double[,] X_inv;
            double[,] K = new double[4, 1];
            double[,] point = new double[2, 1];
            double[,] beta_matrix_22 = new double[2, 2];
            double[,] beta_matrix_21 = new double[2, 1];
            double[,] beta22_point_mult;
            double[,] beta21_bpm;
            
            int xi_hat_i;
            int eta_hat_i;

            FingerPrintPair D_a_r;
            FingerPrintPair D_b_s;

            List<FingerPrintPair> S_best = new List<FingerPrintPair>();
            List<FingerPrintPair> S_pom = new List<FingerPrintPair>();

            for (var r = 0; r < D_a.Count; ++r)
            {
                for (var s = 0; s < D_b.Count; ++s)
                {
                    D_a_r = D_a.ElementAt(r);
                    D_b_s = D_b.ElementAt(s);

                    d_a = D_a_r.Distance;
                    d_b = D_b_s.Distance;

                    // Prilis vzdaleno od sebe
                    if (d_a > (d_b + eps) && d_a < (d_b - eps))
                        continue;

                    S_pom = new List<FingerPrintPair>();
                    p = 0;

                    x_1 = D_a_r.PosX;
                    y_1 = D_a_r.PosY;
                    x_2 = D_a_r.PosX_2;
                    y_2 = D_a_r.PosY_2;

                    xi_1 = D_b_s.PosX;
                    eta_1 = D_b_s.PosY;
                    xi_2 = D_b_s.PosX_2;
                    eta_2 = D_b_s.PosY_2;

                    // X
                    X[0, 0] = 1;
                    X[0, 1] = 0;
                    X[0, 2] = x_1;
                    X[0, 3] = y_1;

                    X[1, 0] = 0;
                    X[1, 1] = 1;
                    X[1, 2] = y_1;
                    X[1, 3] = -x_1;

                    X[2, 0] = 1;
                    X[2, 1] = 0;
                    X[2, 2] = x_2;
                    X[2, 3] = y_2;

                    X[3, 0] = 0;
                    X[3, 1] = 1;
                    X[3, 2] = y_2;
                    X[3, 3] = -x_2;

                    // K
                    K[0, 0] = xi_1;
                    K[1, 0] = eta_1;
                    K[2, 0] = xi_2;
                    K[3, 0] = eta_2;

                    X_inv = GetInverse(X);

                    Beta = MatrixMultiply(X_inv, K);

                    for (var i = 0; i < templated.Minutiaes.Count; ++i)
                    {
                        a_i = templated.Minutiaes.ElementAt(i);
                        point[0, 0] = a_i.PositionX;
                        point[1, 0] = a_i.PositionY;

                        beta_matrix_22[0, 0] = Beta[2, 0];
                        beta_matrix_22[0, 1] = Beta[3, 0];
                        beta_matrix_22[1, 0] = -Beta[3, 0];
                        beta_matrix_22[1, 1] = Beta[2, 0];

                        beta22_point_mult = MatrixMultiply(beta_matrix_22, point);

                        beta_matrix_21[0, 0] = Beta[0, 0];
                        beta_matrix_21[1, 0] = Beta[1, 0];

                        beta21_bpm = MatrixSum(beta_matrix_21, beta22_point_mult);

                        xi_hat_i = Convert.ToInt32(beta21_bpm[0, 0]);
                        eta_hat_i = Convert.ToInt32(beta21_bpm[1, 0]);

                        for (var j = 0; j < extracted.Minutiaes.Count; ++j)
                        {
                            b_j = extracted.Minutiaes.ElementAt(j);

                            // Shodne body
                            if (Math.Sqrt(Math.Pow(b_j.PositionX - xi_hat_i, 2) + Math.Pow(b_j.PositionY - eta_hat_i, 2)) < eps)
                            {
                                FingerPrintPair coincident_pair = new FingerPrintPair();

                                coincident_pair.PosX = a_i.PositionX;
                                coincident_pair.PosY = a_i.PositionY;

                                coincident_pair.PosX_2 = b_j.PositionX;
                                coincident_pair.PosY_2 = b_j.PositionY;

                                S_pom.Add(coincident_pair);
                                p++;
                            }
                        }
                    }
                }

                if (p > n_best)
                {
                    n_best = p;
                    // ZDE pozor!!! Reference jsou svina
                    S_best = S_pom;
                }
            }
            /*
            // Krok 3: Iterativni geometricka transformace
            while (true)
            {
                double Beta_hat_1;
                double Beta_hat_2;
                double Beta_hat_3;
                double Beta_hat_4;

                double x_avg = 0;
                double y_avg = 0;
                double xi_avg = 0;
                double eta_avg = 0;

                double x_i = 0;
                double y_i = 0;
                double xi_i = 0;
                double eta_i = 0;

                double Beta3_numerator = 0;
                double Beta4_numerator = 0;
                double Beta34_divisor = 0;

                double[,] beta_hat_matrix_22 = new double[2, 2];
                double[,] beta_hat_matrix_21 = new double[2, 1];
                double[,] beta22_hat_point_mult;
                double[,] beta21_hat_bpm;

                S_pom.Clear();
                p = 0;

                // Vypocet Beta_hat
                for (var n = 0; n < S_best.Count; ++n)
                {
                    x_avg += S_best.ElementAt(n).PosX;
                    y_avg += S_best.ElementAt(n).PosY;

                    xi_avg += S_best.ElementAt(n).PosX_2;
                    eta_avg += S_best.ElementAt(n).PosY_2;
                }
                x_avg /= S_best.Count;
                y_avg /= S_best.Count;
                xi_avg /= S_best.Count;
                eta_avg /= S_best.Count;

                // TODO: Zkontrolovat!!!
                for (var n = 0; n < S_best.Count; ++n)
                {
                    x_i = S_best.ElementAt(n).PosX;
                    y_i = S_best.ElementAt(n).PosY;
                    xi_i = S_best.ElementAt(n).PosX_2;
                    eta_i = S_best.ElementAt(n).PosY_2;

                    Beta3_numerator += (x_i - x_avg) * (xi_i - xi_avg) + (y_i - y_avg) * (eta_i - eta_avg);
                    Beta34_divisor += Math.Pow(x_i - x_avg, 2) + Math.Pow(y_i - y_avg, 2);

                    Beta4_numerator += (y_i - y_avg) * (xi_i - xi_avg) - (x_i - x_avg) * (eta_i - eta_avg);
                }

                Beta_hat_3 = Beta3_numerator / Beta34_divisor;
                Beta_hat_4 = Beta4_numerator / Beta34_divisor;

                Beta_hat_1 = xi_avg - x_avg * Beta_hat_3 - y_avg * Beta_hat_4;
                Beta_hat_2 = eta_avg + x_avg * Beta_hat_4 - y_avg * Beta_hat_3;

                for (var i = 0; i < templated.Minutiaes.Count; ++i)
                {
                    a_i = templated.Minutiaes.ElementAt(i);
                    point[0, 0] = a_i.PositionX;
                    point[1, 0] = a_i.PositionY;

                    beta_hat_matrix_22[0, 0] = Beta_hat_3;
                    beta_hat_matrix_22[0, 1] = Beta_hat_4;
                    beta_hat_matrix_22[1, 0] = -Beta_hat_4;
                    beta_hat_matrix_22[1, 1] = Beta_hat_3;

                    beta22_hat_point_mult = MatrixMultiply(beta_hat_matrix_22, point);

                    beta_hat_matrix_21[0, 0] = Beta_hat_1;
                    beta_hat_matrix_21[1, 0] = Beta_hat_2;

                    beta21_hat_bpm = MatrixSum(beta_hat_matrix_21, beta22_hat_point_mult);

                    xi_hat_i = Convert.ToInt32(beta21_hat_bpm[0, 0]);
                    eta_hat_i = Convert.ToInt32(beta21_hat_bpm[1, 0]);

                    // TODO:
                    for (var j = 0; j < extracted.Minutiaes.Count; ++j)
                    {
                        b_j = extracted.Minutiaes.ElementAt(j);

                        // Shodne body
                        if (Math.Sqrt(Math.Pow(b_j.PositionX - xi_hat_i, 2) + Math.Pow(b_j.PositionY - eta_hat_i, 2)) < eps)
                        {
                            FingerPrintPair coincident_pair = new FingerPrintPair();

                            coincident_pair.PosX = a_i.PositionX;
                            coincident_pair.PosY = a_i.PositionY;

                            coincident_pair.PosX_2 = b_j.PositionX;
                            coincident_pair.PosY_2 = b_j.PositionY;

                            S_pom.Add(coincident_pair);
                            p++;
                        }
                    }
                }
                S_best = S_pom;

                if (p > n_best)
                    n_best = p;
                else
                    break;
            }*/

            return S_best;
        }
        
        

        public MatchingScore computeMatchingScore(FingerPrintFeatureVector extracted, FingerPrintFeatureVector templated)
        {
            double sum = 0;

            // TODO: Zmenit na spravny datovy typ!!!!
            //List<FingerPrintPair> coincident_points = new List<FingerPrintPair>();

            List<FingerPrintPair> coincident_points = FindCoincidentPoints(extracted, templated);

            /*
            if (extracted.FeatureVector.Size != templated.FeatureVector.Size ||
                extracted.FeatureVector.Cols != 1 || templated.FeatureVector.Cols != 1)
                throw new ArgumentException("Feature vector and template mismatch.");

            var n = extracted.FeatureVector.Rows;
            for (var i = 0; i < n; i++)
            {
                sum += Math.Abs(extracted.FeatureVector[i, 0] - templated.FeatureVector[i, 0]);
            }
            */
            sum = coincident_points.Count;

            if (coincident_points.Count >= 20)
                sum = 100;

            return new MatchingScore(sum);
        }
    }
}
