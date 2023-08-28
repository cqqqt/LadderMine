using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowText : MonoBehaviour
{
    [SerializeField] GameObject HeartText;
    void Start()
    {
        HeartText.SetActive(false);
    }

    public void OnMouseOver()
    {
        HeartText.SetActive(true);
    }

    public void OnMouseExit()
    {
        HeartText.SetActive(false);
    }
}