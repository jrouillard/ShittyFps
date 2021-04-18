using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLine : MonoBehaviour
{
    public float maxDistance = 80;
    public ParticleSystem impact;
    public LineRenderer[] lineRenderers;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            CastTo(transform.InverseTransformPoint(hit.point));
            //impact.Play();
            impact.transform.position = hit.point - transform.forward;
        }
        else
        {
            CastTo(new Vector3(0, 0, maxDistance));
            //impact.Play();
            //impact.Stop();
        }
    }

    void CastTo(Vector3 position)
    {
        foreach (LineRenderer lr in lineRenderers)
        {
            lr.positionCount = 2;
            lr.SetPosition(1, new Vector3(0, 0));
            lr.SetPosition(1, position);
        }
    }
}
