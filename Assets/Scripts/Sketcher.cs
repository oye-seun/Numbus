using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sketcher : MonoBehaviour
{
    private Image image;
    private Texture2D drawingTexture;
    [SerializeField] private float _resolution = 512;
    [SerializeField] private int _radius = 20;
    private Vector2 lastMousePosition;
    private bool isDrawing = false;
    private Color[] pixels;
    [SerializeField] private float _timedelay = 0.2f;
    [SerializeField] private GameObject _number;
    private List<Vector2> _points;
    private float _delay = 0f;


    void Start()
    {
        InitializeTexture();
        SetupImage();
    }

    private void InitializeTexture()
    {
        var (bottomLeft, bottomRight, topRight, topLeft) = HelperFunctions.GetScreenCorners();
        Vector2 width = (topRight - bottomLeft) * _resolution;
        drawingTexture = new Texture2D(Mathf.RoundToInt(width.x), Mathf.RoundToInt(width.y));
        drawingTexture.filterMode = FilterMode.Point;
        
        FillWithColor(Color.white);
    }

    public void FillWithColor(Color color){
        // Initialize pixel array
        pixels = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
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
            GameObject number = Instantiate(_number);
            PolygonCollider2D polyCollider = number.GetComponent<PolygonCollider2D>(); 
            // EdgeCollider2D polyCollider = number.GetComponent<EdgeCollider2D>(); 
            _points = Vectorize.ProcessTexture(drawingTexture);
            List<Vector2> simplifiedPoints  = VertexSimplification.DouglasPeuckerSimplification(_points, 0.0001f);
            _points = simplifiedPoints; 

            polyCollider.points = _points.ToArray();
            _points = polyCollider.points.ToList<Vector2>(); 
            FillWithColor(Color.white);
        }

        _delay -= Time.deltaTime;
    }

    void OnDrawGizmos(){
        if(_points == null) return;
        for(int i = 0; i < _points.Count; i++){
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_points[i], Vector3.one * 0.001f);
            // if(i < _points.Count - 1){
            //     Gizmos.DrawLine(_points[i], _points[i+1]);
            // }
            // else{
            //     Gizmos.DrawLine(_points[i], _points[0]);
            // }
        }
    }

    void OnDestroy()
    {
        if (drawingTexture != null)
            Destroy(drawingTexture);
    }

    // public (Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft) GetScreenCorners()
    // {
    //     Camera cam = Camera.main;
        
    //     Vector2 bottomLeft = cam.ScreenToWorldPoint(new Vector2(0, 0));
    //     Vector2 bottomRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, 0));
    //     Vector2 topRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    //     Vector2 topLeft = cam.ScreenToWorldPoint(new Vector2(0, Screen.height));

    //     return (bottomLeft, bottomRight, topRight, topLeft);
    // }
}
