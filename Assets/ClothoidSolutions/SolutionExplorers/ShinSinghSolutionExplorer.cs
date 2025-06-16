using UnityEngine;
using System.Collections.Generic;
namespace Clothoid {
    public class ShinSinghSolutionExplorer : MonoBehaviour {

        public Vector3 start = Vector3.zero;
        public Vector3 end = new Vector3(10, 0, 8);
        public float startCurvature = 0.1f;
        public float endCurvature = 0.3f;
        public float endAngle = 25;

        private GameObject startGO;
        private GameObject endGO;
        public LineRenderer startLR;
        public LineRenderer endLR;
        public LineRenderer curveLR;
        public LineRenderer pointListLR;

        private bool awake = false;

        private IEnumerator<ClothoidCurve> nextCurve;
        private List<Vector3> pointList = new List<Vector3>() {new Vector3(1, 0, 3), new Vector3(5, 0, 8), new Vector3(13, 0, 2), new Vector3(15, 0, -6), new Vector3(17, 0, -12)};
        private ClothoidSolutionShinSingh solution;

        void Start() {
            awake = true;

            Helpers.DrawOrderedVector3s(pointList, this.pointListLR);
            solution = new ClothoidSolutionShinSingh();
            solution.CalculateClothoidCurve(pointList);
            nextCurve = solution.SolveClothoidParameters();

            //SetupVisuals();
        }

        void SetupVisuals() {
            //Draw start and end point, label curvature and tangent
            startGO.transform.position = start;
            Helpers.DrawOrderedVector3s(new List<Vector3>(){start, start + (Helpers.GetTangent(0) * 3).ToUnityVector3()}, this.startLR);

            endGO.transform.position = new Vector3(end.x, 0, end.z);
            Helpers.DrawOrderedVector3s(new List<Vector3>(){end, end + (Helpers.GetTangentDeg(endAngle) * 3).ToUnityVector3()}, this.endLR);
        }

        void OnValidate() {
            if (!awake) return;

            SetupVisuals();
        }

        [ContextMenu("Get Next Curve Approximation")]
        public void TestPointList() {
            if (nextCurve.MoveNext()) {
                ClothoidCurve c = nextCurve.Current;
                //Debug.Log(c.ToString());
                List<System.Numerics.Vector3> samples = c.GetSamples(100 * c.Count);
                //Debug.Log(samples.Count);
                //segmentedLKLR.startWidth = .1f;
                //segmentedLKLR.endWidth = .1f;
                Helpers.DrawOrderedVector3s(samples, this.curveLR);
            } else {
                Debug.Log("Can't move next!");
            }
        }

    }
}