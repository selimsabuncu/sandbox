using UnityEngine;
using Texture2D = UnityEngine.Texture2D;

public class Creator : MonoBehaviour
{
    public int size = 256;
    [SerializeField] private int brushRadius = 1;
    
    private int height, width;
    private Renderer _renderer;
    private Texture2D _texture;
    private Color[] _pixels;
    private bool[] _occupied;
    private Color _currentColor = Color.yellow;
    private bool _scanLeftToRight = true;
    
    private int BrushRadius
    {
        get => brushRadius;
        set
        {
            brushRadius = Mathf.Clamp(value, 1, 5);
            Debug.Log($"Brush radius set to: {brushRadius}");
        }
    }
    
    private void Start()
    {
        width = size; height = size;
        _renderer = GetComponent<Renderer>();
        _renderer.material = new Material(_renderer.material);
        _texture = GenerateTexture();
        _renderer.material.mainTexture = _texture;
    }

    private void Update()
    {
        HandleMouseInput();
        SandFall();
    }

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButton(0)) return;

        Vector2? pixelUV = GetMouseLocation();
        if (pixelUV == null) return;

        int x = (int)pixelUV.Value.x;
        int y = (int)pixelUV.Value.y;
        
        DrawCircleOnPixels(ref _pixels, _currentColor, x, y, BrushRadius);
    }

    private void SandFall()
    {
        System.Array.Clear(_occupied, 0, _occupied.Length);
        Color[] newPixels = (Color[])_pixels.Clone();
        //Fuck performance ig? wtf is this shit
        
        for (int y = 0; y < height; y++)
        {
            int startX = _scanLeftToRight ? 0 : width - 1;
            int endX = _scanLeftToRight ? width : 0;
            int stepX = _scanLeftToRight ? 1: -1;
            
            for (int x = startX; x != endX; x += stepX)
            {
                int index = y * width + x;
                if (_pixels[index] != Color.yellow) continue;

                int targetY = y - 1;
                if (targetY < 0) continue;

                //always tries to move the sand even if 5billion previous iterations proves there is nowhere to move
                //I ain't fixing this shit?
                int targetIndex = targetY * width + x;
                if (TryMoveSand(newPixels, x, y, x, targetY, index, targetIndex)) continue;
                if (x > 0 && TryMoveSand(newPixels, x, y, x - 1, targetY, index, targetIndex - 1)) continue;
                if (x < width - 1 && TryMoveSand(newPixels, x, y, x + 1, targetY, index, targetIndex + 1)) continue;
            }
            _scanLeftToRight = !_scanLeftToRight;
        }
        
        if (newPixels.Length != width * height)
        {
            Debug.LogError($"Pixel array size mismatch: Expected {width * height}, but got {newPixels.Length}");
            return;
        }
        _texture.SetPixels(newPixels);
        _texture.Apply();
        _pixels = newPixels;
    }
    
    private void DrawCircleOnPixels(ref Color[] pixels, Color color, int centerX, int centerY, int radius)
    {
        int rSquared = radius * radius;
        int minX = Mathf.Max(0, centerX - radius);
        int maxX = Mathf.Min(width - 1, centerX + radius);
        int minY = Mathf.Max(0, centerY - radius);
        int maxY = Mathf.Min(height - 1, centerY + radius);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int dx = centerX - x;
                int dy = centerY - y;
                if (dx * dx + dy * dy <= rSquared)
                {
                    int index = y * width + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = color;
                    }
                }
            }
        }
    }
    
    private bool TryMoveSand(Color[] newPixels, int oldX, int oldY, int newX, int newY, int oldIndex, int newIndex)
    {
        if (_pixels[newIndex] != Color.white || _occupied[newIndex]) return false;
        newPixels[oldIndex] = Color.white;
        newPixels[newIndex] = Color.yellow;
        _occupied[newIndex] = true;
        return true;
    }

    private Vector2? GetMouseLocation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return null;
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= width;
        pixelUV.y *= height;
        return pixelUV;
    }

    private Texture2D GenerateTexture()
    {
        width = size; height = size;
        Texture2D texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] colors = new Color[width * height];
        System.Array.Fill(colors, Color.white);
        texture.SetPixels(colors);
        texture.Apply();
        
        _pixels = texture.GetPixels();
        _occupied = new bool[width * height];
        
        return texture;
    }
    
    public void OnBrushRadiusChanged(float newRadius)
    {
        BrushRadius = Mathf.RoundToInt(newRadius);
    }
    
    public void ClearTexture()
    {
        Color[] colors = new Color[width * height];
        System.Array.Fill(colors, Color.white);
        _texture.SetPixels(colors);
        _texture.Apply();
        _pixels = _texture.GetPixels();
    }

    public void ChangeColor(int terrainType)
    {
        switch (terrainType)
        {
            case 0:
                _currentColor = Color.yellow; //sand
                Debug.Log("Terrain changed to sand.");
                break;
            case 1:
                _currentColor = Color.white; //empty
                Debug.Log("Terrain changed to empty.");
                break;
            case 2:
                _currentColor = Color.gray; //rock
                Debug.Log("Terrain changed to rock.");
                break;
            default:
                Debug.LogWarning("Terrain type not found.");
                break;
        }
    }

    //fucks up something idk what, yet.
    public void ChangeSize(int newSize)
    {
        size = newSize;
        width = size;
        height = size;
        _texture = GenerateTexture();
        _renderer.material.mainTexture = _texture;
    }
    
    /*unity doesn't allow enums on buttons, wonderful.
    private void ChangeColor(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Sand:
                _currentColor = Color.yellow;
                break;
            case TerrainType.Empty:
                _currentColor = Color.white;
                break;
            case TerrainType.Rock:
                _currentColor = Color.gray;
                break;
        }
    }
    */
}