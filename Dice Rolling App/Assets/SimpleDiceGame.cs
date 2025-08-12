using UnityEngine;

public class SimpleDiceGame : MonoBehaviour
{
    private GameObject die1, die2;
    private Rigidbody rb1, rb2;
    
    void Start()
    {
        SetupEverything();
    }
    
    void SetupEverything()
    {
        // Create camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 8, -8);
            cam.transform.rotation = Quaternion.Euler(40, 0, 0);
        }
        
        // Create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, -0.05f, 0); // Position floor so top surface is at Y=0
        floor.transform.localScale = new Vector3(10, 0.1f, 10);
        
        // Create walls - shorter walls, very short front wall
        CreateWall("WallNorth", new Vector3(0, 1.7f, 5), new Vector3(10, 3.3f, 0.1f));
        CreateWall("WallSouth", new Vector3(0, 0.5f, -5), new Vector3(10, 1f, 0.1f)); // Front wall - very short
        CreateWall("WallEast", new Vector3(5, 1.7f, 0), new Vector3(0.1f, 3.3f, 10));
        CreateWall("WallWest", new Vector3(-5, 1.7f, 0), new Vector3(0.1f, 3.3f, 10));
        
        // Create dice
        die1 = CreateDie("Die1", new Vector3(0, 3, 0), Color.blue);
        rb1 = die1.AddComponent<Rigidbody>();
        
        die2 = CreateDie("Die2", new Vector3(1, 3, 0), Color.blue);
        rb2 = die2.AddComponent<Rigidbody>();
        
        Debug.Log("Everything created! Click to roll dice!");
    }
    
    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
    }
    
    GameObject CreateDie(string name, Vector3 position, Color baseColor)
    {
        // Create an empty GameObject and add a MeshRenderer and MeshFilter
        GameObject die = new GameObject(name);
        die.transform.position = position;
        
        // Add required components
        MeshRenderer renderer = die.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = die.AddComponent<MeshFilter>();
        BoxCollider boxCollider = die.AddComponent<BoxCollider>();
        
        // Create custom cube mesh with proper UV mapping
        Mesh cubeMesh = CreateCubeWithUVAtlas();
        meshFilter.mesh = cubeMesh;
        
        // Use BoxCollider for proper physics (MeshCollider requires convex for Rigidbody)
        boxCollider.size = Vector3.one;
        
        // Create material with 3x2 atlas texture
        Material diceMaterial = CreateDiceAtlasMaterial();
        renderer.material = diceMaterial;
        
        return die;
    }
    
    Material CreateDiceAtlasMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        Texture2D atlasTexture = CreateDiceAtlasTexture();
        material.mainTexture = atlasTexture;
        
        // Disable smoothness/metallic to make dice look more traditional
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", 0.1f);
        
        return material;
    }
    
    Texture2D CreateDiceAtlasTexture()
    {
        // Create 3x2 atlas: [1][2][3] over [4][5][6]
        int atlasWidth = 512 * 3;  // 3 faces wide
        int atlasHeight = 512 * 2; // 2 faces tall
        int faceSize = 512;
        
        Texture2D texture = new Texture2D(atlasWidth, atlasHeight);
        Color[] pixels = new Color[atlasWidth * atlasHeight];
        
        // Fill with blue background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.blue;
        }
        
        // Draw each face in the atlas
        // Top row: [1][2][3]
        DrawDiceAtlasFace(pixels, atlasWidth, 0, 1, faceSize, 1); // Face 1 at (0,1)
        DrawDiceAtlasFace(pixels, atlasWidth, 1, 1, faceSize, 2); // Face 2 at (1,1)
        DrawDiceAtlasFace(pixels, atlasWidth, 2, 1, faceSize, 3); // Face 3 at (2,1)
        
        // Bottom row: [4][5][6]
        DrawDiceAtlasFace(pixels, atlasWidth, 0, 0, faceSize, 4); // Face 4 at (0,0)
        DrawDiceAtlasFace(pixels, atlasWidth, 1, 0, faceSize, 5); // Face 5 at (1,0)
        DrawDiceAtlasFace(pixels, atlasWidth, 2, 0, faceSize, 6); // Face 6 at (2,0)
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    void DrawDiceAtlasFace(Color[] pixels, int atlasWidth, int gridX, int gridY, int faceSize, int dotCount)
    {
        int offsetX = gridX * faceSize;
        int offsetY = gridY * faceSize;
        int dotSize = faceSize / 10; // Larger dots for better visibility
        Color dotColor = Color.white;
        
        Vector2[] dotPositions = GetDotPattern(dotCount, faceSize);
        
        foreach (Vector2 dotPos in dotPositions)
        {
            int dotX = offsetX + (int)dotPos.x;
            int dotY = offsetY + (int)dotPos.y;
            DrawDotInAtlas(pixels, atlasWidth, dotX, dotY, dotSize, dotColor);
        }
    }
    
    void DrawDotInAtlas(Color[] pixels, int atlasWidth, int centerX, int centerY, int dotSize, Color dotColor)
    {
        int atlasHeight = pixels.Length / atlasWidth;
        
        for (int x = centerX - dotSize; x < centerX + dotSize; x++)
        {
            for (int y = centerY - dotSize; y < centerY + dotSize; y++)
            {
                if (x >= 0 && x < atlasWidth && y >= 0 && y < atlasHeight)
                {
                    int index = y * atlasWidth + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                        if (distance <= dotSize)
                        {
                            pixels[index] = dotColor;
                        }
                    }
                }
            }
        }
    }
    
    Vector2[] GetDotPattern(int dotCount, int faceSize)
    {
        float q = faceSize * 0.25f;   // Quarter
        float h = faceSize * 0.5f;    // Half
        float tq = faceSize * 0.75f;  // Three quarter
        
        switch (dotCount)
        {
            case 1:
                return new Vector2[] { new Vector2(h, h) };
                
            case 2:
                return new Vector2[] { 
                    new Vector2(q, q), 
                    new Vector2(tq, tq) 
                };
                
            case 3:
                return new Vector2[] { 
                    new Vector2(q, q), 
                    new Vector2(h, h), 
                    new Vector2(tq, tq) 
                };
                
            case 4:
                return new Vector2[] { 
                    new Vector2(q, q), new Vector2(tq, q),
                    new Vector2(q, tq), new Vector2(tq, tq) 
                };
                
            case 5:
                return new Vector2[] { 
                    new Vector2(q, q), new Vector2(tq, q), new Vector2(h, h),
                    new Vector2(q, tq), new Vector2(tq, tq) 
                };
                
            case 6:
                return new Vector2[] { 
                    new Vector2(q, q), new Vector2(q, h), new Vector2(q, tq),
                    new Vector2(tq, q), new Vector2(tq, h), new Vector2(tq, tq) 
                };
                
            default:
                return new Vector2[] { new Vector2(h, h) };
        }
    }
    
    Mesh CreateCubeWithUVAtlas()
    {
        Mesh mesh = new Mesh();
        
        // Create 24 vertices (4 per face) for proper UV mapping
        Vector3[] vertices = new Vector3[24];
        Vector2[] uvs = new Vector2[24];
        int[] triangles = new int[36]; // 6 faces × 2 triangles × 3 vertices
        
        // Face mapping following dice convention (opposite faces add up to 7):
        // Front: 1, Back: 6, Left: 3, Right: 4, Top: 2, Bottom: 5
        
        int vertIndex = 0;
        int triIndex = 0;
        
        // Front face (Face 1) - UV maps to atlas position (0.0, 0.5) to (0.333, 1.0)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                  new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
                  new Vector2(0.0f, 0.5f), new Vector2(0.333f, 1.0f));
                  
        // Back face (Face 6) - UV maps to atlas position (0.666, 0.0) to (1.0, 0.5)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                  new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                  new Vector2(0.666f, 0.0f), new Vector2(1.0f, 0.5f));
                  
        // Left face (Face 3) - UV maps to atlas position (0.666, 0.5) to (1.0, 1.0)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                  new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                  new Vector2(0.666f, 0.5f), new Vector2(1.0f, 1.0f));
                  
        // Right face (Face 4) - UV maps to atlas position (0.0, 0.0) to (0.333, 0.5)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                  new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                  new Vector2(0.0f, 0.0f), new Vector2(0.333f, 0.5f));
                  
        // Top face (Face 2) - UV maps to atlas position (0.333, 0.5) to (0.666, 1.0)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                  new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                  new Vector2(0.333f, 0.5f), new Vector2(0.666f, 1.0f));
                  
        // Bottom face (Face 5) - UV maps to atlas position (0.333, 0.0) to (0.666, 0.5)
        CreateFace(vertices, uvs, triangles, ref vertIndex, ref triIndex,
                  new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                  new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                  new Vector2(0.333f, 0.0f), new Vector2(0.666f, 0.5f));
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    void CreateFace(Vector3[] vertices, Vector2[] uvs, int[] triangles,
                   ref int vertIndex, ref int triIndex,
                   Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
                   Vector2 uvMin, Vector2 uvMax)
    {
        // Set vertices
        vertices[vertIndex] = v0;
        vertices[vertIndex + 1] = v1;
        vertices[vertIndex + 2] = v2;
        vertices[vertIndex + 3] = v3;
        
        // Set UV coordinates for this face's atlas region
        uvs[vertIndex] = new Vector2(uvMin.x, uvMin.y);     // Bottom-left
        uvs[vertIndex + 1] = new Vector2(uvMax.x, uvMin.y); // Bottom-right
        uvs[vertIndex + 2] = new Vector2(uvMax.x, uvMax.y); // Top-right
        uvs[vertIndex + 3] = new Vector2(uvMin.x, uvMax.y); // Top-left
        
        // Set triangles (two triangles per face)
        triangles[triIndex] = vertIndex;
        triangles[triIndex + 1] = vertIndex + 1;
        triangles[triIndex + 2] = vertIndex + 2;
        triangles[triIndex + 3] = vertIndex;
        triangles[triIndex + 4] = vertIndex + 2;
        triangles[triIndex + 5] = vertIndex + 3;
        
        vertIndex += 4;
        triIndex += 6;
    }
    
    Material CreateDiceFaceMaterial(int dotCount)
    {
        Material faceMaterial = new Material(Shader.Find("Standard"));
        Texture2D faceTexture = CreateSingleFaceTexture(dotCount);
        faceMaterial.mainTexture = faceTexture;
        return faceMaterial;
    }
    
    Texture2D CreateSingleFaceTexture(int dotCount)
    {
        int textureSize = 512;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color[] pixels = new Color[textureSize * textureSize];
        
        // Fill with blue background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.blue;
        }
        
        // Draw the correct number of white dots for this face
        DrawDotsForFace(pixels, textureSize, dotCount);
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    void DrawDotsForFace(Color[] pixels, int textureSize, int dotCount)
    {
        int dotSize = textureSize / 10;
        Color dotColor = Color.white;
        
        float q = textureSize * 0.25f;   // Quarter
        float h = textureSize * 0.5f;    // Half
        float tq = textureSize * 0.75f;  // Three quarter
        
        Vector2[] dotPositions = new Vector2[0];
        
        switch (dotCount)
        {
            case 1:
                dotPositions = new Vector2[] { new Vector2(h, h) };
                break;
            case 2:
                dotPositions = new Vector2[] { 
                    new Vector2(q, q), 
                    new Vector2(tq, tq) 
                };
                break;
            case 3:
                dotPositions = new Vector2[] { 
                    new Vector2(q, q), 
                    new Vector2(h, h), 
                    new Vector2(tq, tq) 
                };
                break;
            case 4:
                dotPositions = new Vector2[] { 
                    new Vector2(q, q), new Vector2(tq, q),
                    new Vector2(q, tq), new Vector2(tq, tq) 
                };
                break;
            case 5:
                dotPositions = new Vector2[] { 
                    new Vector2(q, q), new Vector2(tq, q), new Vector2(h, h),
                    new Vector2(q, tq), new Vector2(tq, tq) 
                };
                break;
            case 6:
                dotPositions = new Vector2[] { 
                    new Vector2(q, q), new Vector2(q, h), new Vector2(q, tq),
                    new Vector2(tq, q), new Vector2(tq, h), new Vector2(tq, tq) 
                };
                break;
        }
        
        foreach (Vector2 dotPos in dotPositions)
        {
            DrawDotInTexture(pixels, textureSize, (int)dotPos.x, (int)dotPos.y, dotSize, dotColor);
        }
    }
    
    Texture2D CreateDiceTexture()
    {
        // Simple single-face texture with 5-dot pattern - blue background, white dots
        int textureSize = 512;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color[] pixels = new Color[textureSize * textureSize];
        
        // Fill with blue background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.blue;
        }
        
        // Draw classic 5-dot pattern with white dots
        DrawSimpleDicePattern(pixels, textureSize);
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    void DrawSimpleDicePattern(Color[] pixels, int textureSize)
    {
        int dotSize = textureSize / 10; // Big white dots
        Color dotColor = Color.white;
        
        float quarter = textureSize * 0.25f;
        float half = textureSize * 0.5f;
        float threeQuarter = textureSize * 0.75f;
        
        // Draw 5-dot pattern (corners + center)
        Vector2[] dotPositions = {
            new Vector2(quarter, quarter),           // Top-left
            new Vector2(threeQuarter, quarter),      // Top-right
            new Vector2(half, half),                 // Center
            new Vector2(quarter, threeQuarter),      // Bottom-left
            new Vector2(threeQuarter, threeQuarter)  // Bottom-right
        };
        
        foreach (Vector2 dotPos in dotPositions)
        {
            DrawDotInTexture(pixels, textureSize, (int)dotPos.x, (int)dotPos.y, dotSize, dotColor);
        }
    }
    
    void DrawDiceFace(Color[] pixels, int textureWidth, int gridX, int gridY, int faceSize, int faceNumber)
    {
        int offsetX = gridX * faceSize;
        int offsetY = gridY * faceSize;
        int dotSize = faceSize / 10; // Big white dots
        Color dotColor = Color.white;
        
        // Get dot positions for this face number
        Vector2[] dotPositions = GetDotPositionsForFace(faceNumber, faceSize);
        
        // Draw all dots for this face
        foreach (Vector2 dotPos in dotPositions)
        {
            int dotX = offsetX + (int)dotPos.x;
            int dotY = offsetY + (int)dotPos.y;
            DrawDotInTexture(pixels, textureWidth, dotX, dotY, dotSize, dotColor);
        }
    }
    
    Vector2[] GetDotPositionsForFace(int faceNumber, int faceSize)
    {
        float quarter = faceSize * 0.25f;
        float half = faceSize * 0.5f;
        float threeQuarter = faceSize * 0.75f;
        
        switch (faceNumber)
        {
            case 1: // One dot in center
                return new Vector2[] { new Vector2(half, half) };
                
            case 2: // Two dots diagonally
                return new Vector2[] { 
                    new Vector2(quarter, quarter),
                    new Vector2(threeQuarter, threeQuarter)
                };
                
            case 3: // Three dots diagonally
                return new Vector2[] { 
                    new Vector2(quarter, quarter),
                    new Vector2(half, half),
                    new Vector2(threeQuarter, threeQuarter)
                };
                
            case 4: // Four dots in corners
                return new Vector2[] { 
                    new Vector2(quarter, quarter),
                    new Vector2(threeQuarter, quarter),
                    new Vector2(quarter, threeQuarter),
                    new Vector2(threeQuarter, threeQuarter)
                };
                
            case 5: // Five dots (four corners + center)
                return new Vector2[] { 
                    new Vector2(quarter, quarter),
                    new Vector2(threeQuarter, quarter),
                    new Vector2(half, half),
                    new Vector2(quarter, threeQuarter),
                    new Vector2(threeQuarter, threeQuarter)
                };
                
            case 6: // Six dots (two columns of three)
                return new Vector2[] { 
                    new Vector2(quarter, quarter),
                    new Vector2(quarter, half),
                    new Vector2(quarter, threeQuarter),
                    new Vector2(threeQuarter, quarter),
                    new Vector2(threeQuarter, half),
                    new Vector2(threeQuarter, threeQuarter)
                };
                
            default:
                return new Vector2[] { new Vector2(half, half) };
        }
    }
    
    void DrawDotInTexture(Color[] pixels, int textureWidth, int centerX, int centerY, int dotSize, Color dotColor)
    {
        int textureHeight = pixels.Length / textureWidth;
        
        for (int x = centerX - dotSize; x < centerX + dotSize; x++)
        {
            for (int y = centerY - dotSize; y < centerY + dotSize; y++)
            {
                if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                {
                    int index = y * textureWidth + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        // Create circular dot
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                        if (distance <= dotSize)
                        {
                            pixels[index] = dotColor;
                        }
                    }
                }
            }
        }
    }
    
    void CreateTransparentWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        
        // Make it transparent
        Renderer renderer = wall.GetComponent<Renderer>();
        Material transparentMat = new Material(Shader.Find("Standard"));
        transparentMat.SetFloat("_Mode", 3); // Transparent mode
        transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMat.SetInt("_ZWrite", 0);
        transparentMat.DisableKeyword("_ALPHATEST_ON");
        transparentMat.EnableKeyword("_ALPHABLEND_ON");
        transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMat.renderQueue = 3000;
        
        Color color = transparentMat.color;
        color.a = 0.3f; // 30% opacity
        transparentMat.color = color;
        
        renderer.material = transparentMat;
    }
    
    void Update()
    {
        // Click anywhere to roll dice
        if (Input.GetMouseButtonDown(0))
        {
            RollDice();
        }
    }
    
    void RollDice()
    {
        Debug.Log("Rolling dice!");
        
        // Reset positions
        die1.transform.position = new Vector3(0, 3, 0);
        die2.transform.position = new Vector3(1, 3, 0);
        
        // Reset velocities
        rb1.velocity = Vector3.zero;
        rb1.angularVelocity = Vector3.zero;
        rb2.velocity = Vector3.zero;
        rb2.angularVelocity = Vector3.zero;
        
        // Apply gentler random forces - keep dice in the room, higher jump
        rb1.AddForce(Random.Range(-3f, 3f), Random.Range(4f, 8f), Random.Range(-3f, 3f), ForceMode.Impulse);
        rb1.AddTorque(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
        
        rb2.AddForce(Random.Range(-3f, 3f), Random.Range(4f, 8f), Random.Range(-3f, 3f), ForceMode.Impulse);
        rb2.AddTorque(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
    }
}