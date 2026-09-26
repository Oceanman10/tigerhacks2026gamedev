using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    [Header("Bounds (match your ball's movement margins)")]
    public float maxHorizontalOffset = 3f;
    public float maxVerticalOffset = 2f;

    [Header("Point Spacing")]
    public float pointSpacing = 2f;

    [Header("Difficulty (curve severity)")]
    [Range(0f, 1f)]
    public float curveIntensity = 0.2f; // 0 = straight, 1 = wild curves

    [Header("Level Length")]
    public float maxTime = 60f;
    public float ballVelocity = 5f;

    public LevelData GenerateLevel()
    {
        float totalDistance = maxTime * ballVelocity;
        int numberOfPoints = Mathf.CeilToInt(totalDistance / pointSpacing);

        List<Point> points = new List<Point>();
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float zPos = i * pointSpacing;

            float xOffset = Random.Range(-maxHorizontalOffset, maxHorizontalOffset) * curveIntensity;
            float yOffset = Random.Range(-maxVerticalOffset, maxVerticalOffset) * curveIntensity;

            float newX = Mathf.Clamp(previousPoint.x + xOffset, -maxHorizontalOffset, maxHorizontalOffset);
            float newY = Mathf.Clamp(previousPoint.y + yOffset, -maxVerticalOffset, maxVerticalOffset);

            Vector3 newPoint = new Vector3(newX, newY, zPos);

            Point p = new Point();
            p.point = newPoint;

            // Only set point2 (curve control) above a certain curveIntensity threshold
            if (curveIntensity > 0.3f)
            {
                float midXOffset = Random.Range(-maxHorizontalOffset, maxHorizontalOffset) * curveIntensity;
                float midYOffset = Random.Range(-maxVerticalOffset, maxVerticalOffset) * curveIntensity;
                p.point2 = new Vector3(
                    Mathf.Clamp((previousPoint.x + newPoint.x) / 2f + midXOffset, -maxHorizontalOffset, maxHorizontalOffset),
                    Mathf.Clamp((previousPoint.y + newPoint.y) / 2f + midYOffset, -maxVerticalOffset, maxVerticalOffset),
                    (previousPoint.z + zPos) / 2f
                );
            }

            points.Add(p);
            previousPoint = newPoint;
        }

        LevelData level = new LevelData();
        level.points = points.ToArray();
        return level;
    }
}