using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopperController : MonoBehaviour
{
    public float takeOffSpeed = 1f;
    public GameObject windFX;

    private void FixedUpdate()
    {
        transform.Translate(0, transform.position.y * takeOffSpeed * Time.deltaTime, 0);
        GetComponent<Animator>().enabled = false;

        if(transform.position.y > 24f){
            windFX.SetActive(false);
            gameObject.SetActive(false);
        }
    }

}
