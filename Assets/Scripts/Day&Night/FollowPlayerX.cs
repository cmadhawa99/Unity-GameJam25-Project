using UnityEngine;

public class FollowPlayerXY : MonoBehaviour { // <-- Renamed class
    // Drag your Player GameObject here in the Inspector
    public Transform playerTransform;

    // We store this object's starting Z position
    private float startZ;

    // --- NEW VARIABLE ---
    // We will store the *difference* in Y between us and the player
    private float yOffset;

    void Start() {
        // Check if the player transform is assigned
        if (playerTransform == null) {
            Debug.LogError("Player Transform is not assigned in " + gameObject.name);
            return; // Stop if no player is set
        }

        // --- MODIFIED START ---
        // Store the initial Z position
        startZ = transform.position.z;

        // Calculate and store the initial vertical distance
        // between this object (e.g., the Sun) and the player.
        yOffset = transform.position.y - playerTransform.position.y;
    }

    void LateUpdate() {
        // If we have a player, update our position
        if (playerTransform != null) {

            // --- MODIFIED LATEUPDATE ---

            // 1. Get the player's current Y and add our stored offset
            float newY = playerTransform.position.y + yOffset;

            // 2. Create a new position vector.
            // We use the player's X, our *new* calculated Y, and our original Z.
            Vector3 newPosition = new Vector3(playerTransform.position.x, newY, startZ);

            // 3. Apply the new position
            transform.position = newPosition;
        }
    }
}