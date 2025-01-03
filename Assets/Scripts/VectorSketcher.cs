using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VectorSketcher : MonoBehaviour
{
    [SerializeField] private int _resolution = 10;
    [SerializeField] private int _strokeWidth = 8;
    private Image image;
    private Color[] pixels;
    private StrokeManager strokeManager = new StrokeManager();
    private Texture2D drawingTexture;
    private Vector2 bottomLeftCorner, topRightCorner;

    void Start()
    {
        InitializeTexture();
        SetupImage();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            strokeManager.StartNewStroke();
        }
        
        if (Input.GetMouseButton(0))
        {
            Vector2 point = Input.mousePosition;
            point.x /= Screen.width;
            point.y /= Screen.height;
            strokeManager.AddPoint(point, drawingTexture, _strokeWidth);
        }

        // if(Input.GetMouseButtonUp(0)){
        //     Debug.Log("current stroke count: " + strokeManager.currentStroke.points.Count);
        // }
    }

    void OnDrawGizmos(){
        if(strokeManager.strokes == null) return;
        for(int i = 0; i < strokeManager.strokes.Count; i++){
            if(strokeManager.strokes[i].points == null) continue;
            for(int j = 0; j < strokeManager.strokes[i].points.Count; j++){
                Gizmos.color = Color.red;

                Vector2 point = strokeManager.strokes[i].points[j];

                Vector2 pos = Vector2.zero;
                pos.x = Mathf.Lerp(bottomLeftCorner.x, topRightCorner.x, point.x);
                pos.y = Mathf.Lerp(bottomLeftCorner.y, topRightCorner.y, point.y);
                Gizmos.DrawCube(pos, Vector3.one * 0.01f);
                // if(j < strokeManager.strokes[i].points.Count - 1){
                //     Gizmos.DrawLine(strokeManager.strokes[i].points[j], strokeManager.strokes[i].points[j+1]);
                // }
                // else{
                //     Gizmos.DrawLine(strokeManager.strokes[i].points[j], strokeManager.strokes[i].points[0]);
                // }
            }
            // Gizmos.color = Color.red;
            // Gizmos.DrawCube(_points[i], Vector3.one * 0.001f);
            // if(i < _points.Count - 1){
            //     Gizmos.DrawLine(_points[i], _points[i+1]);
            // }
            // else{
            //     Gizmos.DrawLine(_points[i], _points[0]);
            // }
        }
    }

    private void InitializeTexture()
    {
        var (bottomLeft, bottomRight, topRight, topLeft) = HelperFunctions.GetScreenCorners();
        bottomLeftCorner = bottomLeft;
        topRightCorner = topRight;

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
}



[System.Serializable]
public class Stroke
{
    public List<Vector2> points;

    public Stroke()
    {
        points = new List<Vector2>();
    }

    public void AddPoint(Vector2 point)
    {
        if(points.Count > 0 && points[points.Count - 1] == point) return;
        points.Add(point);
    }

    public void Clear()
    {
        points.Clear();
    }
}

public class StrokeManager
{
    public List<Stroke> strokes;
    public Stroke currentStroke { get; private set; }
    private Color[] pixels;

    public StrokeManager()
    {
        strokes = new List<Stroke>();
    }

    public void StartNewStroke()
    {
        currentStroke = new Stroke();
        strokes.Add(currentStroke);
    }

    public void AddPoint(Vector2 point)
    {
        if (currentStroke != null)
        {
            currentStroke.AddPoint(point);
        }
    }


    public void AddPoint(Vector2 point, Texture2D texture, int radius)
    {
        if (currentStroke == null) return;

        currentStroke.AddPoint(point);

        // Initialize pixels array if needed
        if (pixels == null || pixels.Length != texture.width * texture.height)
        {
            pixels = texture.GetPixels();
        }

        Vector2Int texCoord = TransformToTextureSpace(point, texture.width, texture.height);
        Vector2Int prevTexCoord = (currentStroke.points.Count < 2)? texCoord :
         TransformToTextureSpace(currentStroke.points[currentStroke.points.Count - 2], texture.width, texture.height);

        float distance = Vector2.Distance(texCoord, prevTexCoord);
        if(distance <= 1)
        {
            AddPixel(texCoord.x, texCoord.y, Color.black, texture.width, texture.height, radius);
        }
        else
        {
            int numberOfPoints = Mathf.CeilToInt(distance);
            for (int i = 0; i < numberOfPoints; i++)
            {
                float t = i / (float)numberOfPoints;
                Vector2 interpolatedPosition = Vector2.Lerp(prevTexCoord, texCoord, t);

                AddPixel(Mathf.RoundToInt(interpolatedPosition.x), Mathf.RoundToInt(interpolatedPosition.y), Color.black, texture.width, texture.height, radius);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }
    

    private void AddPixel(int x, int y, Color color, int tWidth, int tHeight, int radius = 1)
    {
        // Paint a square around the point
        for (int py = -radius; py <= radius; py++)
        {
            for (int px = -radius; px <= radius; px++)
            {
                int pixelX = x + px;
                int pixelY = y + py;

                // Check bounds
                if (pixelX >= 0 && pixelX < tWidth && 
                    pixelY >= 0 && pixelY < tHeight)
                {
                    int index = pixelX + (pixelY * tWidth);
                    pixels[index] = Color.black; // Or any color you want
                }
            }
        }
    }

    private Vector2Int TransformToTextureSpace(Vector2 point, int tWidth, int tHeight){
        // Convert screen point to texture coordinates
        Vector2 pos = Vector2.zero;
        pos.x = Mathf.Lerp(0, tWidth, point.x);
        pos.y = Mathf.Lerp(0, tHeight, point.y);

        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public void ClearStrokes()
    {
        strokes.Clear();
        currentStroke = null;
        pixels = null;
    }
}

