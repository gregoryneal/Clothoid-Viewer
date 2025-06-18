using System.Collections.Generic;
using System;
using ClothoidX;

public static class Helpers
{
    /// <summary>
    /// Draw a sequence of System.Numerics.Vector3 on a UnityEngine.LineRenderer
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="lr"></param>
    /// <param name="zOffset"></param>
    /// <param name="yOffset"></param>
    public static void DrawOrderedVector3s(List<System.Numerics.Vector3> positions, UnityEngine.LineRenderer lr, float zOffset = 0, float yOffset = 0)
    {
        List<UnityEngine.Vector3> p = new();
        for (int i = 0; i < positions.Count; i++)
        {
            UnityEngine.Vector3 v = positions[i].ToUnityVector3();
            p.Add(positions[i].ToUnityVector3());
        }
        DrawOrderedVector3s(p, lr, zOffset, yOffset);
    }

    /// <summary>
    /// Draw a sequence of UnityEngine.Vector3 on a UnityEngine.LineRenderer
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="lr"></param>
    /// <param name="zOffset"></param>
    /// <param name="yOffset"></param>
    public static void DrawOrderedVector3s(List<UnityEngine.Vector3> positions, UnityEngine.LineRenderer lr, float zOffset = 0, float yOffset = 0)
    {
        float sumOfLength = 0;
        List<UnityEngine.Vector3> newPositions = new();
        for (int i = 0; i < positions.Count; i++)
        {
            System.Numerics.Vector3 pi = positions[i].ToCSVector3();
            System.Numerics.Vector3 p = pi + (System.Numerics.Vector3.UnitZ * zOffset) + (System.Numerics.Vector3.UnitY * yOffset);
            if (zOffset != 0 || yOffset != 0) newPositions.Add(p.ToUnityVector3());
            if (i < positions.Count - 1) sumOfLength += System.Numerics.Vector3.Distance(pi, positions[i + 1].ToCSVector3());
        }
        if (zOffset != 0 || yOffset != 0) positions = newPositions;
        lr.positionCount = positions.Count;
        lr.SetPositions(positions.ToArray());
    }

    /// <summary>
    /// Get the tangent vector from an angle in radians.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static System.Numerics.Vector3 GetTangent(double radians)
    {
        //it is negative because a "positive" angle should rotate in a counterclockwise direction
        return ClothoidSegment.RotateAboutAxisRad(new System.Numerics.Vector3(1, 0, 0), System.Numerics.Vector3.UnitY, -radians);
    }

    public static System.Numerics.Vector3 GetTangentDeg(double degrees)
    {
        return GetTangent(degrees * Math.PI / 180);
    }

    public static System.Numerics.Vector3 ToCSVector3(this UnityEngine.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
    }

    public static UnityEngine.Vector3 ToUnityVector3(this System.Numerics.Vector3 vector)
    {
        return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
    }
    
}