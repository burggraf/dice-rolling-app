using UnityEngine;
using UnityEngine.UI;

public class SimpleDiceGame : MonoBehaviour
{
    private GameObject die1, die2;
    private Rigidbody rb1, rb2;
    private Text scoreText;
    
    // Roll validation
    private bool isRolling = false;
    private float rollStartTime;
    private float rollTimeoutDuration = 8f;
    private float validationCheckInterval = 1f;
    private float validationTimer;
    
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
        
        // Create Score UI
        CreateScoreUI();
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
        
        // Handle roll validation during rolling
        if (isRolling)
        {
            HandleRollValidation();
        }
    }
    
    void RollDice()
    {
        Debug.Log("Rolling dice!");
        
        // Set rolling state
        isRolling = true;
        rollStartTime = Time.time;
        validationTimer = 0f;
        
        // Update score text to show rolling
        if (scoreText != null)
        {
            scoreText.text = "Rolling...";
            scoreText.color = Color.yellow;
        }
        
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
        
        // Start checking for dice to settle
        InvokeRepeating(nameof(CheckDiceSettled), 1f, 0.5f);
    }
    
    void CreateScoreUI()
    {
        Debug.Log("Creating score UI...");
        
        // Create Canvas
        GameObject canvasGO = new GameObject("Score Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // Create Score Text
        GameObject scoreGO = new GameObject("Score Text");
        scoreGO.transform.SetParent(canvas.transform, false);
        
        scoreText = scoreGO.AddComponent<Text>();
        scoreText.text = "Score: Click to Roll";
        scoreText.fontSize = 32;
        scoreText.color = Color.white;
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Position at top right
        RectTransform rt = scoreText.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.sizeDelta = new Vector2(300f, 50f);
        
        Debug.Log("Score UI created!");
    }
    
    void CheckDiceSettled()
    {
        // Check if both dice have stopped moving
        if (AreDiceSettled())
        {
            CancelInvoke(nameof(CheckDiceSettled)); // Stop checking
            isRolling = false; // Stop rolling validation
            UpdateScore();
        }
    }
    
    bool AreDiceSettled()
    {
        float velocityThreshold = 0.1f;
        float angularVelocityThreshold = 0.1f;
        
        bool die1Settled = rb1.velocity.magnitude < velocityThreshold && 
                          rb1.angularVelocity.magnitude < angularVelocityThreshold;
                          
        bool die2Settled = rb2.velocity.magnitude < velocityThreshold && 
                          rb2.angularVelocity.magnitude < angularVelocityThreshold;
        
        return die1Settled && die2Settled;
    }
    
    void UpdateScore()
    {
        if (scoreText == null) return;
        
        // Check if roll is valid before calculating scores
        bool die1Valid = IsValidDicePosition(die1);
        bool die2Valid = IsValidDicePosition(die2);
        bool rollValid = die1Valid && die2Valid;
        
        if (rollValid)
        {
            // Calculate dice values based on which face is pointing UP
            int die1Value = GetDiceValueFromTopFace(die1.transform);
            int die2Value = GetDiceValueFromTopFace(die2.transform);
            int total = die1Value + die2Value;
            
            scoreText.text = "Score: " + total;
            scoreText.color = Color.white;
            Debug.Log($"Valid roll - Die 1: {die1Value}, Die 2: {die2Value}, Total: {total}");
        }
        else
        {
            scoreText.text = "Roll Again";
            scoreText.color = Color.red;
            Debug.Log($"Invalid roll - Die1 valid: {die1Valid}, Die2 valid: {die2Valid}");
        }
    }
    
    bool IsValidDicePosition(GameObject dice)
    {
        // Check all validation conditions for settled dice
        bool notAgainstWall = !IsDiceAgainstWall(dice);
        bool notOutOfBounds = !IsDiceOutOfBounds(dice);
        bool lyingFlat = IsDiceLyingFlat(dice);
        
        // Full validation with enhanced flatness detection
        bool isValid = notAgainstWall && notOutOfBounds && lyingFlat;
        
        // Detailed logging to see what's failing
        Debug.Log($"Dice {dice.name} position: {dice.transform.position}");
        Debug.Log($"Dice {dice.name} validation details:");
        Debug.Log($"  - NotAgainstWall: {notAgainstWall}");
        Debug.Log($"  - NotOutOfBounds: {notOutOfBounds}");  
        Debug.Log($"  - LyingFlat: {lyingFlat}");
        Debug.Log($"  - IsValid (all checks): {isValid}");
        
        return isValid;
    }
    
    int GetDiceValueFromTopFace(Transform diceTransform)
    {
        // Find which face is pointing most upward (toward world Y+)
        Vector3 worldUp = Vector3.up;
        
        // The six face directions in local space of the dice
        Vector3[] localFaceDirections = {
            Vector3.up,      // Local Y+ (originally top face)
            Vector3.down,    // Local Y- (originally bottom face)
            Vector3.forward, // Local Z+ (originally front face)
            Vector3.back,    // Local Z- (originally back face)
            Vector3.right,   // Local X+ (originally right face)
            Vector3.left     // Local X- (originally left face)
        };
        
        // Based on your dice atlas mapping from CreateCubeWithUVAtlas:
        // Front: 1, Back: 6, Left: 3, Right: 4, Top: 2, Bottom: 5
        int[] faceValues = { 2, 5, 1, 6, 4, 3 };
        
        float maxDot = -1f;
        int bestFaceIndex = 0;
        
        // Transform each local face direction to world space and check alignment with world up
        for (int i = 0; i < localFaceDirections.Length; i++)
        {
            Vector3 worldFaceDirection = diceTransform.TransformDirection(localFaceDirections[i]);
            float dot = Vector3.Dot(worldFaceDirection, worldUp);
            
            if (dot > maxDot)
            {
                maxDot = dot;
                bestFaceIndex = i;
            }
        }
        
        int result = faceValues[bestFaceIndex];
        Debug.Log($"Dice face pointing up: {bestFaceIndex}, Value: {result}, Dot: {maxDot:F2}");
        return result;
    }
    
    bool IsDiceAgainstWall(GameObject dice)
    {
        Vector3 pos = dice.transform.position;
        float threshold = 0.5f; // Less sensitive - only when dice center is very close to wall
        
        // Walls are at x=±5 and z=±5, so check distance from wall surfaces
        bool againstXWall = Mathf.Abs(pos.x) > 5f - threshold;
        bool againstZWall = Mathf.Abs(pos.z) > 5f - threshold;
        bool againstWall = againstXWall || againstZWall;
        
        // Always log for debugging
        Debug.Log($"Dice {dice.name} wall check: Pos={pos}, DistFromXWall={5f - Mathf.Abs(pos.x):F2}, DistFromZWall={5f - Mathf.Abs(pos.z):F2}, Threshold={threshold}, AgainstWall={againstWall}");
        
        return againstWall;
    }
    
    bool IsDiceOutOfBounds(GameObject dice)
    {
        Vector3 pos = dice.transform.position;
        
        // Check each condition separately for detailed logging
        bool belowFloor = pos.y < -2f;
        bool outsideX = Mathf.Abs(pos.x) > 6f;
        bool outsideZ = Mathf.Abs(pos.z) > 6f;
        
        bool outOfBounds = belowFloor || outsideX || outsideZ;
        
        // Log details when checking bounds
        Debug.Log($"Dice {dice.name} bounds check: Pos={pos}, BelowFloor={belowFloor}, OutsideX={outsideX}, OutsideZ={outsideZ}, OutOfBounds={outOfBounds}");
        
        return outOfBounds;
    }
    
    void HandleRollValidation()
    {
        // Check for timeout
        float rollDuration = Time.time - rollStartTime;
        if (rollDuration > rollTimeoutDuration)
        {
            Debug.Log("Roll timed out - forcing invalid result");
            ForceInvalidRoll("Roll timed out");
            return;
        }
        
        // Periodic validation check during rolling
        validationTimer += Time.deltaTime;
        if (validationTimer >= validationCheckInterval)
        {
            validationTimer = 0f;
            Debug.Log($"Checking dice validity during roll. Roll duration: {rollDuration:F1}s");
            CheckDiceValidityDuringRoll();
        }
    }
    
    void CheckDiceValidityDuringRoll()
    {
        // Only check for dice that are way out of bounds (fell off table completely)
        // Don't check wall contact or flatness until dice are fully settled
        
        // Check die1
        Vector3 die1Pos = die1.transform.position;
        float die1Velocity = rb1.velocity.magnitude;
        
        Debug.Log($"Die1 - Position: {die1Pos}, Velocity: {die1Velocity:F2}");
        
        if (IsDiceWayOutOfBounds(die1))
        {
            Debug.Log($"Die1 fell out of play at position {die1Pos} - forcing invalid result");
            ForceInvalidRoll("Die1 fell out of play");
            return;
        }
        
        // Check die2
        Vector3 die2Pos = die2.transform.position;
        float die2Velocity = rb2.velocity.magnitude;
        
        Debug.Log($"Die2 - Position: {die2Pos}, Velocity: {die2Velocity:F2}");
        
        if (IsDiceWayOutOfBounds(die2))
        {
            Debug.Log($"Die2 fell out of play at position {die2Pos} - forcing invalid result");
            ForceInvalidRoll("Die2 fell out of play");
            return;
        }
        
        // Wall contact and flatness will be checked only after dice settle in UpdateScore()
    }
    
    bool IsDiceWayOutOfBounds(GameObject dice)
    {
        Vector3 pos = dice.transform.position;
        
        // Use more permissive bounds for "way out of bounds" check
        // Current bounds in IsDiceOutOfBounds: y < -2, |x| > 6, |z| > 6
        // For "way out" we'll use: y < -3, |x| > 8, |z| > 8
        bool wayOut = (pos.y < -3f || Mathf.Abs(pos.x) > 8f || Mathf.Abs(pos.z) > 8f);
        
        Debug.Log($"Dice {dice.name} way out of bounds check: Pos={pos}, WayOut={wayOut}");
        return wayOut;
    }
    
    void ForceInvalidRoll(string reason)
    {
        Debug.Log($"Forcing invalid roll: {reason}");
        
        // Stop rolling state
        isRolling = false;
        CancelInvoke(nameof(CheckDiceSettled));
        
        // Stop dice movement
        rb1.velocity = Vector3.zero;
        rb1.angularVelocity = Vector3.zero;
        rb2.velocity = Vector3.zero;
        rb2.angularVelocity = Vector3.zero;
        
        // Show "Roll Again" message
        if (scoreText != null)
        {
            scoreText.text = "Roll Again";
            scoreText.color = Color.red;
        }
        
        Debug.Log("Invalid roll detected - showing 'Roll Again'");
    }
    
    bool IsDiceLyingFlat(GameObject dice)
    {
        // Simple but effective flatness detection
        Vector3 pos = dice.transform.position;
        
        // 1. Height check - dice should be around 0.5 units high when flat
        float expectedHeight = 0.5f;
        float heightTolerance = 0.15f; // More permissive
        bool correctHeight = Mathf.Abs(pos.y - expectedHeight) <= heightTolerance;
        
        if (!correctHeight)
        {
            return false;
        }
        
        // 2. Simple stability check - raycast down from dice center
        RaycastHit hit;
        bool hitFloor = Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out hit, 0.8f);
        
        if (!hitFloor || hit.collider.name != "Floor")
        {
            return false; // Not sitting on floor
        }
        
        // 3. Check if dice is too close to any wall (enhanced wall detection)
        float wallDistance = 0.7f; // Dice center must be this far from walls
        bool tooCloseToWall = (Mathf.Abs(pos.x) > 5f - wallDistance || 
                              Mathf.Abs(pos.z) > 5f - wallDistance);
        
        if (tooCloseToWall)
        {
            return false; // Too close to wall, likely leaning
        }
        
        return true; // All checks passed
    }
    
    Vector3 GetBottomFaceNormal(GameObject dice)
    {
        // Find which face is pointing most downward (toward world Y-)
        Vector3 worldDown = Vector3.down;
        
        // The six face normals in local space of the dice
        Vector3[] localFaceNormals = {
            Vector3.up,      // Local Y+ (top face)
            Vector3.down,    // Local Y- (bottom face)
            Vector3.forward, // Local Z+ (front face)
            Vector3.back,    // Local Z- (back face)
            Vector3.right,   // Local X+ (right face)
            Vector3.left     // Local X- (left face)
        };
        
        float maxDot = -1f;
        Vector3 bestFaceNormal = Vector3.down;
        
        // Transform each local face normal to world space and check alignment with world down
        for (int i = 0; i < localFaceNormals.Length; i++)
        {
            Vector3 worldFaceNormal = dice.transform.TransformDirection(localFaceNormals[i]);
            float dot = Vector3.Dot(worldFaceNormal, worldDown);
            
            if (dot > maxDot)
            {
                maxDot = dot;
                bestFaceNormal = worldFaceNormal;
            }
        }
        
        Debug.Log($"Bottom face normal found: {bestFaceNormal} (alignment with down: {maxDot:F3})");
        return bestFaceNormal;
    }
    
    Vector3[] GetBottomFaceCorners(GameObject dice, Vector3 bottomFaceNormal)
    {
        // For a 1x1x1 cube, get the 4 corners of the bottom face
        Vector3 diceCenter = dice.transform.position;
        float halfSize = 0.5f; // Half the size of the cube
        
        // Determine which face is the bottom based on the normal
        Vector3[] corners = new Vector3[4];
        
        // Get the dice's transform vectors
        Vector3 right = dice.transform.right;
        Vector3 up = dice.transform.up;
        Vector3 forward = dice.transform.forward;
        
        // Determine which local axis the bottom normal aligns with most closely
        float dotRight = Mathf.Abs(Vector3.Dot(bottomFaceNormal, right));
        float dotUp = Mathf.Abs(Vector3.Dot(bottomFaceNormal, up));
        float dotForward = Mathf.Abs(Vector3.Dot(bottomFaceNormal, forward));
        
        if (dotUp > dotRight && dotUp > dotForward)
        {
            // Bottom face is aligned with up/down axis
            Vector3 faceCenter = diceCenter + (Vector3.Dot(bottomFaceNormal, up) > 0 ? -up : up) * halfSize;
            corners[0] = faceCenter + right * halfSize + forward * halfSize;
            corners[1] = faceCenter - right * halfSize + forward * halfSize;
            corners[2] = faceCenter - right * halfSize - forward * halfSize;
            corners[3] = faceCenter + right * halfSize - forward * halfSize;
        }
        else if (dotRight > dotForward)
        {
            // Bottom face is aligned with right/left axis
            Vector3 faceCenter = diceCenter + (Vector3.Dot(bottomFaceNormal, right) > 0 ? -right : right) * halfSize;
            corners[0] = faceCenter + up * halfSize + forward * halfSize;
            corners[1] = faceCenter - up * halfSize + forward * halfSize;
            corners[2] = faceCenter - up * halfSize - forward * halfSize;
            corners[3] = faceCenter + up * halfSize - forward * halfSize;
        }
        else
        {
            // Bottom face is aligned with forward/back axis
            Vector3 faceCenter = diceCenter + (Vector3.Dot(bottomFaceNormal, forward) > 0 ? -forward : forward) * halfSize;
            corners[0] = faceCenter + right * halfSize + up * halfSize;
            corners[1] = faceCenter - right * halfSize + up * halfSize;
            corners[2] = faceCenter - right * halfSize - up * halfSize;
            corners[3] = faceCenter + right * halfSize - up * halfSize;
        }
        
        Debug.Log($"Bottom face corners calculated: {corners[0]}, {corners[1]}, {corners[2]}, {corners[3]}");
        return corners;
    }
}