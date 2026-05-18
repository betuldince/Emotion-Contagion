using System.Numerics;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.Splines;
using Vector3 = UnityEngine.Vector3;

public class ProbabilityUtils : MonoBehaviour
{
    private static Spline spline;

    void Start()
    {
        InitializeSpline();
    }

    // Static method to create a spline with predefined points
    static void InitializeSpline()
    {
        Spline tempSpline = new Spline();

        // Add key points to the spline at t = 0, 0.5, 1 with the corresponding values
        // Values are based on the percentage of people that ran in the run, hide, and mixed conditions
        tempSpline.Add(new BezierKnot(new Vector3(0f, .5538f, 0f)));  // t = 0, value = 55.38 (Hide)
        tempSpline.Add(new BezierKnot(new Vector3(0.5f, .5849f, 0f)));  // t = 0.5, value = 58.49 (Mixed)
        tempSpline.Add(new BezierKnot(new Vector3(1f, .7246f, 0f)));  // t = 1, value = 72.46 (Run)

        tempSpline.Closed = false;

        spline = tempSpline;
    }

    static Spline InitializeCustomSpline(float pos1, float pos2, float pos3)
    {
        Spline tempSpline = new Spline();

        // Add key points to the spline at t = 0, 0.5, 1 with the corresponding values
        tempSpline.Add(new BezierKnot(new Vector3(0f, pos1, 0f)));  // t = 0, value = 55.38
        tempSpline.Add(new BezierKnot(new Vector3(0.5f, pos2, 0f)));  // t = 0.5, value = 58.49
        tempSpline.Add(new BezierKnot(new Vector3(1f, pos3, 0f)));  // t = 1, value = 72.46

        tempSpline.Closed = false;

        return tempSpline;
    }


    // Static method to evaluate the spline at a given t (between 0 and 1)
    public static float EvaluateDefaultSpline(float t)
    {
        if (spline == null)
        {
            InitializeSpline();
        }
        // Evaluate the spline's position at parameter t
        Vector3 splinePosition = SplineUtility.EvaluatePosition(spline, t);
        return splinePosition.y;  // Return the 'y' component since that holds the value
    }

    public static float EvaluateCustomSpline(Spline tempSpline,  float t)
    {
        // Evaluate the spline's position at parameter t
        Vector3 splinePosition = SplineUtility.EvaluatePosition(tempSpline, t);
        return splinePosition.y;  // Return the 'y' component since that holds the value
    }

    public static bool RandomTrue(float probability)
    {
        // Clamp probability between 0 and 1 to ensure valid input
        probability = Mathf.Clamp01(probability);

        // Return true if a random value (between 0 and 1) is less than or equal to the probability
        return Random.value <= probability;
    }
}
