using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clothoid
{
    public class BertolazziSolutionExplorer : MonoBehaviour
    {
        bool awake = false;

        public Vector3 start = new Vector3(-1, 0, 0);
        public Vector3 end = new Vector3(1, 0, 0);
        bool shouldDraw = false;
        public bool canvasDraw = false;
        List<Vector3> drawingPoints = new List<Vector3>();
        Vector3 lastSample;
        [Range(.2f, 2)]
        public float sampleArcLength = 1f;
        [Range(-180f, 180f)]
        public float startAngle = 60;
        [Range(-180f, 180f)]
        public float endAngle = 210;
        public LineRenderer startLR;
        public LineRenderer endLR;
        public LineRenderer curveLR;
        public LineRenderer curveLR2;

        void Start()
        {
            awake = true;
        }

        [ContextMenu("G1 Fit")]
        public void G1Fit()
        {
            HermiteData p1 = new HermiteData()
            {
                x = start.x,
                z = start.z,
                tangentAngle = startAngle,
                curvature = 0
            };
            HermiteData p2 = new HermiteData()
            {
                x = end.x,
                z = end.z,
                tangentAngle = endAngle,
                curvature = 0
            };

            ClothoidCurve c = G1(p1, p2);
            if (c.Count > 0)
            {
                //float[] p = ClothoidSolutionBertolazzi.SolveG1Parameters(c);
                //ClothoidCurve c = new ClothoidCurve();
                //c += new ClothoidSegment(0, arcLength, startCurvature, endCurvature);
                Helpers.DrawOrderedVector3s(c.GetSamples(100), curveLR2);
                //Helpers.DrawOrderedVector3s(ClothoidSolutionBertolazzi.Eval(p[3], p[1], p[0], p[2], new System.Numerics.Vector3((float)p1.x, 0, (float)p1.z), 100), curveLR);

               // Debug.Log(c.ToString());
            }
        }

        [ContextMenu("Angle Stress Test")]
        public void A()
        {
            StartCoroutine(G1AngleStressTest2());
        }

        [ContextMenu("Bertolazzi Tests")]
        public void _B()
        {
            StartCoroutine(B());
        }
        public IEnumerator B()
        {
            List<Vector3> vs = new List<Vector3>() { new Vector3(5, 0, 4), new Vector3(3, 0, 5), new Vector3(3, 0, 6), new Vector3(3, 0, 6), new Vector3(5, 0, 4), new Vector3(4, 0, 4) };
            List<Vector3> ve = new List<Vector3>() { new Vector3(5, 0, 6), new Vector3(6, 0, 5), new Vector3(6, 0, 6), new Vector3(6, 0, 6), new Vector3(4, 0, 5), new Vector3(5, 0, 5) };
            List<float> ts = new List<float>() { (float)Math.PI / 3, 2.14676f, 3.05433f, 0.08727f, 0.34907f, 0.52360f };
            List<float> te = new List<float>() { (float)Math.PI * 7 / 6, 2.86234f, 3.14159f, 3.05433f, 4.48550f, 4.66003f };

            HermiteData h1 = new HermiteData();
            HermiteData h2 = new HermiteData();
            h1.curvature = 0;
            h2.curvature = 0;
            Vector3 s;
            Vector3 e;
            ClothoidCurve c;
            for (int i = 0; i < vs.Count; i++)
            {
                h1.x = vs[i].x;
                h1.z = vs[i].z;
                h1.tangentAngle = ts[i] * 180 / Math.PI;
                h2.x = ve[i].x;
                h2.z = ve[i].z;
                h2.tangentAngle = te[i] * 180 / Math.PI;

                c = G1(h1, h2);
                if (c.Count > 0)
                {
                    s = h1.Position.ToUnityVector3();
                    e = h2.Position.ToUnityVector3();
                    Helpers.DrawOrderedVector3s(c.GetSamples(100), curveLR2);
                    Helpers.DrawOrderedVector3s(new List<Vector3>() { s, s + (Helpers.GetTangent((float)h1.tangentAngle) * .5f) }, this.startLR, 0, .1f);
                    Helpers.DrawOrderedVector3s(new List<Vector3>() { e, e + (Helpers.GetTangent((float)h2.tangentAngle) * .5f) }, this.endLR, 0, .1f);
                }
                else
                {
                    UnityEngine.Debug.Log($"Error with curve generation!");
                }

                yield return new WaitForSeconds(10f);
            }

            yield return null;
        }

        /// <summary>
        /// Run through all the possible values of phi0 and phi1 at a certain increment.
        /// </summary>
        /// <returns></returns>
        public IEnumerator G1AngleStressTest2()
        {
            ClothoidCurve c;
            float inc = 1f;

            for (float phi0 = -179; phi0 < 179; phi0 += inc)
            {
                for (float phi1 = Math.Abs(phi0) + inc; phi1 <= 180; phi1 += inc)
                {
                    Debug.Log($"phi0: {phi0} | phi1: {phi1}");
                    c = ClothoidSolutionBertolazzi.G1Curve(start.x, start.z, phi0, end.x, end.z, phi1, true);
                    Helpers.DrawOrderedVector3s(c.GetSamples(100), this.curveLR);
                    yield return new WaitForEndOfFrame();
                }
            }

            yield return null;
        }

        void Update()
        {
            //Drawing points
            if (Input.GetMouseButton(0))
            {
                shouldDraw = true;
            }
            else
            {
                //Reset the drawing points
                if (shouldDraw)
                {
                    Vector3[] points = new Vector3[drawingPoints.Count];
                    drawingPoints.CopyTo(points, 0);
                    drawingPoints.Clear();
                    shouldDraw = false;
                }
            }

            if (shouldDraw && canvasDraw)
            {
                Vector2 mousePixels = Input.mousePosition; //bottom left is 0,0
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePixels.x, mousePixels.y, Camera.main.transform.position.y));
                if (lastSample == null) lastSample = worldPos;
                else if (Vector3.Distance(worldPos, lastSample) >= sampleArcLength)
                {
                    drawingPoints.Add(worldPos);
                    //Helpers.DrawOrderedVector3s(drawingPoints, this.curveLR2);
                    lastSample = worldPos;
                }

                //Update curve
                if (drawingPoints.Count > 3)
                {
                    //MakeCoolGraph();
                    RedrawCurve();
                }
            }
        }

        void RedrawCurve()
        {
            Posture[] postures = Posture.CalculatePostures(drawingPoints).ToArray();
            ClothoidCurve c = ClothoidSolutionBertolazzi.G1Spline(postures);
            Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), this.curveLR);
        }

        void OnValidate()
        {
            if (!awake) return;

            Helpers.DrawOrderedVector3s(new List<Vector3>() { start, start + (Helpers.GetTangent(startAngle) * .3f) }, this.startLR);
            Helpers.DrawOrderedVector3s(new List<Vector3>() { end, end + (Helpers.GetTangent(endAngle) * .3f) }, this.endLR);
            G1Fit();
        }

        ClothoidCurve G1(HermiteData p1, HermiteData p2)
        {
            return ClothoidSolutionBertolazzi.G1Curve(p1.x, p1.z, p1.tangentAngle, p2.x, p2.z, p2.tangentAngle, true);
        }
    }
}