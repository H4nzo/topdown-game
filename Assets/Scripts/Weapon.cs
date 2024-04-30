using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
   public float damage = 30f;
   public float range = 100f;
    

   public Transform origin;
   public ParticleSystem[] muzzleFlash;
   public GameObject impactFX;

   public bool isFiring;

    public void Shoot(Transform target)
    {
        // Calculate the direction to the target
        Vector3 direction = target.position - origin.position;

        // Check if there's an obstacle between the weapon's origin and the target
        RaycastHit hit;
        if (Physics.Raycast(origin.position, direction, out hit, range))
        {

            // Check if the hit object is the target
            if (hit.transform == target)
            {
                foreach (var p in muzzleFlash)
                {
                    p.Play();
                    p.loop = true;
                }

                target.GetComponent<PlayerHealth>().TakeDamage(damage);

                // Create impact effect at the target's position
                GameObject impactGO = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f);
            }
           
        }
    }

    public void StopShooting()
    {
            foreach (var p in muzzleFlash)
            {
            p.Stop();
                p.loop = false;
            }

     }

    // Method to align the weapon's z rotation with the enemy's transform
    public void AlignWithEnemy(Transform Player)
   {
       Vector3 direction = Player.position - origin.position;
       direction.y = 0f; // Ensure no rotation around y-axis
       Quaternion rotation = Quaternion.LookRotation(direction);
       Vector3 enemyDir = Player.position - transform.position;
        Quaternion enemyRot = Quaternion.LookRotation(enemyDir);
        transform.rotation = enemyRot;
        origin.rotation = rotation;
   }
}



