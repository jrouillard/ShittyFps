using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    public List<Transform> objectives;
    public Transform body;
    public float bodyHeight;
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    private Vector3 playerVelocity;
    float horizontalSpeed = 2.0f;
    float verticalSpeed = 2.0f;
    private List<Matrix4x4> restPosition;

    Vector3 GetLocalGroundPosition()
    {
        Vector3 groundPosition = new Vector3();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            Vector3 localPoint = transform.InverseTransformPoint(hit.point);
            groundPosition.y = localPoint.y - 0.5f;
        }
        return groundPosition;
    }

    Vector3 GetGoundPosition()
    {
        return transform.TransformPoint(GetLocalGroundPosition());
    }

    // Start is called before the first frame update
    void Start()
    {
        restPosition = new List<Matrix4x4>();
        Vector3 groundPosition = GetGoundPosition();
        foreach(Transform objective in objectives)
        {
            objective.position = new Vector3(objective.position.x, groundPosition.y, objective.position.z);
            Vector3 lookAtTarget = new Vector3(transform.position.x, objective.position.y, transform.position.z);
            objective.LookAt(lookAtTarget);
            restPosition.Add(Matrix4x4.TRS(objective.localPosition, objective.localRotation, objective.localScale));
        }
    }

    Vector3 FindClosestPoint(Matrix4x4 restPoint, float radius)
    {
        int layerMask = ~(1 << 6);
        RaycastHit hit;
        const float distance = 40;

        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(0, 1 - radius * Mathf.Sin(-1.5f), radius - radius * Mathf.Cos(-1.5f)));
        const float numPoints = 20;
        Vector3 globalA = new Vector3();
        Vector3 globalB = new Vector3();
        for (int i = 1; i < numPoints; i++) {
            float angle = i  * Mathf.PI * 1.3f / numPoints;
            points.Add(new Vector3(0, 1 - radius * Mathf.Sin(-1.5f + angle), radius - radius * Mathf.Cos(-1.5f + angle)));

            globalA = transform.TransformPoint(restPoint.MultiplyPoint(points[i - 1]));
            globalB = transform.TransformPoint(restPoint.MultiplyPoint(points[i]));
            float globalDistance = Vector3.Distance(globalA, globalB);
            if (i == numPoints - 1)
                Debug.DrawLine(globalA, globalB, Color.green);
            else
                Debug.DrawLine(globalA, globalB, Color.red);
            if (Physics.Raycast(globalA, globalB - globalA, out hit, globalDistance, layerMask))
            {
                return hit.point;
            }
        }
        Vector3 position = new Vector3(restPoint[0,3], restPoint[1,3], restPoint[2,3]);
        return transform.TransformPoint(position);
    }

    Vector3 CheckObjective(Matrix4x4 restPoint)
    {
        return FindClosestPoint(restPoint, 2.5f);
    }

    void UpdateBody()
    {
        Vector3 orthogonals = Vector3.zero;

        float avgSurfaceDist = 0;
        Vector3 point, a, b, c;

        for(int i = 1; i < objectives.Count; i++)
        {
            point = objectives[i].position;
            avgSurfaceDist += transform.InverseTransformPoint(point).y;
            a = (transform.position - point).normalized;
            b = ((objectives[i-1].position) - point).normalized;
            c = Vector3.Cross(b, a);
            orthogonals += c;
            Debug.DrawRay(point, a * 5, Color.red, 0);
            Debug.DrawRay(point, b * 5, Color.green, 0);
            Debug.DrawRay(point, c * 5, Color.blue, 0);
        }
        orthogonals /= objectives.Count;
        Debug.DrawRay(transform.position, orthogonals * 1000, Color.yellow);

        float step = 200.0f * Time.deltaTime;
        Quaternion objectiveRotation = Quaternion.FromToRotation(transform.up, orthogonals) * transform.rotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, objectiveRotation, step);

        Vector3 position = new Vector3();
        foreach(Transform objective in objectives)
        {
            position += transform.InverseTransformPoint(objective.position);
        }
        position /= objectives.Count;
        position.x = 0;
        position.z = 0;
        position.y += bodyHeight;
        transform.position = Vector3.MoveTowards(transform.position, transform.TransformPoint(position), 0.3f);
    }

    void UpdatePosition()
    {
        float translation = Input.GetAxis("Vertical") * 40.0f;
        float rotation = Input.GetAxis("Horizontal") * 20.0f;
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);
    }

    void ComputeObjectives()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].position = CheckObjective(restPosition[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            bodyHeight -= 0.1f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            bodyHeight += 0.1f;
        }
        UpdatePosition();
        ComputeObjectives();
        UpdateBody();
    }
}
