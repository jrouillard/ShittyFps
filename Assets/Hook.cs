using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Hook : MonoBehaviour
{
    // Start is called before the first frame update
    

    public LayerMask whatIsGrappleable;
    public UnityEvent hasHooked;
    public Transform objectHit { get; set; }
    bool _launched;
    public bool launched { 
        get { return _launched; }
        set {
            if (value) {
                GetComponent<SphereCollider>().enabled = true;
            } else {
                GetComponent<SphereCollider>().enabled = false;
            }
            _launched = value; 
        } }

    bool _hooked;
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
        Debug.Log(launched);
        if (!launched || whatIsGrappleable != (whatIsGrappleable | (1 << layer))) return;
        launched = false;
        hooked = true;
        transform.parent = collision.collider.gameObject.transform;
        objectHit = collision.collider.gameObject.transform;
        hasHooked.Invoke();
    }
}
