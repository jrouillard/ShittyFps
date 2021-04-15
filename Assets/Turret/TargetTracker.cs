using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTracker : MonoBehaviour
{
    public float trackingSpeed = 10f;

    GameObject target = null;
    Vector3 lastKnownPosition = Vector3.zero;
    Quaternion lookAtRotation;

    void Update()
    {
        if (target)
        {
            if (lastKnownPosition != target.transform.position)
            {
                lastKnownPosition = target.transform.position;
                lookAtRotation = Quaternion.LookRotation(lastKnownPosition - transform.position);
            }

            if (transform.rotation != lookAtRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, trackingSpeed * Time.deltaTime);
            }
        }
    }

    public void FocusOn(GameObject target)
    {
        this.target = target;
    }
}
