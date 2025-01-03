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
}