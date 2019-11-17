using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPositionFollow : MonoBehaviour
{
    public GameObject player;
    
    private Vector3 _startPos;

    private void Start() 
    {
        _startPos = this.transform.position;
    }

    private void LateUpdate()
    {
         if(player != null)
         {
             this.transform.position = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);
         }
         else
         {
             this.transform.position = _startPos;
         }
    }
}
