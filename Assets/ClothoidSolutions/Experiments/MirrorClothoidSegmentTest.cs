using Clothoid;
using UnityEngine;

/// <summary>
/// This test generates two segments, one is using ClothoidSolutionBertolazzi.G1Curve() the other G1CurveM(), this is to test if they truly generate the same curves.
/// </summary>
public class MirrorClothoidSegmentTest : MonoBehaviour
{
    Vector3 start = new Vector3(-1, 0, 0);
    Vector3 end = new Vector3(1, 0, 0);
    bool awake = false;
    LineRenderer lr1;
    LineRenderer lr2;

    [Range(-180f, 180f)]
    public float startAngle = 0;
    [Range(-180f, 180f)]
    public float endAngle = 90;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        awake = true;

        LineRenderer[] lr = GetComponentsInChildren<LineRenderer>();
        lr1 = lr[0];
        lr2 = lr[1];
    }

    void OnValidate()
    {
        if (!awake) return;

        ClothoidCurve c = ClothoidSolutionBertolazzi.G1Curve(start.x, start.z, startAngle, end.x, end.z, endAngle, true);
        ClothoidCurve m = ClothoidSolutionBertolazzi.G1CurveM(start.x, start.z, startAngle, end.x, end.z, endAngle, true);
        Helpers.DrawOrderedVector3s(c.GetSamples(100), lr1, 0, .1f);
        Helpers.DrawOrderedVector3s(m.GetSamples(100), lr2);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
