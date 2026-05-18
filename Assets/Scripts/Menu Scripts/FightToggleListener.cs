using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightToggleListener : MonoBehaviour
{
    [SerializeField] Slider probabilitySlider;
    [SerializeField] Toggle fightToggle;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure the slider starts as inactive
        probabilitySlider.gameObject.SetActive(false);

        // Subscribe to the toggle's onValueChanged event
        fightToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    // Called when the toggle value changes
    void OnToggleValueChanged(bool isOn)
    {
        // Activate or deactivate the slider based on the toggle's state
        probabilitySlider.gameObject.SetActive(isOn);
    }

    // Ensure to clean up listeners when the object is destroyed
    void OnDestroy()
    {
        fightToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}
