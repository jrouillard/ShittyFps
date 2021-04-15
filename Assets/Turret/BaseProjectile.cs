using UnityEngine;
using System.Collections;

public abstract class BaseProjectile : MonoBehaviour {
    public GameObject explosionPrefab;
    public int TTL;
    public int explosionForce;
    public int explosionRadius;

    public GameObject Target { get; set; }

    protected void Explode(Vector3 position, Quaternion rotation)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, rotation);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        Object.Destroy(explosion, 1.5f);
    }
}