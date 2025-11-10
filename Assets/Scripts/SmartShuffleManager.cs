using UnityEngine;
using System.Collections.Generic;
using System.Linq; // We'll use this to easily copy the list

public class SmartShuffleManager : MonoBehaviour {
    public AudioSource audioSource;

    // Your "master" list of all songs. Assign these in the Inspector.
    public List<AudioClip> masterPlaylist;

    // This is the "draw pile" we will play from.
    private List<AudioClip> shuffledPlaylist;

    // Tracks which song we are on.
    private int currentTrackIndex = 0;

    // Used to prevent end-to-start repeats.
    private AudioClip lastPlayedClip;

    void Start() {
        // Don't play on start, we will handle it.
        audioSource.playOnAwake = false;
        audioSource.loop = false; // We want to know when a track *ends*.

        if (masterPlaylist.Count == 0) {
            Debug.LogError("Master Playlist is empty!");
            return;
        }

        // 1. Create our first shuffled list
        shuffledPlaylist = new List<AudioClip>(masterPlaylist);
        Shuffle(shuffledPlaylist);

        // 2. Start playing
        PlayNextSong();
    }

    void Update() {
        // Check if the AudioSource is *not* playing and the game isn't paused
        if (!audioSource.isPlaying && Time.timeScale > 0) {
            // The song must have finished. Time to play the next one.
            PlayNextSong();
        }
    }

    void PlayNextSong() {
        // 1. Check if we've finished our shuffled list
        if (currentTrackIndex >= shuffledPlaylist.Count) {
            // --- TIME TO RE-SHUFFLE ---

            // Store the last song we played
            lastPlayedClip = shuffledPlaylist[shuffledPlaylist.Count - 1];

            // Reset the playlist from the master list
            shuffledPlaylist = new List<AudioClip>(masterPlaylist);

            // Shuffle it again
            Shuffle(shuffledPlaylist);

            // --- THE CRITICAL CHECK ---
            // If the *new* first song is the same as the *old* last song...
            if (shuffledPlaylist[0] == lastPlayedClip && shuffledPlaylist.Count > 1) {
                // ...swap it with another song (e.g., the last one) to prevent a repeat.
                AudioClip temp = shuffledPlaylist[0];
                int swapIndex = Random.Range(1, shuffledPlaylist.Count); // Pick any *other* song
                shuffledPlaylist[0] = shuffledPlaylist[swapIndex];
                shuffledPlaylist[swapIndex] = temp;
            }

            // Reset the index
            currentTrackIndex = 0;
        }

        // 2. Play the track
        audioSource.clip = shuffledPlaylist[currentTrackIndex];
        audioSource.Play();

        // 3. Move to the next index for next time
        currentTrackIndex++;
    }

    // Standard Fisher-Yates shuffle algorithm
    void Shuffle(List<AudioClip> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            // Pick a random index from 0 to i
            int j = Random.Range(0, i + 1);

            // Swap the elements
            AudioClip temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}