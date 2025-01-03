using UnityEngine;
using System.Collections.Generic;

public class Vectorize
{
    // [SerializeField] private float _threshold = 0.1f; // Edge detection sensitivity
    private static readonly int[,] _sobelX = new int[,] 
    { 
        { -1, 0, 1 }, 
        { -2, 0, 2 }, 
        { -1, 0, 1 } 
    };
    
    private static readonly int[,] _sobelY = new int[,] 
    { 
        { -1, -2, -1 }, 
        {  0,  0,  0 }, 
        {  1,  2,  1 } 
    };

    public static List<Vector2> GenerateVerticesFromTexture(Texture2D texture, float threshold)
    {
        List<Vector2> vertices = new List<Vector2>();
        float[,] gradientMagnitudes = CalculateGradientMagnitudes(texture);
        
        // Find edge points by thresholding
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (gradientMagnitudes[x,y] > threshold)
                {
                    // Convert pixel coordinates to normalized coordinates (-0.5 to 0.5 range)
                    float normalizedX = (x / (float)texture.width) - 0.5f;
                    float normalizedY = (y / (float)texture.height) - 0.5f;
                    vertices.Add(new Vector2(normalizedX, normalizedY));
                }
            }
        }
        
        return vertices;
    }

    public static float[,] CalculateGradientMagnitudes(Texture2D texture)
    {
        float[,] magnitudes = new float[texture.width, texture.height];
        Color[] pixels = texture.GetPixels();
        
        for (int y = 1; y < texture.height - 1; y++)
        {
            for (int x = 1; x < texture.width - 1; x++)
            {
                float gx = CalculateGradient(pixels, x, y, _sobelX, texture.width);
                float gy = CalculateGradient(pixels, x, y, _sobelY, texture.width);
                
                // Calculate gradient magnitude
                magnitudes[x,y] = Mathf.Sqrt((gx * gx) + (gy * gy));
            }
        }
        
        return magnitudes;
    }

    private static float CalculateGradient(Color[] pixels, int centerX, int centerY, int[,] kernel, int textureWidth)
    {
        float gradient = 0;
        
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                int pixelIndex = (centerX + x) + ((centerY + y) * textureWidth);
                float grayscale = pixels[pixelIndex].grayscale;
                gradient += grayscale * kernel[y + 1, x + 1];
            }
        }
        
        return gradient;
    }

    // Optional: Helper method to convert color image to grayscale texture
    public static Texture2D ConvertToGrayscale(Texture2D source)
    {
        Texture2D grayscaleTexture = new Texture2D(source.width, source.height);
        Color[] pixels = source.GetPixels();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            float grayscale = pixels[i].grayscale;
            pixels[i] = new Color(grayscale, grayscale, grayscale);
        }
        
        grayscaleTexture.SetPixels(pixels);
        grayscaleTexture.Apply();
        return grayscaleTexture;
    }

    // // Optional: Visualize edges by creating a new texture
    // public Texture2D CreateEdgeTexture(Texture2D sourceTexture)
    // {
    //     float[,] gradientMagnitudes = CalculateGradientMagnitudes(sourceTexture);
    //     Texture2D edgeTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
    //     Color[] edgePixels = new Color[sourceTexture.width * sourceTexture.height];
        
    //     for (int y = 0; y < sourceTexture.height; y++)
    //     {
    //         for (int x = 0; x < sourceTexture.width; x++)
    //         {
    //             float magnitude = gradientMagnitudes[x,y];
    //             float normalizedMagnitude = magnitude > _threshold ? 1 : 0;
    //             edgePixels[x + (y * sourceTexture.width)] = new Color(normalizedMagnitude, normalizedMagnitude, normalizedMagnitude);
    //         }
    //     }
        
    //     edgeTexture.SetPixels(edgePixels);
    //     edgeTexture.Apply();
    //     return edgeTexture;
    // }

    // Example usage
    public static List<Vector2> ProcessTexture(Texture2D inputTexture, float threshold = 0.1f)
    {
        // Convert to grayscale first (optional but recommended)
        Texture2D grayscaleTexture = ConvertToGrayscale(inputTexture);
        
        // Get edge vertices
        List<Vector2> edgeVertices = GenerateVerticesFromTexture(grayscaleTexture, threshold);
        
        // Optional: Create visualization
        // Texture2D edgeTexture = CreateEdgeTexture(grayscaleTexture);
        
        // Clean up
        // Destroy(grayscaleTexture);
        
        // Use the vertices as needed
        Debug.Log($"Found {edgeVertices.Count} edge vertices");
        
        // Example: Create a mesh from vertices
        // CreateMeshFromVertices(edgeVertices);

        return edgeVertices;
    }

    // private void CreateMeshFromVertices(List<Vector2> vertices)
    // {
    //     Mesh mesh = new Mesh();
    //     Vector3[] meshVertices = new Vector3[vertices.Count];
        
    //     // Convert 2D vertices to 3D
    //     for (int i = 0; i < vertices.Count; i++)
    //     {
    //         meshVertices[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
    //     }
        
    //     mesh.vertices = meshVertices;
        
    //     // If you want to render the points, you could use:
    //     int[] indices = new int[vertices.Count];
    //     for (int i = 0; i < vertices.Count; i++)
    //     {
    //         indices[i] = i;
    //     }
    //     mesh.SetIndices(indices, MeshTopology.Points, 0);
        
    //     // Assign to a MeshFilter if needed
    //     if (TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
    //     {
    //         meshFilter.mesh = mesh;
    //     }
    // }
} 
