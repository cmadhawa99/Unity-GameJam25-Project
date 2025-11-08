using UnityEngine;

public class BossManMovement : MonoBehaviour {
    // Drag your ragdoll's central Rigidbody (like the Torso) here
    public Rigidbody2D mainBody;

    // Drag the GameObject that has the Animator on it here
    // This might be the same GameObject as this script, or a child (like the "Sprite")
    public Animator animator;

    // How fast to move
    public float moveSpeed = 10f;

    private float horizontalInput;

    void Update() {
        // 1. Get input from A/D keys
        horizontalInput = Input.GetAxis("Horizontal"); // -1 for A, 1 for D

        if (animator == null) {
            // If animator is missing, just skip the animation logic
            return;
        }

        // 2. Tell the Animator the speed
        // We use Mathf.Abs to always send a positive speed (0 if idle, >0 if moving left or right)
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
    }

    void FixedUpdate() {
        if (mainBody == null) {
            Debug.LogError("Main Rigidbody is not assigned!");
            return;
        }

        // 4. Apply the movement as a velocity
        mainBody.linearVelocity = new Vector2(horizontalInput * moveSpeed, mainBody.linearVelocity.y);
    }

}