using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PanicSoundController : MonoBehaviour
{
    public AudioSource panicSound;
    private bool callOnce;
    public bool cafeteria = true;
    public float shoutDelay = 0;
    public float timeUntilFade = 5f;
    public float duration;

    public AudioClip runClip;
    public AudioClip hideClip;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        panicSound = GetComponent<AudioSource>();
        callOnce = true;
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (SimController.panicStarted)
        {
            if (callOnce)
            {
                Debug.Log("playing panic sound");

                panicSound.Play();
                StartCoroutine(StartFade());
                //print("Invoke start");
                // Invoke("FadeAudio", timeUntilFade);

                callOnce = false;
            }

        }
    }

    IEnumerator StartFade()
    {
        yield return new WaitForSeconds(timeUntilFade);
        float currentTime = 0;
        float start = panicSound.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            panicSound.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}
