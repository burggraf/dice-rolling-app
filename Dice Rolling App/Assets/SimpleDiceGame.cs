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
        
        // Create walls
        CreateWall("WallNorth", new Vector3(0, 2.5f, 5), new Vector3(10, 5, 0.1f));
        CreateWall("WallSouth", new Vector3(0, 2.5f, -5), new Vector3(10, 5, 0.1f));
        CreateWall("WallEast", new Vector3(5, 2.5f, 0), new Vector3(0.1f, 5, 10));
        CreateWall("WallWest", new Vector3(-5, 2.5f, 0), new Vector3(0.1f, 5, 10));
        
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
        
        // Apply random forces
        rb1.AddForce(Random.Range(-10f, 10f), Random.Range(5f, 15f), Random.Range(-10f, 10f), ForceMode.Impulse);
        rb1.AddTorque(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
        
        rb2.AddForce(Random.Range(-10f, 10f), Random.Range(5f, 15f), Random.Range(-10f, 10f), ForceMode.Impulse);
        rb2.AddTorque(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
    }
}