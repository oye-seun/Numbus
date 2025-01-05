using UnityEngine;

public class HelperFunctions
{
    public static (Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft) GetScreenCorners()
    {
        Camera cam = Camera.main;
        
        Vector2 bottomLeft = cam.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 bottomRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, 0));
        Vector2 topRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 topLeft = cam.ScreenToWorldPoint(new Vector2(0, Screen.height));

        return (bottomLeft, bottomRight, topRight, topLeft);
    }

    public static Vector2 RotateVector(Vector2 vector, float angleInDegrees)
    {
        // Convert angle to radians
        float angleRad = angleInDegrees * Mathf.Deg2Rad;
        
        // Create rotation matrix components
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        
        // Rotation matrix multiplication:
        // [cos -sin] [x]
        // [sin  cos] [y]
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

}