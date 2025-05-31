using System;
using System.Collections.Generic;
using System.Numerics;

namespace Clothoid
{
    public class ClothoidSolutionBertolazzi : ClothoidSolution
    {


        const double EPS = 1e-15;
        const double PI = Math.PI;
        const double PI_2 = Math.PI / 2;


        static readonly double[] fn = new double[11] {
            0.49999988085884732562,
            1.3511177791210715095,
            1.3175407836168659241,
            1.1861149300293854992,
            0.7709627298888346769,
            0.4173874338787963957,
            0.19044202705272903923,
            0.06655998896627697537,
            0.022789258616785717418,
            0.0040116689358507943804,
            0.0012192036851249883877
        };
        static readonly double[] fd = new double[12] {
            1.0,
            2.7022305772400260215,
            4.2059268151438492767,
            4.5221882840107715516,
            3.7240352281630359588,
            2.4589286254678152943,
            1.3125491629443702962,
            0.5997685720120932908,
            0.20907680750378849485,
            0.07159621634657901433,
            0.012602969513793714191,
            0.0038302423512931250065
        };
        static readonly double[] gn = new double[11] {
            0.50000014392706344801,
            0.032346434925349128728,
            0.17619325157863254363,
            0.038606273170706486252,
            0.023693692309257725361,
            0.007092018516845033662,
            0.0012492123212412087428,
            0.00044023040894778468486,
            -8.80266827476172521e-6,
            -1.4033554916580018648e-8,
            2.3509221782155474353e-10
        };
        static readonly double[] gd = new double[12]{
            1.0,
            2.0646987497019598937,
            2.9109311766948031235,
            2.6561936751333032911,
            2.0195563983177268073,
            1.1167891129189363902,
            0.57267874755973172715,
            0.19408481169593070798,
            0.07634808341431248904,
            0.011573247407207865977,
            0.0044099273693067311209,
            -0.00009070958410429993314
        };



        /// <summary>
        /// Allowable error in the solutions
        /// </summary>
        public static double A_THRESHOLD = .01;
        public static int A_SERIE_SIZE = 3;
        private static double ROOT_TOLERANCE = 1E-2;
        private static double ROOT_TOLERANCE_FORGIVENESS = 1; //can't find a root with tolerance ROOT_TOLERANCE? use this one as a secondary "at least" condition.
        private static readonly double[] CF = new double[6] { 2.989696028701907, 0.716228953608281, -0.458969738821509, -0.502821153340377, 0.261062141752652, -0.045854475238709 };

        private float solvedCurvature = 0;
        private float solvedSharpness = 0;
        public override ClothoidCurve CalculateClothoidCurve(List<UnityEngine.Vector3> inputPolyline, float allowableError = 0.1F, float endpointWeight = 1)
        {
            throw new System.NotImplementedException();
        }

        public static ClothoidCurve BuildSegmentsFromStandardFrame(HermiteData point1, HermiteData point2)
        {
            List<ClothoidSegment> segments = new List<ClothoidSegment>();

            if (point1.x != -1 || point1.z != 0 || point2.x != 1 || point2.z != 0) return ClothoidCurve.FromSegments(segments);




            return ClothoidCurve.FromSegments(segments);
        }

        public static float[] SolveG1Parameters(HermiteData point1, HermiteData point2)
        {
            ClothoidCurve c = G1Curve(point1.x, point1.z, point1.tangentAngle, point2.x, point2.z, point2.tangentAngle, true);
            return new float[4] { c[0].Sharpness, c[0].StartCurvature, c.AngleOffset, c.TotalArcLength };
        }

        public static float[] SolveG1Parameters(ClothoidCurve curve)
        {
            return new float[4] { curve[0].Sharpness, curve[0].StartCurvature, curve.AngleOffset, curve.TotalArcLength };
        }

        public static ClothoidCurve G1Spline(Posture[] data)
        {
            ClothoidCurve c = new ClothoidCurve();

            for (int i = 0; i + 1 < data.Length; i++)
            {
                c += G1Curve(data[i].X, data[i].Z, data[i].Angle, data[i + 1].X, data[i + 1].Z, data[i + 1].Angle);
            }

            c.Offset = data[0].Position;
            c.AngleOffset = data[0].Angle;

            return c;
        }

        /// <summary>
        /// Get a G1 clothoid curve using two points and associated tangents.
        /// Angles should be in degrees.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="z0"></param>
        /// <param name="t0"></param>
        /// <param name="x1"></param>
        /// <param name="z1"></param>
        /// <param name="t1"></param>
        /// <param name="addOffsets">If true, the returned curve will be offet and rotated by the first point and its tangent. We might want to leave it false if we are building a G1 spline, in which case we would only offset and rotate the entire G1 curve by the first point after building the whole thing. See <see cref="G1Spline"/></param>
        /// <returns></returns>
        public static ClothoidCurve G1Curve(double x0, double z0, double t0, double x1, double z1, double t1, bool addOffsets = false)
        {
            double dx = x1 - x0;
            double dz = z1 - z0;

            double phi = Math.Atan2(dz, dx);
            double phi_deg = phi * 180 / Math.PI;
            double r = Math.Sqrt((dx * dx) + (dz * dz));

            double phi0 = NormalizeAngle((t0 * Math.PI / 180) - phi);
            double phi1 = NormalizeAngle((t1 * Math.PI / 180) - phi);

            double d = phi1 - phi0;

            double g;
            double dg;
            List<double[]> IntCS;
            int u = 0;

            double A = InitialGuessA(phi0, phi1);

            /*
            //Line segment
            if (A < A_THRESHOLD && d - A < A_THRESHOLD)
            {
                ClothoidCurve cc = new ClothoidCurve().AddSegment(new ClothoidSegment(0, 0, (float)r));
                cc.Offset = point1.Position.ToUnityVector3();
                cc.AngleOffset = (float)point1.tangentAngle;
                return cc;
            }
            else if (A < A_THRESHOLD)
            {
            }*/

            do
            {
                //if (isMirrored && !isReversed) IntCS = GeneralizedFresnelCS(3, 2 * A, -d - A, -phi0);
                //else if (isReversed && !isMirrored) IntCS = GeneralizedFresnelCS(3, 2 * A, d - A, -phi1);
                //else if (isReversed && isMirrored) IntCS = GeneralizedFresnelCS(3, 2 * A, d - A, -phi1);
                IntCS = GeneralizedFresnelCS(3, 2 * A, d - A, phi0);
                g = IntCS[1][0];
                dg = IntCS[0][2] - IntCS[0][1];
                A -= g / dg;//
            } while (++u < 20 && Math.Abs(g) > ROOT_TOLERANCE);

            if (Math.Abs(g) > ROOT_TOLERANCE)
            {
                UnityEngine.Debug.LogWarning($"No root found! (g, tol, tol2): ({g}, {ROOT_TOLERANCE}, {ROOT_TOLERANCE_FORGIVENESS})");
                //Assert.IsTrue(Math.Abs(g) <= ROOT_TOLERANCE);
                //could not find a root
                return new ClothoidCurve();
            }

            //UnityEngine.Debug.Log($"Number of attempts: {u}");
            double[] intCS;

            intCS = GeneralizedFresnelCS(2 * A, d - A, phi0);
            double s = r / intCS[0];


            //Assert.IsTrue(s > 0);
            if (s <= 0)
            {
                UnityEngine.Debug.LogWarning($"ArcLength Negative or Zero! s: {s} | r: {r} | intCS[0]: {intCS[0]} | intCS[1]: {intCS[1]} | phi0: {phi0 * 180 / Math.PI} | phi1: {phi1 * 180 / Math.PI} | A: {A} | d: {d * 180 / Math.PI}");
                if (s <= 0) return new ClothoidCurve();
            }

            double startCurvature = (d - A) / s;
            double sharpness = (2 * A) / (s * s);

            //ClothoidSegment segment = new ClothoidSegment((float)startCurvature, (float)sharpness, (float)s);

            ClothoidCurve c = new ClothoidCurve().AddSegment(new ClothoidSegment((float)startCurvature, (float)sharpness, (float)s));
            if (addOffsets)
            {
                c.Offset = new UnityEngine.Vector3((float)x0, 0, (float)z0);
                c.AngleOffset = (float)t0;
            }
            return c;
        }

        /// <summary>
        /// Normalize an angle in radians to be between -pi and pi.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        private static double InitialGuessA(double phi0, double phi1)
        {
            double X = phi0 / Math.PI;
            double Y = phi1 / Math.PI;
            double xy = X * Y;
            X *= X;
            Y *= Y;
            //double c1 = 3.070645;
            //double c2 = 0.947923;
            //double c3 = -0.673029;
            //return (phi0 + phi1) * (3.070645 + (0.947923 * X * Y) + (-0.673029 * ((X * X) + (Y * Y))));
            return (phi0 + phi1) * (CF[0] + xy * (CF[1] + xy * CF[2]) + (CF[3] + xy * CF[4]) * (X + Y) + CF[5] * (X * X + Y * Y));
        }

        public static double RLommel(double mu, double v, double b)
        {

            double t = 1 / ((mu + v + 1) * (mu - v + 1));
            double r = t;
            double n = 1;
            double e = .00000000001;

            while (Math.Abs(t) > e * Math.Abs(r))
            {
                t *= ((-b) / ((2 * n) + mu - v + 1)) * (b / ((2 * n) + mu + v + 1));
                r += t;
                n++;
            }

            return r;
        }

        /// <summary>
        /// Evaluate XY when a is 0
        /// </summary>
        /// <param name="b"></param>
        /// <param name="k"></param>
        /// <param name="e">The allowable error</param>
        /// <returns></returns>
        private static List<double[]> EvalXYaZero(double b, int k)
        {
            double b2 = b * b;
            double sb = Math.Sin(b);
            double cb = Math.Cos(b);

            double[] X = new double[45];
            double[] Y = new double[45];

            if (Math.Abs(b) < 1E-3)
            {
                X[0] = 1 - (b2 / 6) * (1 - (b2 / 20) * (1 - (b2 / 42)));
                Y[0] = (b2 / 2) * (1 - (b2 / 12) * (1 - (b2 / 30)));
            }
            else
            {
                X[0] = sb / b;
                Y[0] = (1 - cb) / b;
            }

            //int m = (int)Math.Min(Math.Max(1, Math.Floor(2 * b)), k);
            int m = (int)Math.Floor(2 * b);

            if (m >= k) m = k - 1;
            if (m < 1) m = 1;

            for (int i = 1; i < m; ++i)
            {
                X[i] = (sb - (i * Y[i - 1])) / b;
                Y[i] = ((i * X[i - 1]) - cb) / b;
            }

            if (m < k)
            {
                double A = b * sb;
                double D = sb - (b * cb);
                double B = b * D;
                double C = -b2 * sb;
                double rLa = RLommel(m + 0.5, 1.5, b);
                double rLd = RLommel(m + 0.5, 0.5, b);
                double rLb;
                double rLc;

                for (int i = m; i < k; ++i)
                {
                    rLb = RLommel(i + 1.5, 0.5, b);
                    rLc = RLommel(i + 1.5, 1.5, b);
                    //UnityEngine.Debug.LogWarning($"i, m, k: {i}, {m}, {k}");
                    X[i] = ((i * A * rLa) + (B * rLb) + cb) / (1 + i);
                    Y[i] = (((C * rLc) + sb) / (2 + i)) + (D * rLd);
                    rLa = rLc;
                    rLd = rLb;
                    //
                }

            }

            return new List<double[]>() { X, Y };
        }

        /// <summary>
        /// Eval XY when a is small and k is 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="k"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static double[] EvalXYaSmall(double a, double b, int p)
        {
            //Debug.Assert(p > 0 && p < 11);

            List<double[]> points = EvalXYaZero(b, (4 * p) + 2);
            double[] X0 = points[0];
            double[] Y0 = points[1];

            double X = X0[0] - (a / 2) * Y0[2];
            double Y = Y0[0] + (a / 2) * X0[2];

            double t = 1;
            double aa = -a * a / 4;
            double bf;
            int jj;

            for (int i = 1; i <= p; i++)
            {
                t *= aa / (2 * i * ((2 * i) - 1));
                bf = a / ((4 * i) + 2);
                jj = 4 * i;
                X += t * (X0[jj] - (bf * Y0[jj + 2]));
                Y += t * (Y0[jj] + (bf * X0[jj + 2]));
            }

            return new double[2] { X, Y };
        }

        private static List<double[]> EvalXYaSmall(int k, double a, double b, int p)
        {

            int nkk = k + (4 * p) + 2;
            List<double[]> points = EvalXYaZero(b, nkk);
            double[] X0 = points[0];
            double[] Y0 = points[1];
            double[] X = new double[3];
            double[] Y = new double[3];

            for (int j = 0; j < k; j++)
            {
                X[j] = X0[j] - (a / 2) * Y0[j + 2];
                Y[j] = Y0[j] + (a / 2) * X0[j + 2];
            }

            double t = 1;
            double aa = -a * a / 4;
            double bf;
            int jj;
            for (int n = 1; n <= p; n++)
            {
                t *= aa / (2 * n * ((2 * n) - 1));
                bf = a / ((4 * n) + 2);

                for (int j = 0; j < k; j++)
                {
                    jj = (4 * n) + j;
                    X[j] += t * (X0[jj] - (bf * Y0[jj + 2]));
                    Y[j] += t * (Y0[jj] + (bf * X0[jj + 2]));
                }
            }

            return new List<double[]>() { X, Y };
        }

        private static double[] EvalXYaLarge(double a, double b)
        {
            List<double[]> XY = EvalXYaLarge(0, a, b);
            return new double[2] { XY[0][0], XY[1][0] };
        }

        private static List<double[]> EvalXYaLarge(int nk, double a, double b)
        {
            if (nk > 3 || nk < 0) throw new ArgumentOutOfRangeException();

            double s = a > 0 ? 1 : -1;
            double absa = Math.Abs(a);
            double z = Math.Sqrt(absa) / Math.Sqrt(Math.PI);
            double ell = s * b / (Math.Sqrt(Math.PI) * Math.Sqrt(absa));
            double g = -s * b * b / (2 * absa);
            double cg = Math.Cos(g) / z;
            double sg = Math.Sin(g) / z;
            List<double[]> CS1 = FresnelCS(nk, ell);
            List<double[]> CSz = FresnelCS(nk, ell + z);
            double[] C1 = CS1[0];
            double[] S1 = CS1[1];
            double[] Cz = CSz[0];
            double[] Sz = CSz[1];

            double dC0 = Cz[0] - C1[0];
            double dS0 = Sz[0] - S1[0];

            double[] X = new double[3];
            double[] Y = new double[3];

            //UnityEngine.Debug.LogWarning($"Number of elements in CS1: ({C1.Length}, {S1.Length}) | in CSZ: ({Cz.Length}, {Sz.Length})");

            X[0] = (cg * dC0) - (s * sg * dS0);
            Y[0] = (sg * dC0) + (s * cg * dS0);
            if (nk > 1)
            {
                cg /= z;
                sg /= z;
                double dC1 = Cz[1] - C1[1];
                double dS1 = Sz[1] - S1[1];
                double DC = dC1 - (ell * dC0);
                double DS = dS1 - (ell * dS0);

                X[1] = (cg * DC) - (s * sg * DS);
                Y[1] = (sg * DC) + (s * cg * DS);

                if (nk > 2)
                {
                    double dC2 = Cz[2] - C1[2];
                    double dS2 = Sz[2] - S1[2];
                    DC = dC2 + (ell * ((ell * dC0) - (2 * dC1)));
                    DS = dS2 + (ell * ((ell * dS0) - (2 * dS1)));
                    cg /= z;
                    sg /= z;
                    X[2] = (cg * DC) - (s * sg * DS);
                    Y[2] = (sg * DC) + (s * cg * DS);
                }
            }

            return new List<double[]>() { X, Y };
        }

        /// <summary>
        /// Evaluate the fresnel integral and its momenta at arc length t
        /// </summary>
        /// <param name="nk"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<double[]> FresnelCS(int nk, double t)
        {
            double[] C = new double[3];
            double[] S = new double[3];
            FresnelCS(t, out double c, out double s);
            C[0] = c;
            S[0] = s;

            if (nk > 1)
            {
                C[1] = C1(t);
                S[1] = S1(t);
                /*
                double tt = Math.PI * t * t / 2;
                double stt = Math.Sin(tt);
                double ctt = Math.Cos(tt);
                C[1] = stt / Math.PI;
                S[1] = (1 - ctt) / Math.PI;*/
                if (nk > 2)
                {
                    C[2] = C2(t); //((t * stt) - S[0]) / Math.PI;
                    S[2] = S2(t); //(C[0] - (t * ctt)) / Math.PI;
                }
            }

            return new List<double[]>() { C, S };
        }

        public static void FresnelCS(double y, out double C, out double S)
        {
            double x = Math.Abs(y);

            if (x < 1.0)
            {
                double term, sum;
                double s = PI_2 * (x * x);
                double t = -s * s;

                // Cosine integral series
                double twofn = 0.0;
                double fact = 1.0;
                double denterm = 1.0;
                double numterm = 1.0;
                sum = 1.0;
                do
                {
                    twofn += 2.0;
                    fact *= twofn * (twofn - 1.0);
                    denterm += 4.0;
                    numterm *= t;
                    term = numterm / (fact * denterm);
                    sum += term;
                } while (Math.Abs(term) > EPS * Math.Abs(sum));

                C = x * sum;

                // Sine integral series
                twofn = 1.0;
                fact = 1.0;
                denterm = 3.0;
                numterm = 1.0;
                sum = 1.0 / 3.0;
                do
                {
                    twofn += 2.0;
                    fact *= twofn * (twofn - 1.0);
                    denterm += 4.0;
                    numterm *= t;
                    term = numterm / (fact * denterm);
                    sum += term;
                } while (Math.Abs(term) > EPS * Math.Abs(sum));

                S = PI_2 * sum * (x * x * x);
            }
            else if (x < 6.0)
            {
                // Rational approximation for f
                double sumn = 0.0;
                double sumd = fd[11];
                for (int k = 10; k >= 0; --k)
                {
                    sumn = fn[k] + x * sumn;
                    sumd = fd[k] + x * sumd;
                }
                double f = sumn / sumd;

                // Rational approximation for g
                sumn = 0.0;
                sumd = gd[11];
                for (int k = 10; k >= 0; --k)
                {
                    sumn = gn[k] + x * sumn;
                    sumd = gd[k] + x * sumd;
                }
                double g = sumn / sumd;

                double U = PI_2 * (x * x);
                double SinU = Math.Sin(U);
                double CosU = Math.Cos(U);

                C = 0.5 + f * SinU - g * CosU;
                S = 0.5 - f * CosU - g * SinU;
            }
            else
            {
                // x >= 6.0; asymptotic expansions for f and g
                double s = PI * x * x;
                double t = -1 / (s * s);

                // Expansion for f
                double numterm = -1.0;
                double term = 1.0;
                double sum = 1.0;
                double oldterm = 1.0;
                double eps10 = 0.1 * EPS;
                double absterm;

                do
                {
                    numterm += 4.0;
                    term *= numterm * (numterm - 2.0) * t;
                    sum += term;
                    absterm = Math.Abs(term);
                    if (oldterm < absterm)
                    {
                        throw new Exception($"In FresnelCS f not converged to eps, x = {x}, oldterm = {oldterm}, absterm = {absterm}");
                    }
                    oldterm = absterm;
                } while (absterm > eps10 * Math.Abs(sum));

                double f = sum / (PI * x);

                // Expansion for g
                numterm = -1.0;
                term = 1.0;
                sum = 1.0;
                oldterm = 1.0;

                do
                {
                    numterm += 4.0;
                    term *= numterm * (numterm + 2.0) * t;
                    sum += term;
                    absterm = Math.Abs(term);
                    if (oldterm < absterm)
                    {
                        throw new Exception($"In FresnelCS g not converged to eps, x = {x}, oldterm = {oldterm}, absterm = {absterm}");
                    }
                    oldterm = absterm;
                } while (absterm > eps10 * Math.Abs(sum));

                double g = sum / ((PI * x) * (PI * x * x));

                double U = PI_2 * (x * x);
                double SinU = Math.Sin(U);
                double CosU = Math.Cos(U);

                C = 0.5 + f * SinU - g * CosU;
                S = 0.5 - f * CosU - g * SinU;
            }

            if (y < 0)
            {
                C = -C;
                S = -S;
            }
        }

        private static List<double[]> GeneralizedFresnelCS(int nk, double a, double b, double c)
        {
            //UnityEngine.Debug.Log($"nk: {nk}, a: {a}, b: {b}, c: {c}");
            if (nk < 1 || nk > 3) throw new ArgumentOutOfRangeException($"Value of index: {nk} | Expected value between 1 and 3 inclusive.");
            double cc = Math.Cos(c);
            double sc = Math.Sin(c);
            List<double[]> CS;

            if (Math.Abs(a) < A_THRESHOLD) CS = EvalXYaSmall(nk, a, b, A_SERIE_SIZE);
            else CS = EvalXYaLarge(nk, a, b);

            double xx;
            double yy;

            for (int i = 0; i < nk; i++)
            {
                xx = CS[0][i];
                yy = CS[1][i];
                CS[0][i] = (xx * cc) - (yy * sc);
                CS[1][i] = (xx * sc) + (yy * cc);
            }

            return CS;
        }

        private static double[] GeneralizedFresnelCS(double a, double b, double c)
        {
            List<double[]> CS = GeneralizedFresnelCS(1, a, b, c);
            return new double[] { CS[0][0], CS[1][0] };
        }

        private static float C0(double t)
        {
            return Mathc.C((float)t);
        }

        private static float S0(double t)
        {
            return Mathc.S((float)t);
        }

        private static float C1(double t)
        {
            return (float)(Math.Sin(Math.PI * t * t / 2) / Math.PI);
        }

        private static float S1(double t)
        {
            return (float)((1 - Math.Cos(Math.PI * t * t / 2)) / Math.PI);
        }

        private static float C2(double t)
        {
            return (float)(((t * Math.Sin(Math.PI * t * t / 2)) - S0(t)) / Math.PI);
        }

        private static float S2(double t)
        {
            return (float)((C0(t) - (t * Math.Cos(Math.PI * t * t / 2))) / Math.PI);
        }

        public static List<UnityEngine.Vector3> Eval(float arcLength, float curvature, float sharpness, float startAngle, Vector3 start, int numSamples = 100)
        {
            double x0 = start.X;
            double y0 = start.Z;
            List<UnityEngine.Vector3> points = new List<UnityEngine.Vector3>();
            double[] XY;
            double increment = arcLength / numSamples;
            for (double s = 0; s < arcLength; s += increment)
            {
                XY = GeneralizedFresnelCS(sharpness * s * s, curvature * s, startAngle);
                points.Add(new Vector3((float)(x0 + (XY[0] * s)), 0, (float)(y0 + (XY[1] * s))).ToUnityVector3());
            }
            //add final point
            XY = GeneralizedFresnelCS(sharpness * arcLength * arcLength, curvature * arcLength, startAngle);
            points.Add(new Vector3((float)(x0 + (XY[0] * arcLength)), 0, (float)(y0 + (XY[1] * arcLength))).ToUnityVector3());

            return points;
        }
    }
}