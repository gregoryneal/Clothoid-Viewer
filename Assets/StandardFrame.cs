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
        public StandardFrame(params HermiteData[] points)
        {
            this.points = points;
        }

        public static HermiteData[] CastToStandardFrame(HermiteData[] points) {
            HermiteData[] p = new HermiteData[points.Length];
            for (int i = 0; i < points.Length; i++)
            {

            }
            return p;
        }

        private static Vector3 Rotate(Vector3 point, float angle) {
            return ClothoidSegment.RotateAboutAxisDeg(point, Vector3.UnitY, angle);
        }
    }
}