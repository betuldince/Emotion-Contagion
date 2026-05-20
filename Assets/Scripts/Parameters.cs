using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Parameters : MonoBehaviour
{
    public enum Distribution
    {
        Mixed,
        Run,
        Hide
    }

    // Determines the size of an agent's emotional contagion area of effect
    public static float contagionAOE = 5f;

    // ** Currently OBSOLETE: current behavior is based on all 3 conditions
    // Determines which distribution agents will choose their initial destination based on
    public static Distribution distribution = Distribution.Mixed;

    // Menu "Behavior Contagion" toggle — majority run/hide + destination from neighbors in AOE
    public static bool behaviourActive = false;

    // Proportional run/hide + B (independent of behaviourActive / majority toggle)
    public static bool proportionActive = false;
    // Toggles screaming, groaning, and blood
    public static bool gore = true;

    // the probability that a student will attack the shooter
    public static float fightProbability = 0f;

    // Contagion strength B (0-1). Destination follows crowd majority only when B > 0.5
    public static float proportionStrengthB = 0f;

    [SerializeField] Slider aoeSlider;

    [SerializeField] Slider proportionStrengthBSlider;
    [SerializeField] TMP_Dropdown distributionSelect;
    [SerializeField] Toggle emotionToggle;
    [SerializeField] Toggle proportionToggle;
    [SerializeField] Toggle goreToggle;
    [SerializeField] Slider fightProbabilitySlider;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void assignParameters()
    {
        contagionAOE = aoeSlider.value;
        distribution = (Distribution)distributionSelect.value;
        behaviourActive = emotionToggle.isOn;
        proportionActive = proportionToggle.isOn;
        gore = goreToggle.isOn;
        fightProbability = fightProbabilitySlider.value / 100;
        print(fightProbability);
        proportionStrengthB = proportionStrengthBSlider.value;
    }
}
