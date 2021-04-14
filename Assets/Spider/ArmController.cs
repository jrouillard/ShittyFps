using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

public class BunnyHop
{
    private Vector3 initial;
    private Vector3 destination;
    private float animationDuration = 0.2f;
    private float startAnimationTime;
    public AnimationCurve curve;
    public float maxAltitude { get; set; }

    private bool moving = false;

    public void Start(AnimationCurve curve, float animationDuration)
    {
        startAnimationTime = Time.time - 20;
        this.curve = curve;
        this.animationDuration = animationDuration;
    }

    public void UpdatePositions(Vector3 initial, Vector3 destination)
    {
        this.initial = initial;
        this.destination = destination;
        startAnimationTime = Time.time;
        moving = true;
    }

    public Vector3 ComputePosition(Transform transform)
    {
        if (Time.time >= startAnimationTime + animationDuration)
        {
            moving = false;
            return destination;
        }

        Vector3 localInitial = transform.InverseTransformPoint(initial);
        Vector3 localDestination = transform.InverseTransformPoint(destination);
        Vector3 position = new Vector3();
        float t = (Time.time - startAnimationTime) / animationDuration;
        position.x = Mathf.Lerp(localInitial.x, localDestination.x, t);
        position.z = Mathf.Lerp(localInitial.z, localDestination.z, t);
        position.y = Mathf.Lerp(localInitial.y, localDestination.y, t) + curve.Evaluate(t * curve.keys.Select(k => k.time).Max()) * maxAltitude;
        return transform.TransformPoint(position);
    }

    public bool isMoving()
    {
        return moving;
    }
}

[ExecuteInEditMode]
public class ArmController : MonoBehaviour
{
	public Transform shoulder;
	public Transform upperArm;
	public Transform forearm;
	public Transform hand;
    public bool elbowUp;
    public AnimationCurve curve;
    public UnityEventVector3 groundHit;
    public float animationDuration;

    // hop control
    public float maxDistance;
    public float hopAltitude;
    public Transform objective;
    private BunnyHop hop = new BunnyHop();
    private Vector3 globalTarget;

    void Start()
    {
        globalTarget = objective.position;
        hop.maxAltitude = hopAltitude;
        hop.Start(curve, animationDuration);
    }

    void CheckDistance()
    {
        if (!hop.isMoving())
        {
            float distance = Vector3.Distance(globalTarget, objective.position);
            if (distance > maxDistance * transform.lossyScale.x)
            {
                hop.UpdatePositions(globalTarget, objective.position);
            }
        }
    }

    void MoveLeg()
    {
        if (hop.isMoving())
        {
            globalTarget = hop.ComputePosition(transform);
            if (!hop.isMoving())
            {
                groundHit.Invoke(globalTarget);
            }
        }
    }

    bool ComputeCircleCircleIntersection(Vector2 center1, float radius1, Vector2 center2, float radius2, out Vector2 p1, out Vector2 p2)
    {
        float distance = Vector2.Distance(center1, center2);
        p1 = new Vector2();
        p2 = new Vector2();
        if (distance > radius1 + radius2)
        {
            return false;
        }
        else if (distance < Mathf.Abs(radius1 - radius2))
        {
            return false;
        }
        else if (radius1 == radius2 && center1 == center2)
        {
            return false;
        }
        else
        {
            float a = (radius1 * radius1 - radius2 * radius2 + distance * distance) / (2 * distance);
            float h = Mathf.Sqrt(radius1 * radius1 - a * a);
            Vector2 p = center1 + a * (center2 - center1) / distance;
            p1.x = p.x + h * (center2.y - center1.y) / distance;
            p1.y = p.y - h * (center2.x - center1.x) / distance;
            p2.x = p.x - h * (center2.y - center1.y) / distance;
            p2.y = p.y + h * (center2.x - center1.x) / distance;
            return true;
        }
    }

    // Rotate shoulder horizontally (relative to the arm)
    void UpdateShoulder(Transform currentTransform, Vector3 localTarget)
    {
        Vector3 localShoulderObjective = new Vector3(localTarget.x, shoulder.localPosition.y, localTarget.z);
        Vector3 shoulderObjective = currentTransform.TransformPoint(localShoulderObjective);
        shoulder.LookAt(shoulderObjective);
    }

    void UpdateIK(Transform currentTransform, Vector3 localTarget)
    {
        Vector3 localUpperArm = currentTransform.InverseTransformPoint(upperArm.position);
        Vector3 localForearm = currentTransform.InverseTransformPoint(forearm.position);
        Vector3 localHand = currentTransform.InverseTransformPoint(hand.position);

        float distance = Vector3.Distance(localUpperArm, localTarget);
        Vector2 distanceUpperArmTarget2d = new Vector2(localUpperArm.x - localTarget.x, localUpperArm.z - localTarget.z);
        Vector2 localUpperArm2d = new Vector2(0, localUpperArm.y);
        Vector2 localTarget2d = new Vector2(distanceUpperArmTarget2d.magnitude, localTarget.y);

        float upperArmLength = Vector3.Distance(localUpperArm, localForearm);
        float forearmLength = Vector3.Distance(localForearm, localHand);
        Vector2 p1, p2;
        if (ComputeCircleCircleIntersection(localUpperArm2d, upperArmLength, localTarget2d, forearmLength, out p1, out p2)) {
            Vector3 origin = new Vector3(localUpperArm2d.x, localUpperArm2d.y, 0);
            Vector3 targetdebug = new Vector3(localTarget2d.x, localTarget2d.y, 0);
            //Debug.DrawLine(origin, targetdebug, Color.blue);
            //Debug.DrawLine(origin, Vector3.up * upperArmLength, Color.red);
            //Debug.DrawLine(origin, new Vector3(p1.x, p1.y, 0), Color.green);
            //Debug.DrawLine(origin, new Vector3(p2.x, p2.y, 0), Color.green);
            Vector2 p3;
            if (elbowUp) {
                p3 = p1.y > p2.y ? p1 : p2;
            } else {
                p3 = p1.y > p2.y ? p2 : p1;
            }
            float t = p3.x / localTarget2d.x;
            Vector3 localForearm3d = new Vector3(Mathf.LerpUnclamped(localUpperArm.x, localTarget.x, t), p3.y, Mathf.LerpUnclamped(localUpperArm.z, localTarget.z, t));
            //Debug.DrawLine(upperArm.position, currentTransform.TransformPoint(localForearm3d));
            upperArm.LookAt(currentTransform.TransformPoint(localForearm3d));
            forearm.LookAt(globalTarget);
        } else {
            upperArm.LookAt(globalTarget);
            forearm.LookAt(globalTarget);
        }
    }

    void LateUpdate()
    {
        Transform currentTransform = GetComponent<Transform>();
        Vector3 localTarget = currentTransform.InverseTransformPoint(globalTarget);
        UpdateShoulder(currentTransform, localTarget);
        UpdateIK(currentTransform, localTarget);
        // hop control
        CheckDistance();
        MoveLeg();
    }
}
