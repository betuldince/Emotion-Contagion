using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grow : MonoBehaviour
{
    public float growRate = 3;

    float timeElapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        float t = Mathf.Clamp01(timeElapsed / growRate);

        if (!transform.localScale.Equals(Vector3.one))
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
        }
    }
}
