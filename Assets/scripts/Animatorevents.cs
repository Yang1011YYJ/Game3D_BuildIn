using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animatorevents : MonoBehaviour
{
    [Header("UI")]
    public GameObject Phone;

    void Start()
    {
        Phone.SetActive(false);
    }
    public void OpenPhone()
    {
        Phone.SetActive(true);
    }
}
