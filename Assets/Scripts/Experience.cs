using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int Value {
        get => experienceValue;
    }

    private int experienceValue;

    public void SetExperience(int experience) {
        experienceValue = experience;
    }
}
