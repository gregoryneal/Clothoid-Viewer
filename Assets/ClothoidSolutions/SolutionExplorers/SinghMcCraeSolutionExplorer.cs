using System.Collections.Generic;
using System.Linq;
using Clothoid;
using UnityEngine;

public class SinghMcCraeSolutionExplorer : MonoBehaviour
{
    [Header("To use this component press play and then draw in the game view. \nA clothoid curve approximating your input polyline will be \ngenerated using a method devised by Singh and McCrae. The input \npolyline will be converted into a sequence of 2D points in arc \nlength/curvature space. This graph will have segmented linear \nregression performed on it, and the resulting polyline in arc \nlength/curvature space will be used to generate curve segments. \nThe curve is generated in local space, so it needs to be matched \nwith the input polyline via 2D transformation and rotation, this \nis accomplished via singular value decomposition of some sample \npoints of the polyline and resulting curve.")]
    LineRenderer lr1;
    LineRenderer lr2;
    bool shouldDraw = false;
    List<Vector3> drawingPoints = new List<Vector3>();
    Vector3 lastSample;
    float sampleArcLength = 1f;
    //ClothoidSolutionSinghMcCrae solution;

    void Start()
    {//dsdfsdf
        LineRenderer[] lrs = GetComponentsInChildren<LineRenderer>();
        lr1 = lrs[0];
        lr2 = lrs[1];
    }
    // Update is called once per frame
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

        if (shouldDraw)
        {
            Vector2 mousePixels = Input.mousePosition; //bottom left is 0,0
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePixels.x, mousePixels.y, Camera.main.transform.position.y));
            if (lastSample == null) lastSample = worldPos;
            else if (Vector3.Distance(worldPos, lastSample) >= sampleArcLength)
            {
                drawingPoints.Add(worldPos);
                Helpers.DrawOrderedVector3s(drawingPoints, this.lr1);
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
        ClothoidCurve c = ClothoidSolutionSinghMcCrae.G2(drawingPoints.Select(p => p.ToCSVector3()).ToList());
        Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), this.lr2);
    }
}
