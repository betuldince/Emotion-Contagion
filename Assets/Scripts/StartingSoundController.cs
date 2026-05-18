using System.Collections;
using UnityEngine;

public class StartingSoundController : MonoBehaviour
{
    public AudioSource startingSound;
    public float stopTime = 3;
    public float fadeTime = 1.5f;
    public bool isTraining = false;
    //bool isPlay;


    // Start is called before the first frame update
    void Start()
    {
        startingSound = GetComponent<AudioSource>();
        // might remove if they want it to Play all the time
        //isPlay = (Playgame.emotion_level == Constants.EMOTION_HIGH) || isTraining;
        startingSound.enabled = true;
        startingSound.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (SimController.firstShot)
        {
            StartCoroutine("StopTimer", stopTime);
        }

    }

    IEnumerator StopTimer(float timer)
    {
        yield return new WaitForSeconds(timer);

        //StartCoroutine("FadeAudio", fadeTime);

        float currentTime = 0;
        float start = startingSound.volume;
        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            startingSound.volume = Mathf.Lerp(start, 0, currentTime / fadeTime);
            yield return null;
        }

        startingSound.Stop();
        startingSound.loop = false;

        yield break;
    }
}
