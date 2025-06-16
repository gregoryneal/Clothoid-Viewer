using System.Collections;
using System.Collections.Generic;
using Clothoid;
using UnityEngine;

public class EvalXYaLargeComparison : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
              
    }

    [ContextMenu("Test")]
    public void Test()
    {
        StartCoroutine(DoTest());
    }

    IEnumerator DoTest()
    {
        double a = 0;
        double b = 10;
        double increment = 0.1f;

        List<double[]> XY;

        while (b > 0 && a < 10)
        {
            Debug.Log($"a: {a} | b: {b}");
            Debug.Log("1 (Code)");
            Print(ClothoidSolutionBertolazzi.EvalXYaLarge(3, a, b));
            Debug.Log("2 (Math)");
            Print(ClothoidSolutionBertolazzi.EvalXYaLarge2(3, a, b));

            a += increment;
            b -= increment;

            yield return new WaitForSeconds(.04f);
            Debug.Log("==");
        }  

        yield return null;
    }

    void Print(List<double[]> XY)
    {
        Debug.Log("-------------");
        for (int i = 0; i < XY[0].Length; i++)
        {
            Debug.Log($"X{i}: {XY[0][i]} | Y{i}: {XY[1][i]}");
        }
        Debug.Log("-------------");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
