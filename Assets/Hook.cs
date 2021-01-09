using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Hook : MonoBehaviour
{
    // Start is called before the first frame update
    
    bool _hooked;

    public LayerMask whatIsGrappleable;
    public UnityEvent hasHooked;
    public Transform objectHit { get; set; }
    public bool launched { get; set; }

    public bool hooked
    {
        get { return _hooked; }
        set {
            if (value) {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            } else {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            }
            _hooked = value; 
        }
    }

    void Start()
    {
        launched = false;
        hooked = false;
    }
    // void LateUpdate() {
    //     if (hooked) {
    //         transform.position = v3;
    //         transform.rotation = q;
    //     } else {
    //         v3 = transform.position;
    //         q = transform.rotation;
    //     }
    // }
    void OnCollisionEnter(Collision collision)
    {
        int layer = collision.gameObject.layer;
        if (!launched || whatIsGrappleable != (whatIsGrappleable | (1 << layer))) return;
        hooked = true;
        launched = false;
        transform.parent = collision.collider.gameObject.transform;
        objectHit = collision.collider.gameObject.transform;
        hasHooked.Invoke();
    }
}
