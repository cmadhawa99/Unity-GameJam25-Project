using UnityEngine;

public class Qgame : MonoBehaviour {
    // This method is called once per frame
    void Update() {
        // Check if the 'Escape' key was pressed down this frame
        if (Input.GetKeyDown(KeyCode.Escape)) {
            // If yes, call the QuitGame function
            QuitGame();
        }
    }

    // This is the same function from before
    public void QuitGame() {
        Application.Quit();
        Debug.Log("Quit button pressed!");
    }
}