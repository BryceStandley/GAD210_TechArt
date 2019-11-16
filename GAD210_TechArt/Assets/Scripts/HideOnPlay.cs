using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnPlay : MonoBehaviour
{
    void Start()
    {
        this.gameObject.SetActive(false);
    }
}
