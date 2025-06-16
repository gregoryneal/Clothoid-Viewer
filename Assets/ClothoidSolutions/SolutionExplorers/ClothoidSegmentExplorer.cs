using Clothoid;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ClothoidSegmentExplorer : MonoBehaviour
{
    [Min(0)]
    public float startArcLength = 0;
    public float endArcLength = 1;
    public float startCurvature = 0;
    public float endCurvature = 0.5f;
    [Tooltip("this is the scaling paramater")]
    public float B = 1;

    [Min(2)]
    public int numSamples = 10;
    private ClothoidSegment segment;
    private LineRenderer lr;

    private bool awake = false;
    
    void Start()
    {
        this.awake = true;
        this.lr = GetComponent<LineRenderer>();
        Redraw();
    }

    private void Redraw() {
        this.segment = new ClothoidSegment(this.startArcLength, this.endArcLength, this.startCurvature, this.endCurvature, this.B);
        Helpers.DrawOrderedVector3s(this.segment.GetSamples(this.numSamples), lr);
    }

    void OnValidate()
    {
        if (awake) {
            Redraw();     
        }
    }
}
