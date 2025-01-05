using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Mathematics;

public class VectorSketcher : MonoBehaviour
{
    [SerializeField] private int _resolution = 10;
    [SerializeField] private int _strokeWidth = 8;
    [SerializeField] private float _delay = 1f;
    [SerializeField] private GameObject _numberPrefab;
    private Image image;
    private Color[] pixels;
    private StrokeManager strokeManager = new StrokeManager();
    private Texture2D drawingTexture;
    private Vector2 bottomLeftCorner, topRightCorner;
    private float delayCountdown;

    void Start()
    {
        InitializeTexture();
        SetupImage();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            delayCountdown = _delay;
            strokeManager.StartNewStroke();
        }
        
        if (Input.GetMouseButton(0))
        {
            delayCountdown = _delay;
            Vector2 point = Input.mousePosition;
            point.x /= Screen.width;
            point.y /= Screen.height;
            strokeManager.AddPoint(point, drawingTexture, _strokeWidth);
        }

        // if(Input.GetMouseButtonUp(0)){
        //     Debug.Log("current stroke count: " + strokeManager.currentStroke.points.Count);
        // }

        // create object
        if(delayCountdown - Time.deltaTime <= 0 && delayCountdown > 0)
        {
            
            Vector2Int min = Vector2Int.zero;
            Texture2D numTexture = CropTexture(drawingTexture, out min);
            Vector2 pos2d = new Vector2(min.x + (float)numTexture.width/2, min.y + (float)numTexture.height/2);
            pos2d.x /= drawingTexture.width;
            pos2d.y /= drawingTexture.height;
            pos2d = TransformToWorldSpace(pos2d);
            Vector3 pos3d = new Vector3(pos2d.x, pos2d.y, 0);

            // numTexture.
            GameObject numberObj = Instantiate(_numberPrefab, pos3d, Quaternion.identity);
            SpriteRenderer img = numberObj.GetComponent<SpriteRenderer>();
            
            Sprite sprite = Sprite.Create(
                numTexture,
                new Rect(0, 0, /*wScreen.x * */numTexture.width,/*/drawingTexture.width, wScreen.y * */numTexture.height/*/drawingTexture.height*/ ),
                new Vector2(0.5f, 0.5f)
            );
            img.sprite = sprite;
            // img.transform.localScale = new Vector3((float)Screen.width/drawingTexture.width, (float)Screen.height/drawingTexture.height, 1);
            
            Vector2 wScreen = topRightCorner - bottomLeftCorner;
            float worldWidth = wScreen.x * numTexture.width/drawingTexture.width;
            float worldHeight = wScreen.y * numTexture.height/drawingTexture.height;   
            Vector3 scale = img.transform.localScale;
            scale.x = worldWidth/img.sprite.bounds.size.x;
            scale.y = worldHeight/img.sprite.bounds.size.y;
            // img.transform.localScale = scale;
            img.size *= scale;

            // Vector2 size = new Vector2(numTexture.width, numTexture.height);
            // size.x /= drawingTexture.width;
            // size.y /= drawingTexture.height;
            // size = TransformToWorldSpace(size);
            // img.size = size;

            PolygonCollider2D polyCollider = numberObj.GetComponent<PolygonCollider2D>();
            SetColliderPaths(polyCollider);

            strokeManager.ClearStrokes();
            FillWithColor(Color.clear);
        }
        delayCountdown -= Time.deltaTime;        
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
            }
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
        
        FillWithColor(Color.clear);
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



    public static Texture2D CropTexture(Texture2D sourceTexture, out Vector2Int min)
    {
        min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        for(int i = 0; i < sourceTexture.width; i++)
        {
            for(int j = 0; j < sourceTexture.height; j++)
            {
                if(sourceTexture.GetPixel(i, j).a > 0)
                {
                    if(i < min.x) min.x = i;
                    if(j < min.y) min.y = j;
                    if(i > max.x) max.x = i;
                    if(j > max.y) max.y = j;
                }
            }
        }
        int width = max.x - min.x;
        int height = max.y - min.y;

        // int worldWidth = width * Screen.width/sourceTexture.width;
        // int worldHeight = height * Screen.height/sourceTexture.height;

        // Color[] pixels = sourceTexture.GetPixels(min.x, min.y, width, height);
        // Texture2D croppedTex = new Texture2D(width, height);
        // croppedTex.SetPixels(pixels);
        // croppedTex.Apply();

        // RenderTexture rt = RenderTexture.GetTemporary(width, height);
        // rt.filterMode = FilterMode.Bilinear; // or FilterMode.Point for pixel-perfect scaling

        // // Copy source texture to render texture
        // RenderTexture.active = rt;
        // Graphics.Blit(croppedTex, rt);

        Color[] pixels = sourceTexture.GetPixels(min.x, min.y, width, height);
        Texture2D croppedTexture = new Texture2D(width, height);

        // Texture2D croppedTexture = new Texture2D(worldWidth, worldHeight);
        // croppedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        return croppedTexture;
    }



    private void SetColliderPaths(PolygonCollider2D collider)
    {
        if (strokeManager.strokes == null || strokeManager.strokes.Count == 0) return;

        List<Vector2[]> paths = new List<Vector2[]>();
        
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Stroke stroke in strokeManager.strokes)
        {
            if (stroke.points == null || stroke.points.Count < 2) continue;

            List<Vector2> simplifiedPoints = VertexSimplification.DouglasPeuckerSimplification(stroke.points, 0.001f);
            // Vector2[] pathPoints = new Vector2[stroke.points.Count];
            // Vector2[] pathPoints = new Vector2[simplifiedPoints.Count];

            List<Vector2> rPoints = new List<Vector2>();
            List<Vector2> lPoints = new List<Vector2>();
            for (int i = 0; i < simplifiedPoints.Count; i++)
            {
                // Vector2 point = simplifiedPoints[i];
                // find Bounding box
                Vector2 worldPoint = TransformToWorldSpace(simplifiedPoints[i]);
                if(worldPoint.x < min.x) min.x = worldPoint.x;
                if(worldPoint.y < min.y) min.y = worldPoint.y;
                if(worldPoint.x > max.x) max.x = worldPoint.x;
                if(worldPoint.y > max.y) max.y = worldPoint.y;

                Vector2 fwdDir = (i < simplifiedPoints.Count - 1)?  (simplifiedPoints[i + 1] - simplifiedPoints[i]).normalized : Vector2.zero;
                Vector2 bckDir = (i > 0)? (simplifiedPoints[i] - simplifiedPoints[i - 1]).normalized : Vector2.zero;
                Vector2 meanDir = ((fwdDir + bckDir)/2).normalized;

                Vector2 perpDir = HelperFunctions.RotateVector(meanDir, 90);
                rPoints.Add(GetShiftedPoint(simplifiedPoints[i], perpDir));
                lPoints.Insert(0, GetShiftedPoint(simplifiedPoints[i], -perpDir));
                // pathPoints[i] = worldPos;
            }
            // paths.Add(pathPoints);
            rPoints.AddRange(lPoints);
            paths.Add(rPoints.ToArray());
            // paths.Add(lPoints.ToArray());
        }

        // calculate centre and shift paths
        Vector2 centre = (min + max)/2;
        Vector2 dstToCentre = Vector2.zero - centre;
        for (int i = 0; i < paths.Count; i++)
        {
            for (int j = 0; j < paths[i].Length; j++)
            {
                paths[i][j] += dstToCentre;
            }
        }

        collider.pathCount = paths.Count;
        for (int i = 0; i < paths.Count; i++)
        {
            collider.SetPath(i, paths[i]);
        }
    } 

    private Vector2 GetShiftedPoint(Vector2 point, Vector2 dir)
    {
        point.x *= Screen.width;
        point.y *= Screen.height;

        Vector2 shiftedPoint = point + (dir * _strokeWidth);
        shiftedPoint.x /= Screen.width;
        shiftedPoint.y /= Screen.height;

         return TransformToWorldSpace(shiftedPoint);
    }

    private Vector2 TransformToWorldSpace(Vector2 point)
    {
        point.x = Mathf.Lerp(bottomLeftCorner.x, topRightCorner.x, point.x);
        point.y = Mathf.Lerp(bottomLeftCorner.y, topRightCorner.y, point.y);
        return point;
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

