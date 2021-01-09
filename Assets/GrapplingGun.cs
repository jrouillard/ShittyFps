using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrapplingGun : MonoBehaviour {

    private LineRenderer lr;
    private Vector3 grapplePoint;
    private Transform stuckTo;
    private Quaternion offset;
    private float distance;
    private bool grappling;
    public int button;
    public Transform gunTip, player;
    public Camera maincamera;
    public float maxDistance = 100f;
    private SpringJoint springJoint;
    public Hook hook;
    private bool readyToShoot = true;
    private bool grappling_rewind = false;
    public float shootForce;
    private Animator animator;
    private Vector3 localPoint;
    
    public float curveSize;
    public float scrollSpeed;
    public float animSpeed;
    public AnimationCurve effectOverTime;
    public AnimationCurve curve;
    public AnimationCurve curveEffectOverDistance;
    public float segments;
    private float _time;

    private List<GameObject> spheres;


    private void ProcessBounce() {
        var vectors = new List<Vector3>();

        _time = Mathf.MoveTowards(_time, 1f,
            Mathf.Max(Mathf.Lerp(_time, 1f, animSpeed * Time.deltaTime) - _time, 0.2f * Time.deltaTime));
        
        vectors.Add(gunTip.position);

        var forward = Quaternion.LookRotation(grapplePoint - gunTip.position);
        var up = forward * Vector3.up;

        for (var i = 1; i < segments + 1; i++) {
            var delta = 1f / segments * i;
            var realDelta = delta * curveSize;
            while (realDelta > 1f) realDelta -= 1f;
            var calcTime = realDelta + -scrollSpeed * _time;
            while (calcTime < 0f) calcTime += 1f;

            var defaultPos = GetPos(delta);
            var effect = Eval(effectOverTime, _time) * Eval(curveEffectOverDistance, delta) * Eval(curve, calcTime);
                
            // spheres.ElementAt(i - 1).transform.position = defaultPos + up * effect;
            vectors.Add(defaultPos + up * effect);
        }  

        lr.positionCount = vectors.Count;
        lr.SetPositions(vectors.ToArray());
    }

    private Vector3 GetPos(float d) {
        return Vector3.Lerp(gunTip.position, grapplePoint, d);
    }

    private static float Eval(AnimationCurve ac, float t) {
        return ac.Evaluate(t * ac.keys.Select(k => k.time).Max());
    }


    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        spheres = new List<GameObject>();
        for (var i = 1; i < segments + 1; i++) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere.layer = 10;
            spheres.Add(sphere);
            
        }
        Debug.Log(spheres.Count);
    }

    void Awake() {
        lr = GetComponent<LineRenderer>();
    }

    public void Hooked() {
        grapplePoint = hook.transform.position;
        localPoint = hook.objectHit.InverseTransformPoint(grapplePoint);

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
        AddSpringJoint(grapplePoint, null, distanceFromPoint * 0.25f, distanceFromPoint * 0.8f, 50f, 7f, 4.5f);
        
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.yellow, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;

    }

    void Update() {
        grapplePoint = hook.transform.position;
        if (grappling && Input.GetKeyDown(KeyCode.LeftControl)) {
        }
        if (Input.GetMouseButtonDown(button) && readyToShoot) {
            Vector3 point = CastRay();
            ShootHook(point);
        }
        else if (Input.GetMouseButtonUp(button)) {
            RecallHook();
        }
    }

    void LateUpdate() {
        if (grappling_rewind) {
            float step = 2 * shootForce * Time.deltaTime; // calculate distance to move
            hook.transform.position = Vector3.MoveTowards(hook.transform.position, gunTip.position, step);
            
            float distanceHook = Vector3.Distance(gunTip.position, hook.transform.position);
            if (distanceHook < 0.005f)
            {
                Rigidbody hook_rb = hook.transform.GetComponent<Rigidbody>();
                hook.transform.GetComponent<Collider>().enabled = true;
                hook_rb.useGravity = false;
                hook_rb.velocity = Vector3.zero;
                grappling_rewind = false;
                grappling = false;
                stuckTo = null;
                readyToShoot = true;
                lr.positionCount = 0;
                hook.transform.parent = transform;
            }
        }
        DrawRope();
    }

    void RecallHook() {
        Destroy(springJoint);
        hook.launched = false;
        Rigidbody hook_rb = hook.transform.GetComponent<Rigidbody>();
        hook_rb.velocity = Vector3.zero;
        grappling_rewind = true;
        hook.transform.GetComponent<Collider>().enabled = false;
        hook.hooked = false;
    }

    void ShootHook(Vector3 targetPoint) {
        readyToShoot = false;
        hook.launched = true;
        hook.transform.parent = null;
        animator.SetTrigger("Grapple");
        grappling = true;
        Vector3 direction = targetPoint - gunTip.position;
        grapplePoint = hook.transform.position;
        Rigidbody hook_rb = hook.transform.GetComponent<Rigidbody>();
        hook_rb.useGravity = true;
        localPoint = hook.transform.InverseTransformPoint(grapplePoint);
        stuckTo = hook.transform.transform;
        hook_rb.AddForce(direction.normalized * shootForce, ForceMode.Impulse);

        _time = 0f;

    }


    Vector3 CastRay() {
        RaycastHit hit;
        Ray ray = maincamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out hit, maxDistance)) {
            return hit.point;
        } else {
            return ray.GetPoint(75);
        }
    }

    void AddSpringJoint(Vector3 grapplePoint, Rigidbody rb, float minDistance,  float maxDistance,  float spring, float damper, float massScale) {
        Destroy(springJoint);
        springJoint = player.gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = grapplePoint;
        springJoint.connectedBody = rb;
        springJoint.maxDistance = maxDistance;
        springJoint.minDistance = minDistance;
        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.massScale = massScale;
    }
    
    private Vector3 currentGrapplePosition;
    

    void DrawRope() {
        //If not grappling, don't draw rope
        if (!stuckTo) return;
        
        if (grappling && !hook.hooked) {
            float distanceHook = Vector3.Distance(gunTip.position, hook.transform.position);
            if (distanceHook > maxDistance) {
                RecallHook();
            }
        }
        

        Vector3 target = stuckTo.TransformPoint(localPoint);

        if (springJoint) {
            springJoint.connectedAnchor = target;
        }
        if (!hook.hooked) {
            
            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f),  new GradientColorKey(Color.magenta, 0.03f), new GradientColorKey(Color.red, 0.3f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lr.colorGradient = gradient;
            ProcessBounce();
            // DrawSimpleLine();
        } else {
            DrawSimpleLine();
        }
    }
    void DrawSimpleLine() {
        lr.positionCount = 2;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }
    public bool IsGrappling() {
        return stuckTo != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}
