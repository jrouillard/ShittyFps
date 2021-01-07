using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // The target we are going to track
    [SerializeField] Transform target;
    // A reference to the gecko's neck
    [SerializeField] Transform headBone;

    [SerializeField] float headTrackingSpeed;
    [SerializeField] float headMaxTurnAngle;

    [SerializeField] Transform leftEyeBone;
    [SerializeField] Transform rightEyeBone;

    [SerializeField] float eyeTrackingSpeed;
    [SerializeField] float leftEyeMaxYRotation;
    [SerializeField] float leftEyeMinYRotation;
    [SerializeField] float rightEyeMaxYRotation;
    [SerializeField] float rightEyeMinYRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Bone manipulation
    void LateUpdate()
    {
        HeadTrackingUpdate();
        EyeTrackingUpdate();
    }

    void HeadTrackingUpdate()
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward,
            targetLocalLookDir,
            Mathf.Deg2Rad * headMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime));
    }

    void EyeTrackingUpdate()
    {
        Quaternion targetLeftEyeRotation = Quaternion.LookRotation(target.position - leftEyeBone.position, transform.up);
        Quaternion targetRightEyeRotation = Quaternion.LookRotation(target.position - rightEyeBone.position, transform.up);
        leftEyeBone.rotation = Quaternion.Slerp(leftEyeBone.rotation, targetLeftEyeRotation, 1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime));
        rightEyeBone.rotation = Quaternion.Slerp(rightEyeBone.rotation, targetRightEyeRotation, 1 - Mathf.Exp(-eyeTrackingSpeed * Time.deltaTime));

        float leftEyeCurrentYRotation = leftEyeBone.localEulerAngles.y;
        float rightEyeCurrentYRotation = rightEyeBone.localEulerAngles.y;

        // Move the rotation to a -180 ~ 180 range
        if (leftEyeCurrentYRotation > 180)
        {
            leftEyeCurrentYRotation -= 360;
        }
        if (rightEyeCurrentYRotation > 180)
        {
            rightEyeCurrentYRotation -= 360;
        }

        // Clamp the Y axis rotation
        float leftEyeClampedYRotation = Mathf.Clamp(leftEyeCurrentYRotation, leftEyeMinYRotation, leftEyeMaxYRotation );
        float rightEyeClampedYRotation = Mathf.Clamp(rightEyeCurrentYRotation, rightEyeMinYRotation, rightEyeMaxYRotation);

        // Apply the clamped Y rotation without changing the X and Z rotations
        leftEyeBone.localEulerAngles = new Vector3(leftEyeBone.localEulerAngles.x, leftEyeClampedYRotation, leftEyeBone.localEulerAngles.z);
        rightEyeBone.localEulerAngles = new Vector3(rightEyeBone.localEulerAngles.x, rightEyeClampedYRotation, rightEyeBone.localEulerAngles.z);
    }
}
