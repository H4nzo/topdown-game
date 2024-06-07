using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
    public SpriteRenderer minimapIcon;
    public Color deathColor;

    [Header("Collectibles")]
    public GameObject cashCollectible;

    // Update is called once per frame
    private void Start()
    {
        Instantiate(cashCollectible, this.transform.position, Quaternion.identity);
    }

    void Update()
    {
        minimapIcon.color = deathColor;
    }
}
