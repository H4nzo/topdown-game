using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapom : MonoBehaviour
{

}

//public class Weapon : MonoBehaviour
//{
//    public float damage = 10f;
//    public float range = 100f;

//    public Transform origin;
//    public ParticleSystem[] muzzleFlash;
//    public GameObject impactFX;
//    public GameObject bulletTrailPrefab;

//    public bool isFiring;

//    public void Shoot(RaycastHit _hit, LayerMask enemyLayer)
//    {
//        RaycastHit hit = _hit;

//        if (Physics.Raycast(origin.position, origin.forward, out hit, range))
//        {
//            // Check if the hit object belongs to the enemy layer
//            if (((1 << hit.transform.gameObject.layer) & enemyLayer) != 0)
//            {
//                Debug.Log(hit.transform.name);
//                foreach (var p in muzzleFlash)
//                {
//                    p.Play();
//                    p.loop = true;
//                }
//                GameObject impactGO = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal));
//                Destroy(impactGO, 1f);

//                // Instantiate bullet trail renderer
//                //GameObject trail = Instantiate(bulletTrailPrefab, origin.position, Quaternion.identity);
//                //TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
//                //if (trailRenderer != null)
//                //{
//                //    // Set trail renderer properties
//                //    trailRenderer.Clear();
//                //    trailRenderer.AddPosition(origin.position);
//                //    trailRenderer.AddPosition(hit.point);
//                //}
//            }
//        }
//    }

//    // Method to align the weapon's z rotation with the enemy's transform
//    public void AlignWithEnemy(Transform enemyTransform)
//    {
//        Vector3 direction = enemyTransform.position - origin.position;
//        direction.y = 0f; // Ensure no rotation around y-axis
//        Quaternion rotation = Quaternion.LookRotation(direction);
//        origin.rotation = rotation;
//    }
//}
