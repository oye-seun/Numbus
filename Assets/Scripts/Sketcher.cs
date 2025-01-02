using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sketcher : MonoBehaviour
{
//     private SpriteRenderer spriteRenderer;
//     private Texture2D drawingTexture;

//     void Start()
//     {
//         // Create texture
//         drawingTexture = new Texture2D(512, 512);
//         Color[] pixels = new Color[512 * 512];
//         for (int i = 0; i < pixels.Length; i++)
//         {
//             pixels[i] = Color.white;
//         }
//         drawingTexture.SetPixels(pixels);
//         drawingTexture.Apply();

//         // Create sprite from texture
//         Sprite sprite = Sprite.Create(drawingTexture, 
//             new Rect(0, 0, drawingTexture.width, drawingTexture.height),
//             new Vector2(0.5f, 0.5f)); // Pivot point at center

//         // Get or add SpriteRenderer component
//         spriteRenderer = GetComponent<SpriteRenderer>();
//         if (spriteRenderer == null)
//             spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

//         // Assign sprite
//         spriteRenderer.sprite = sprite;

//         // PaintSquare(drawingTexture, Color.black, 256, 256, 15);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (Input.GetMouseButton(0)) // Left mouse button
//         {
//             // Debug.Log("leftClick");
//             PaintSquareAtMouse(drawingTexture, Color.red, 20);
//         }
//     }

//     // Creates a texture with a solid color
//     public Texture2D CreateSolidColorTexture(int width, int height, Color color)
//     {
//         Texture2D texture = new Texture2D(width, height);
//         Color[] pixels = new Color[width * height];
        
//         // Fill the entire array with the same color
//         for (int i = 0; i < pixels.Length; i++)
//         {
//             pixels[i] = color;
//         }
        
//         // Alternative using LINQ (requires 'using System.Linq;'):
//         // Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
        
//         texture.SetPixels(pixels);
//         texture.Apply();
//         return texture;
//     }


//     // public void PaintSquareAtMouse(Texture2D texture, Color squareColor, int squareSize)
//     // {
//     //     // Get mouse position in screen coordinates
//     //     Vector3 mousePos = Input.mousePosition;
        
//     //     // Convert to texture coordinates (assuming texture is on a UI RawImage)
//     //     RectTransform rectTransform = GetComponent<RectTransform>();
//     //     if (rectTransform != null)
//     //     {
//     //         RectTransformUtility.ScreenPointToLocalPointInRectangle(
//     //             rectTransform,
//     //             mousePos,
//     //             null,
//     //             out Vector2 localPoint
//     //         );

//     //         // Convert local point to texture coordinates
//     //         float normalizedX = (localPoint.x / rectTransform.rect.width) + 0.5f;
//     //         float normalizedY = (localPoint.y / rectTransform.rect.height) + 0.5f;

//     //         int textureX = Mathf.RoundToInt(normalizedX * texture.width);
//     //         int textureY = Mathf.RoundToInt(normalizedY * texture.height);

//     //         // Center the square on the mouse position
//     //         textureX -= squareSize / 2;
//     //         textureY -= squareSize / 2;

//     //         PaintSquare(texture, squareColor, textureX, textureY, squareSize);
//     //     }
//     // }

//     public void PaintSquareAtMouse(Texture2D texture, Color squareColor, int squareSize)
// {
//     // Get mouse position in world space
//     Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
//     // Convert world position to local position relative to the sprite
//     Vector2 localPos = transform.InverseTransformPoint(mouseWorldPos);

//     Vector2 min = -spriteRenderer.bounds.size/2;

    
//     // Debug.Log("min: " + min);
//     // Debug.Log("localpos: " + localPos);
//     // Convert local position to texture coordinates
//     // Assuming sprite is centered and covers the entire texture
//     // float normalizedX = (localPos.x + 0.5f); // Convert from -0.5/0.5 to 0/1 range
//     // float normalizedY = (localPos.y + 0.5f);

//     float normalizedX = Mathf.InverseLerp(min.x, -min.x, localPos.x) * texture.width; // Convert from -0.5/0.5 to 0/1 range
//     float normalizedY = Mathf.InverseLerp(min.y, -min.y, localPos.y) * texture.height;
    
//     // Convert to texture pixels
//     // int textureX = Mathf.RoundToInt(normalizedX * texture.width);
//     // int textureY = Mathf.RoundToInt(normalizedY * texture.height);

//     int textureX = Mathf.RoundToInt(normalizedX);
//     int textureY = Mathf.RoundToInt(normalizedY);
    
//     // Center the square on the mouse position
//     textureX -= squareSize / 2;
//     textureY -= squareSize / 2;
    
//     // Paint the square
//     PaintSquare(texture, squareColor, textureX, textureY, squareSize);
// }

//      // Paint a filled square on an existing texture
//     public void PaintSquare(Texture2D texture, Color squareColor, 
//                            int squareX, int squareY, int squareSize)
//     {
//         // Make sure the texture is readable
//         if (!texture.isReadable)
//         {
//             Debug.LogError("Texture is not readable. Enable 'Read/Write' in texture import settings.");
//             return;
//         }

//         // Get current pixels
//         Color[] pixels = texture.GetPixels();
//         int textureWidth = texture.width;
//         int textureHeight = texture.height;

//         // Draw the square
//         for (int y = 0; y < squareSize; y++)
//         {
//             for (int x = 0; x < squareSize; x++)
//             {
//                 int pixelX = squareX + x;
//                 int pixelY = squareY + y;
                
//                 // Check if the pixel is within texture bounds
//                 if (pixelX >= 0 && pixelX < textureWidth && 
//                     pixelY >= 0 && pixelY < textureHeight)
//                 {
//                     int index = pixelX + (pixelY * textureWidth);
//                     pixels[index] = squareColor;
//                 }
//             }
//         }
        
//         texture.SetPixels(pixels);
//         texture.Apply();
//     }



    private Image image;
    private Texture2D drawingTexture;
    [SerializeField] private float _resolution = 512;
    [SerializeField] private int _radius = 20;
    private Vector2 lastMousePosition;
    private bool isDrawing = false;
    private Color[] pixels;
    [SerializeField] private float _timedelay = 0.2f;
    private float _delay = 0f;


    void Start()
    {
        InitializeTexture();
        SetupImage();
    }

    private void InitializeTexture()
    {
        var (bottomLeft, bottomRight, topRight, topLeft) = GetScreenCorners();
        Vector2 width = (topRight - bottomLeft) * _resolution;
        drawingTexture = new Texture2D(Mathf.RoundToInt(width.x), Mathf.RoundToInt(width.y));
        drawingTexture.filterMode = FilterMode.Point;
        
        // Initialize pixel array
        pixels = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();
    }

    private void SetupImage()
    {
        image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        Sprite sprite = Sprite.Create(
            drawingTexture,
            new Rect(0, 0, drawingTexture.width, drawingTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        image.sprite = sprite;
    }

    private void DrawLine(Vector2 start, Vector2 end, int radius)
    {
        float distance = Vector2.Distance(start, end);
        
        if (distance < 1)
        {
            PaintSquareAtMouse(drawingTexture, Color.red, radius);
            return;
        }

        int numberOfPoints = Mathf.CeilToInt(distance);
        bool needsApply = false;
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)numberOfPoints;
            Vector2 interpolatedPosition = Vector2.Lerp(start, end, t);
            
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                image.rectTransform,
                interpolatedPosition,
                null,
                out mousePos
            );

            Vector2 rectSize = image.rectTransform.rect.size;
            float normalizedX = (mousePos.x + rectSize.x/2) / rectSize.x;
            float normalizedY = (mousePos.y + rectSize.y/2) / rectSize.y;

            int textureX = Mathf.RoundToInt(normalizedX * drawingTexture.width);
            int textureY = Mathf.RoundToInt(normalizedY * drawingTexture.height);

            textureX -= radius / 2;
            textureY -= radius / 2;

            if (PaintSquare(textureX, textureY, radius))
            {
                needsApply = true;
            }
        }

        if (needsApply)
        {
            drawingTexture.SetPixels(pixels);
            drawingTexture.Apply();
        }
    }

    private bool PaintSquare(int squareX, int squareY, int squareSize)
    {
        bool pixelChanged = false;
        int textureWidth = drawingTexture.width;
        
        for (int y = 0; y < squareSize; y++)
        {
            for (int x = 0; x < squareSize; x++)
            {
                int pixelX = squareX + x;
                int pixelY = squareY + y;
                
                if (pixelX >= 0 && pixelX < drawingTexture.width && 
                    pixelY >= 0 && pixelY < drawingTexture.height)
                {
                    int index = pixelX + (pixelY * textureWidth);
                    if (pixels[index] != Color.red)
                    {
                        pixels[index] = Color.red;
                        pixelChanged = true;
                    }
                }
            }
        }
        return pixelChanged;
    }

    public void PaintSquareAtMouse(Texture2D texture, Color squareColor, int squareSize)
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            image.rectTransform,
            Input.mousePosition,
            null,
            out mousePos
        );

        Vector2 rectSize = image.rectTransform.rect.size;
        float normalizedX = (mousePos.x + rectSize.x/2) / rectSize.x;
        float normalizedY = (mousePos.y + rectSize.y/2) / rectSize.y;

        int textureX = Mathf.RoundToInt(normalizedX * texture.width);
        int textureY = Mathf.RoundToInt(normalizedY * texture.height);

        textureX -= squareSize / 2;
        textureY -= squareSize / 2;

        if (PaintSquare(textureX, textureY, squareSize))
        {
            texture.SetPixels(pixels);
            texture.Apply();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            _delay = _timedelay;
            lastMousePosition = Input.mousePosition;
            PaintSquareAtMouse(drawingTexture, Color.red, _radius);
        }
        else if (Input.GetMouseButton(0))
        {
            _delay = _timedelay;
            Vector2 currentMousePosition = Input.mousePosition;
            DrawLine(lastMousePosition, currentMousePosition, _radius);
            lastMousePosition = currentMousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if(_delay - Time.deltaTime <= 0 && _delay > 0)
        {
            Instantiate()
        }
        else{
            _delay -= Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        if (drawingTexture != null)
            Destroy(drawingTexture);
    }

    public (Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft) GetScreenCorners()
    {
        Camera cam = Camera.main;
        
        Vector2 bottomLeft = cam.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 bottomRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, 0));
        Vector2 topRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 topLeft = cam.ScreenToWorldPoint(new Vector2(0, Screen.height));

        return (bottomLeft, bottomRight, topRight, topLeft);
    }
}
