using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Note to self: display the timescale on the UI so the user knows what it is
public class ModifyTimescale : MonoBehaviour
{
    [SerializeField] private TMP_Text timescaleText;
    float storedRate = 1f;

    void Update()
    {
        // Check for arrow key inputs
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SlowDown();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SpeedUp();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Restore();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Pause();
        }
    }

    public void SpeedUp()
    {
        Time.timeScale += .1f;
        storedRate = Time.timeScale; // Ensure storedRate is updated
        timescaleText.text = storedRate.ToString() + "x";
        ChangePitch();
    }

    public void SlowDown()
    {
        Time.timeScale -= .1f;
        storedRate = Time.timeScale; // Ensure storedRate is updated
        timescaleText.text = storedRate.ToString() + "x";
        ChangePitch();
    }

    public void Pause()
    {
        if (Time.timeScale == 0 && storedRate > 0)
        {
            Time.timeScale = storedRate; // Resume to stored rate
            timescaleText.text = storedRate.ToString() + "x";
            ChangePitch();
        }
        else if (Time.timeScale != 0)
        {
            storedRate = Mathf.Max(0.1f, Time.timeScale); // Store current rate, ensure it’s not 0
            Time.timeScale = 0; // Pause the game
            timescaleText.text = Time.timeScale.ToString() + "x";
            ChangePitch();
        }
    }

    public void Restore()
    {
        Time.timeScale = 1f; // Reset time scale to default
        storedRate = 1f;     // Update stored rate
        timescaleText.text = storedRate.ToString() + "x";
        ChangePitch();
    }

    void ChangePitch()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources) 
        { 
            source.pitch = Time.timeScale;
        }

    }
}
