using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    public Transform tip;
    public GameObject muzzleFlash;
    public bool beam;
    public float shootForce;
    public float upwardForce;

    private Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void Shoot(GameObject projectile, GameObject target)
    {
        GameObject muzzle = Instantiate(muzzleFlash, tip.position, Quaternion.Euler(transform.forward)) as GameObject;
        Object.Destroy(muzzle, 2f);
        GameObject bullet = Instantiate(projectile, tip.position, Quaternion.Euler(transform.forward)) as GameObject;
        if (bullet != null)
        {
            bullet.transform.forward = transform.forward;
            BaseProjectile baseProjectile = bullet.GetComponent<BaseProjectile>();
            baseProjectile.Target = target;
            Object.Destroy(bullet, baseProjectile.TTL);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.AddForce(transform.forward.normalized * shootForce, ForceMode.Impulse);
            }
            if (beam)
            {
                bullet.transform.SetParent(this.transform);
            }
        }
        animator.SetTrigger("Shoot");
    }
}
