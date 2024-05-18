using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
   public SpriteRenderer minimapIcon;
   public Color deathColor;

    // Update is called once per frame
    void Update()
    {
        minimapIcon.color = deathColor;
    }
}
