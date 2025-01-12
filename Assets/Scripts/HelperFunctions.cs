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

    public static Texture2D FloatArrayToTexture(float[] data, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                // Clamp value between 0 and 1, then convert to byte (0-255)
                // byte value = (byte)(Mathf.Clamp01(data[index]) * 255);
                // Debug.Log("Data Val: " + data[index] + "   Value: " + value);
                // Using the same value for R,G,B to create grayscale
                pixels[/*(height - 1 - y)*/ y * width + x] = new Color(data[index], data[index], data[index], 1);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public static (Vector2Int min, Vector2Int max) GetTextureBounds(Texture2D sourceTexture)
    {
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        for(int i = 0; i < sourceTexture.width; i++)
        {
            for(int j = 0; j < sourceTexture.height; j++)
            {
                Color col = sourceTexture.GetPixel(i, j);
                if(col.a * (col.r + col.g + col.b)/3 > 0)
                {
                    if(i < min.x) min.x = i;
                    if(j < min.y) min.y = j;
                    if(i > max.x) max.x = i;
                    if(j > max.y) max.y = j;
                }
            }
        }
        return (min, max);
    }

    public static void logData(float[] arr){
        for(int i = 0; i < arr.Length; i++){
            Debug.Log(arr[i]);
        }
    }

    public static void log2dData(float[,] arr){
        for(int i = 0; i < arr.GetLength(0); i++){
            for(int j = 0; j < arr.GetLength(1); j++){
                Debug.Log(arr[i,j]);
            }
        }
    }
}