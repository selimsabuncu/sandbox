using UnityEngine;
using Texture2D = UnityEngine.Texture2D;

public class Creator : MonoBehaviour
{
    public int size = 256;
    private int height, width;
    private Renderer _renderer;
    
    private void Start()
    {
        height = size; width = size;
        
        _renderer = GetComponent<Renderer>();
        _renderer.material.mainTexture = GenerateTexture();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2? pixelUV = GetMouseLocation();
            if (pixelUV != null)
            {
                ColorTexture(_renderer.material.mainTexture as Texture2D, pixelUV.Value, Color.yellow);
            }
        }
        
        SandFall();
    }

    private void SandFall()
    {
        Texture2D texture = _renderer.material.mainTexture as Texture2D;
        
        for (int y = 1; y < height; y++) 
        {
            for (int x = 0; x < width; x++)
            {
                Color color = texture.GetPixel(x, y);
                if (color == Color.yellow) 
                {
                    if (texture.GetPixel(x, y - 1) == Color.white)
                    {
                        ColorTexture(texture, new Vector2(x, y - 1), Color.yellow);
                        ColorTexture(texture, new Vector2(x, y), Color.white);
                    }
                    else if (x > 0 && texture.GetPixel(x - 1, y - 1) == Color.white)
                    {
                        ColorTexture(texture, new Vector2(x - 1, y - 1), Color.yellow);
                        ColorTexture(texture, new Vector2(x, y), Color.white);
                    }
                    else if (x < width - 1 && texture.GetPixel(x + 1, y - 1) == Color.white)
                    {
                        ColorTexture(texture, new Vector2(x + 1, y - 1), Color.yellow);
                        ColorTexture(texture, new Vector2(x, y), Color.white);
                    }
                }
            }
        }
    }
    
    private Vector2? GetMouseLocation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= width;
            pixelUV.y *= height;
            return pixelUV;
        }

        return null;
    }
    
    private void ColorTexture(Texture2D texture2D, Vector2 pixelUV, Color color)
    {
        texture2D.SetPixel((int) pixelUV.x, (int) pixelUV.y, color);
        texture2D.Apply();
    }
    
    private Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return texture;
    }
}
