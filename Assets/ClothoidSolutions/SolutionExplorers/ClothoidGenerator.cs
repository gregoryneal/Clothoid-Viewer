using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clothoid {

    [RequireComponent(typeof(LineRenderer))]
    public class ClothoidGenerator : MonoBehaviour
    {
        LineRenderer lr;
        bool awake = false;
        bool lerp = false;
        float t = 0;
        [Range(1, 100)]
        public float arcLength = 5;
        [Range(-1, 1)]
        float startCurvature = 0;
        [Range(-.03f, .03f)]
        float sharpness = .01f;
        public List<Segment> segments = new();

        [ContextMenu("Redraw curve")]
        public void BuildCurve()
        {
            ClothoidCurve c = new ClothoidCurve();
            for (int i = 0; i < segments.Count; i++)
            {
                c += new ClothoidSegment(0, segments[i].arcLength, segments[i].startCurvature, segments[i].endCurvature);
            }
            Helpers.DrawOrderedVector3s(c.GetSamples(100 * c.Count), this.lr);
        }

        void Update()
        {
            if (lerp)
            {
                startCurvature = Mathf.Cos(t);
                sharpness = Mathf.Cos(t / 2) * .04f;
                
                ClothoidSegment s = new(startCurvature, sharpness, arcLength); //final curvature is k + xs (initial curvature + (sharpness * arclength))
                ClothoidCurve c = new ClothoidCurve().AddSegments(s);
                Helpers.DrawOrderedVector3s(c.GetSamples(100), this.lr);

                t += Time.deltaTime;
            }
        }

        void Start()
        {
            lr = GetComponent<LineRenderer>();
            awake = true;
        }

        void OnValidate()
        {
            if (awake)
            {
                BuildCurve();
            }
        }

        [ContextMenu("Lerp")]
        public void L()
        {
            lerp = true;
        }

        [ContextMenu("Stop Lerp")]
        public void SL()
        {
            lerp = false;
        }

        [Serializable]
        public class Segment
        {
            public float arcLength = 5;
            public float startCurvature = 0;
            public float endCurvature = 0;
        }
    }
}
