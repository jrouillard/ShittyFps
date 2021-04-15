using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    public Transform tip;
    public GameObject muzzleFlash;

    public float shootForce, upwardForce;

    private Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void Shoot(GameObject projectile, GameObject target)
    {
        GameObject bullet = Instantiate(projectile, tip.position, Quaternion.Euler(transform.forward)) as GameObject;
        bullet.transform.forward = transform.forward;
        BaseProjectile baseProjectile = bullet.GetComponent<BaseProjectile>();
        baseProjectile.Target = target;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb) {
            rb.AddForce(transform.forward.normalized * shootForce, ForceMode.Impulse);
        }
        Object.Destroy(bullet, baseProjectile.TTL);
        animator.SetTrigger("Shoot");
    }
}
