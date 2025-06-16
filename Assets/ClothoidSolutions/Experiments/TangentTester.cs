using UnityEngine;
using Clothoid;
using System;

/// <summary>
/// This testing script will draw a clothoid curve and display the tangent along the arc length.
/// </summary>
public class TangentTester : MonoBehaviour
{
    bool awake = false;
    ClothoidCurve c;

    [Range(0f, 1f)]
    public float arcLength = 0;

    [Range(0, Mathf.PI)]
    public float angleOffset = 0;
    public Vector3 offset = Vector3.zero;

    public LineRenderer lr1;
    public LineRenderer lr2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        awake = true;

        c = new ClothoidCurve();// + ClothoidSolutionBertolazzi.G1Curve(-1, 0, startAngle, 1, 0, endAngle, true);
        RandomizeCurve();
        DrawCurves();
    }

    [ContextMenu("New curve")]
    public void RandomizeCurve()
    {
        c.Reset();
        float dk = UnityEngine.Random.Range(-.01f, .01f);
        float l = UnityEngine.Random.Range(1f, 4f);
        for (int i = 0; i < 10; i++)
        {
            c += new ClothoidSegment(c.EndCurvature, dk, l);
            dk = UnityEngine.Random.Range(-.03f, .03f);
            l = UnityEngine.Random.Range(1f, 4f);
        }
        c.AngleOffset = angleOffset;
        c.Offset = offset.ToCSVector3();
        Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), lr1);
    }

    // Update is called once per frame
    void OnValidate()
    {
        if (!awake) return;
        if (!lr1 || !lr2) return;

        c.AngleOffset = angleOffset;
        c.Offset = offset.ToCSVector3();
        Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), lr1);
        DrawCurves();
    }

    void DrawCurves()
    {

        double al = c.TotalArcLength * arcLength;
        ///Radians and degrees are mislabeled everywhere, stick to one or the other lets get real here. 
        double ang = c.Tangent(al);
        Debug.Log($"angle: {ang * 180 / Math.PI}");
        Vector3 tangent = Helpers.GetTangent(ang).ToUnityVector3();
        Vector3 pos = c.SampleCurveFromArcLength(al).ToUnityVector3();
        Helpers.DrawOrderedVector3s(new System.Collections.Generic.List<Vector3>() { pos, pos + tangent }, this.lr2);
    }
}
