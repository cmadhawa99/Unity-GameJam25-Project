using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour {
    public TextMeshProUGUI distanceText;
    public enum DisplayUnit { Meters, Yards }
    public DisplayUnit unit = DisplayUnit.Meters;

    private float totalDistance = 0f;
    private Vector3 lastPosition;

    // Conversion constant: 1 meter = 1.09361 yards
    private const float METERS_TO_YARDS = 1.09361f;

    void Start() {
        lastPosition = transform.position;
        UpdateDistanceText(); // Update text to "0m" or "0yd" on start
    }

    void Update() {
        // Create 2D vectors by ignoring the y-axis
        Vector3 currentPosition = transform.position;
        Vector3 lastPosition2D = new Vector3(lastPosition.x, 0, lastPosition.z);
        Vector3 currentPosition2D = new Vector3(currentPosition.x, 0, currentPosition.z);

        // Calculate the 2D distance moved this frame
        float distanceThisFrame = Vector3.Distance(currentPosition2D, lastPosition2D);

        // Add it to the total distance
        totalDistance += distanceThisFrame;

        // Update the last position (still need the 3D original) for the next frame
        lastPosition = transform.position;

        UpdateDistanceText();
    }

    private void UpdateDistanceText() {
        float displayDistance = 0f;
        string unitSuffix = "";

        // Check which unit to display
        if (unit == DisplayUnit.Meters) {
            displayDistance = totalDistance;
            unitSuffix = "m";
        } else if (unit == DisplayUnit.Yards) {
            // Convert from meters (Unity units) to yards
            displayDistance = totalDistance * METERS_TO_YARDS;
            unitSuffix = "yd";
        }
        // Use "F0" for whole numbers (e.g., "10 m")
        distanceText.text = $"Distance: {displayDistance.ToString("F1")} {unitSuffix}";
    }
}