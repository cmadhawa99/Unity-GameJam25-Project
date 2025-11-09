using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class MG : MonoBehaviour {
    public Transform player;            // Player reference
    public float chunkWidth = 40f;      // Width of each terrain chunk
    public int pointsPerChunk = 80;     // Smoothness (higher = smoother)
    public float heightScale = 15f;     // How tall the mountains are
    public float noiseScale = 0.05f;    // Controls mountain frequency
    public int renderDistance = 4;      // How many chunks to load ahead/behind

    // --- OLD VARIABLE (You can delete this if you only use the new one) ---
    public float groundDepth = -25f;

    // --- NEW VARIABLES ---
    [Header("Slope Settings")]
    // This controls the overall "diagonal" trend of the terrain.
    // 0.1 = rises 1 unit for every 10 units moved in X.
    // 0.0 = flat (like your original script)
    //-0.1 = falls 1 unit for every 10 units moved in X.
    public float globalSlope = 0.1f;
    // How far the mesh goes down, *relative to the slope*
    public float relativeGroundDepth = -25f;

    private float seed;
    private Dictionary<int, GameObject> chunks = new Dictionary<int, GameObject>();

    // Store the chunk index the player was last in
    private int currentChunkIndex;

    void Start() {
        seed = Random.Range(0f, 9999f);

        // Get the player's starting chunk index
        currentChunkIndex = GetPlayerChunk();
        // Load the initial chunks
        UpdateChunks();
    }

    void Update() {
        // Get the player's current chunk
        int playerChunk = GetPlayerChunk();

        // Only run the logic if the player has moved to a new chunk
        if (playerChunk != currentChunkIndex) {
            currentChunkIndex = playerChunk;
            UpdateChunks();
        }
    }

    int GetPlayerChunk() {
        return Mathf.FloorToInt(player.position.x / chunkWidth);
    }

    void UpdateChunks() {
        int playerChunk = currentChunkIndex;

        // Spawn new chunks around player
        for (int offset = -renderDistance; offset <= renderDistance; offset++) {
            int chunkIndex = playerChunk + offset;
            if (!chunks.ContainsKey(chunkIndex))
                CreateChunk(chunkIndex);
        }

        // Remove far-away chunks
        List<int> toRemove = new List<int>();
        foreach (var c in chunks) {
            if (Mathf.Abs(c.Key - playerChunk) > renderDistance)
                toRemove.Add(c.Key);
        }

        foreach (int key in toRemove) {
            Destroy(chunks[key]);
            chunks.Remove(key);
        }
    }

    // --- NEW HELPER FUNCTION ---
    /// <summary>
    /// Calculates the terrain height at any world X position,
    /// including both the global slope and Perlin noise.
    /// </summary>
    float GetHeight(float worldX) {
        // 1. Calculate the main diagonal baseline height
        float baselineHeight = worldX * globalSlope;

        // 2. Calculate the Perlin noise variation (your original logic)
        float noiseVariation = Mathf.PerlinNoise(seed, worldX * noiseScale) * heightScale;
        noiseVariation += Mathf.PerlinNoise(seed * 0.5f, worldX * noiseScale * 0.5f) * heightScale * 0.5f;
        noiseVariation -= Mathf.PerlinNoise(seed * 2f, worldX * noiseScale * 2f) * heightScale * 0.3f;

        // 3. Add them together
        return baselineHeight + noiseVariation;
    }


    // --- MODIFIED FUNCTION ---
    void CreateChunk(int index) {
        GameObject chunkObj = new GameObject("Chunk " + index);
        chunkObj.transform.parent = transform;
        // Chunks are still spawned at y=0, but their vertices
        // will be calculated using world positions.
        chunkObj.transform.position = new Vector3(index * chunkWidth, 0f, 0f);

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        PolygonCollider2D poly = chunkObj.AddComponent<PolygonCollider2D>();

        mr.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        float step = chunkWidth / (pointsPerChunk - 1);

        // --- 1. GENERATE COLLIDER POINTS ---
        List<Vector2> colliderPoints = new List<Vector2>();
        Vector3[] topPoints = new Vector3[pointsPerChunk];

        for (int i = 0; i < pointsPerChunk; i++) {
            float t = (float)i / (pointsPerChunk - 1);
            float localX = t * chunkWidth;
            float worldX = (index * chunkWidth) + localX;

            // --- MODIFIED ---
            // Use our new helper function to get the sloped height
            float y = GetHeight(worldX);

            topPoints[i] = new Vector3(localX, y, 0);
            colliderPoints.Add(new Vector2(localX, y));
        }

        // --- MODIFIED ---
        // Add bottom points for the collider.
        // These must now follow the global slope as well.
        float startBaselineY = (index * chunkWidth) * globalSlope;
        float endBaselineY = (index * chunkWidth + chunkWidth) * globalSlope;

        colliderPoints.Add(new Vector2(chunkWidth, endBaselineY + relativeGroundDepth));
        colliderPoints.Add(new Vector2(0, startBaselineY + relativeGroundDepth));
        poly.points = colliderPoints.ToArray();


        // --- 2. GENERATE MESH VERTICES (With overlap) ---
        List<Vector3> verts = new List<Vector3>(topPoints);

        // --- MODIFIED ---
        // Calculate the "stitch" vertex (which is the first vertex of the *next* chunk)
        float next_localX = chunkWidth + step;
        float next_worldX = (index * chunkWidth) + next_localX;
        float next_y = GetHeight(next_worldX); // Use helper function

        verts.Add(new Vector3(next_localX, next_y, 0)); // Add the stitch vertex

        // Our top line now has (pointsPerChunk + 1) vertices [indices 0 to pointsPerChunk]

        // --- MODIFIED ---
        // Add bottom vertices, aligned with the new, sloped ground
        float next_baselineY = next_worldX * globalSlope; // Baseline for the stitch point

        int bottomRightIdx = pointsPerChunk + 1; // Index after the stitch vertex
        int bottomLeftIdx = pointsPerChunk + 2;  // Index after that

        verts.Add(new Vector3(next_localX, next_baselineY + relativeGroundDepth, 0)); // New bottom-right
        verts.Add(new Vector3(0, startBaselineY + relativeGroundDepth, 0));         // Original bottom-left


        // --- 3. TRIANGLES (This logic is unchanged) ---
        List<int> tris = new List<int>();
        for (int i = 0; i < pointsPerChunk; i++) { // Loop one more time
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(bottomRightIdx);

            tris.Add(i + 1);
            tris.Add(bottomLeftIdx);
            tris.Add(bottomRightIdx);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.mesh = mesh;

        chunks[index] = chunkObj;
    }
}