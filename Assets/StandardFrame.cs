using System.Numerics;

namespace Clothoid
{
    /// <summary>
    /// Recast an input polyline into a sequence of so called "standard frame" segments, basically translate, rotate each two successive points and scale the curve such that the start and endpoints lie on (-1, 0) and (1, 0).
    /// This allows us to use Bertolazzi and Co's solution
    /// </summary>
    public class StandardFrame
    {
        HermiteData[] points;
        HermiteData[] scaledPoints;
        double[] d;
        double[] phi;
        double[] gamma;
        public StandardFrame(params HermiteData[] points)
        {
            this.points = points;
            d = new double[points.Length];
            phi = new double[points.Length];
            gamma = new double[points.Length];
        }

        public static HermiteData[] CastToStandardFrame(HermiteData[] points) {
            HermiteData[] p = new HermiteData[points.Length];
            double d;
            double phi;
            double gamma;
            for (int i = 0; i < points.Length; i++)
            {

            }
            return p;
        }

        private static Vector3 Rotate(Vector3 point, float angle) {
            return ClothoidSegment.RotateAboutAxis(point, Vector3.UnitY, angle);
        }
    }
}