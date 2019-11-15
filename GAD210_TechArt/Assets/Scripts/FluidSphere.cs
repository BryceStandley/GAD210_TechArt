using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSphere : MonoBehaviour
{
    public GameObject waterParticle;
    public GameObject waterParent;
    public int waterAmount;

    private void Awake() 
    {
        bool moved = false;
        for(int i = 0; i < waterAmount; i++)
        {
            Instantiate(waterParticle, waterParent.transform);
            if(moved)
            {
                this.transform.position += new Vector3(1,0,1);
                moved = false;
            }
            else
            {
                this.transform.position -= new Vector3(1,0,1);
                moved = true;
            }
        }
    }

}
