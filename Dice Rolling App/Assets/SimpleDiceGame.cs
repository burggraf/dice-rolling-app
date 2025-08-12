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
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(10, 0.1f, 10);
        
        // Create walls - shorter walls, very short front wall
        CreateWall("WallNorth", new Vector3(0, 1.7f, 5), new Vector3(10, 3.3f, 0.1f));
        CreateWall("WallSouth", new Vector3(0, 0.5f, -5), new Vector3(10, 1f, 0.1f)); // Front wall - very short
        CreateWall("WallEast", new Vector3(5, 1.7f, 0), new Vector3(0.1f, 3.3f, 10));
        CreateWall("WallWest", new Vector3(-5, 1.7f, 0), new Vector3(0.1f, 3.3f, 10));
        
        // Create dice
        die1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        die1.name = "Die1";
        die1.transform.position = new Vector3(0, 3, 0);
        rb1 = die1.AddComponent<Rigidbody>();
        
        die2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        die2.name = "Die2";
        die2.transform.position = new Vector3(1, 3, 0);
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
        
        // Apply gentler random forces - keep dice in the room
        rb1.AddForce(Random.Range(-3f, 3f), Random.Range(2f, 5f), Random.Range(-3f, 3f), ForceMode.Impulse);
        rb1.AddTorque(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
        
        rb2.AddForce(Random.Range(-3f, 3f), Random.Range(2f, 5f), Random.Range(-3f, 3f), ForceMode.Impulse);
        rb2.AddTorque(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
    }
}