using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderListener : MonoBehaviour
{
    [SerializeField] TMP_Text valueText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChanged(float newValue)
    {
        valueText.text = newValue.ToString();
    }
}
