using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Clothoid
{
    public class BertolazziSolutionExplorer : MonoBehaviour
    {
        bool awake = false;

        public Vector3 start = new Vector3(-1, 0, 0);
        public Vector3 end = new Vector3(1, 0, 0);
        bool shouldDraw = false;
        [Tooltip("If true, will use walton meek solution, if false, will use bertolazzi solution")]
        public bool useWM = false;
        public bool useBoth = false;
        public bool canvasDraw = false;
        List<Vector3> drawingPoints = new List<Vector3>();
        Vector3 lastSample;
        [Range(.2f, 2)]
        public float sampleArcLength = 1f;
        [Range(-Mathf.PI, Mathf.PI)]
        public float startAngle = 60;
        [Range(-Mathf.PI, Mathf.PI)]
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
            if (useWM || useBoth)
            {
                ClothoidCurve c = ClothoidSolutionWaltonMeek3.G1(start.x, start.z, startAngle, end.x, end.z, endAngle);
                Helpers.DrawOrderedVector3s(c.GetSamples(100), this.curveLR2);
            }
            if (!useWM || useBoth)
            {
                ClothoidCurve c = ClothoidSolutionBertolazzi.G1(start.x, start.z, startAngle, end.x, end.z, endAngle);                    
                Helpers.DrawOrderedVector3s(c.GetSamples(100), this.curveLR);   
            }
        }

        [ContextMenu("Angle Stress Test")]
        public void A()
        {
            StartCoroutine(G1AngleStressTest2());
        }

        /// <summary>
        /// Run through all the possible values of phi0 and phi1 at a certain increment.
        /// </summary>
        /// <returns></returns>
        public IEnumerator G1AngleStressTest2()
        {
            ClothoidCurve c;
            ClothoidCurve c1;
            float inc = 3 * .017453f; //n * pi / 180

            for (float phi0 = -3.1384510609f; phi0 < 3.1384510609f; phi0 += inc)
            {
                for (float phi1 = Math.Abs(phi0) + inc; phi1 <= 3.1384510609f; phi1 += inc)
                {
                    //Debug.Log($"phi0: {phi0} | phi1: {phi1}");
                    if (useWM || useBoth)
                    {
                        c1 = ClothoidSolutionWaltonMeek3.G1(start.x, start.z, phi0, end.x, end.z, phi1);
                        Helpers.DrawOrderedVector3s(c1.GetSamples(100), this.curveLR2);
                    }
                    if (!useWM || useBoth)
                    {
                        c = ClothoidSolutionBertolazzi.G1(start.x, start.z, phi0, end.x, end.z, phi1);                    
                        Helpers.DrawOrderedVector3s(c.GetSamples(100), this.curveLR);   
                    }
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
                    Helpers.DrawOrderedVector3s(drawingPoints, this.curveLR2);
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
            List<System.Numerics.Vector3> v = drawingPoints.Select(a => a.ToCSVector3()).ToList();
            Posture[] postures = Posture.CalculatePostures(v).ToArray();
            Debug.Log(postures.Length);

            if (useWM || useBoth)
            {
                ClothoidCurve c1 = ClothoidSolutionWaltonMeek3.G1Spline(postures);
                Helpers.DrawOrderedVector3s(c1.GetSamples(c1.Count * 100), this.curveLR2);
            }
            if (!useWM || useBoth)
            {
                ClothoidCurve c = ClothoidSolutionBertolazzi.G1Spline(postures);
                Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), this.curveLR);
            }
        }

        void OnValidate()
        {
            if (!awake) return;

            Helpers.DrawOrderedVector3s(new List<Vector3>() { start, start + (.3f*Helpers.GetTangent(startAngle)).ToUnityVector3() }, this.startLR);
            Helpers.DrawOrderedVector3s(new List<Vector3>() { end, end + (.3f*Helpers.GetTangent(endAngle)).ToUnityVector3() }, this.endLR);
            G1Fit();
        }
    }
}