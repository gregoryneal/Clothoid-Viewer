using Clothoid;
using UnityEngine;

public class ReverseReflectSegment : MonoBehaviour
{
    ClothoidSegment s1;
    ClothoidSegment s2;
    ClothoidCurve c;

    bool awake = false;

    public bool reverse = false;
    public bool reflect = false;
    bool lastReverse = false;
    bool lastReflect = false;
    public int segmentIndex = 0;

    [Range(-Mathf.PI, Mathf.PI)]
    public float startAngle = 0;
    [Range(-Mathf.PI, Mathf.PI)]
    public float endAngle = 90;

    public LineRenderer lr1;
    public LineRenderer lr2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        awake = true;

        c = new ClothoidCurve();
        RandomizeCurve();
        UpdateCurves();
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
        Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), lr1);
    }

    // Update is called once per frame
    void OnValidate()
    {
        if (!awake) return;
        if (!lr1 || !lr2) return;
        UpdateCurves();
    }

    void UpdateCurves()
    {
        /*
        //TODO: Implement Reverse and Reflect methods into ClothoidCurve class, this will require a function
        //to get the tangent angle given some arc length.
        s1 = ClothoidSolutionBertolazzi.G1Curve(-1, 0, startAngle, 1, 0, endAngle, true);
        s2 = new(s1);
        if (reverse) s1.Reverse();
        if (reflect) s1.Reflect();

        ClothoidCurve c1 = new ClothoidCurve() + s1, c2 = new ClothoidCurve() + s2;
        //c1.AngleOffset = reverse ? -endAngle : startAngle;
        c1.AngleOffset = reverse ? -endAngle : startAngle;
        if (reflect)
        {
            c1.AngleOffset *= -1;
            //c1.AngleOffset += endAngle;
        }
        c2.AngleOffset = startAngle;

        Helpers.DrawOrderedVector3s(c1.GetSamples(100), lr1);
        Helpers.DrawOrderedVector3s(c2.GetSamples(100), lr2);*/

        if (lastReverse != reverse)
        {
            c[segmentIndex].Reverse();
            lastReverse = reverse;
        }

        if (lastReflect != reflect)
        {
            c[segmentIndex].Reflect();
            lastReflect = reflect;
        }
        Helpers.DrawOrderedVector3s(c.GetSamples(c.Count * 100), lr1);
    }
}
