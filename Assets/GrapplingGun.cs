using UnityEngine;

public class GrapplingGun : MonoBehaviour {

    private LineRenderer lr;
    private Vector3 grapplePoint;
    private Transform stuckTo;
    private Quaternion offset;
    private float distance;
    private bool grappling;
    public LayerMask whatIsGrappleable;
    public int button;
    public Transform gunTip, maincamera, player;
    public float maxDistance = 100f;
    private SpringJoint joint;

    void Awake() {
        lr = GetComponent<LineRenderer>();
    }

    void Update() {
        if (grappling && Input.GetKeyDown(KeyCode.LeftControl)) {
            Debug.Log("Croucnch");
        }
        if (Input.GetMouseButtonDown(button)) {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(button)) {
            StopGrapple();
        }
    }

    //Called after Update
    void LateUpdate() {
        DrawRope();
    }

    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(maincamera.position, maincamera.forward, out hit, maxDistance, whatIsGrappleable)) {
            grappling = true;
            grapplePoint = hit.point;

            stuckTo = hit.transform;
            Debug.Log(hit.point);
            Vector3 diff = stuckTo.position - grapplePoint;
            offset = Quaternion.FromToRotation(stuckTo.forward, diff.normalized);
            distance = diff.magnitude;

            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = 50f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }


    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple() {
        stuckTo = null;
        grappling = false;
        lr.positionCount = 0;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;
    
    void DrawRope() {
        //If not grappling, don't draw rope
        if (!joint ||!stuckTo) return;


        Vector3 dir = offset * stuckTo.forward;
        grapplePoint = stuckTo.position - (dir * distance);

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}
