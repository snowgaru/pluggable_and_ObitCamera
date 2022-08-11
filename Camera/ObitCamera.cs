using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  1. 카메라로부터 위치 오프셋, 피봇 오프셋을 설정
///  
///  2. 충돌체크 : 이중체크  
///  캐릭터로부터 카메라 사이
///  카메라로부터 캐릭터 사이
///  
/// 3.recoil 
/// 
/// 4 fov
/// </summary>

[RequireComponent(typeof(Camera))]
public class ObitCamera : MonoBehaviour
{
    public Transform charctorPlayer;

    public Vector3 pivotOffset = new Vector3(0, -1f, 0);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2f);

    public float smooth = 10f;
    public float aimingMouseSpeedH = 6;
    public float aimingMouseSpeedV = 6;
    public float angleMaxV = 30.0f;
    public float angleMinV = -60.0f;

    public float angleBounceRecoil = 5f; //반동?

    private float angleHorizontal = 0f;
    private float angleVertical = 0f;

    private Transform transformCamera;

    private Camera fovCamera;

    //플레이어로 부터 카메라까지 벡터
    private Vector3 posRealCamera;

    //카메라와 플레이어 사이 거리
    private float posDistanceRealCamera;

    //보간 조준할때 쓸꺼 부드럽게 이동하게 해주는것
    private Vector3 lerpPivotOffset;
    private Vector3 lerpCamOffset;
    private Vector3 targetPivotOffset;
    private Vector3 targetCamOffset;

    private float lerpDefaultFov;
    private float lerpTargetFov;

    private float maxVerticalAngleTarget;
    private float angleRecoil = 0f;

    public float getHorizontal
    {
        //get => angleHorizontal;
        get
        {
            return angleHorizontal;
        }
    }

    private void Awake()
    {
        transformCamera = transform;
        fovCamera = transformCamera.GetComponent<Camera>();

        transformCamera.position = charctorPlayer.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        transformCamera.rotation = Quaternion.identity;

        posRealCamera = transformCamera.position - charctorPlayer.position;
        posDistanceRealCamera = posRealCamera.magnitude - 0.5f;

        lerpPivotOffset = pivotOffset;
        lerpCamOffset = camOffset;

        lerpDefaultFov = fovCamera.fieldOfView;
        angleHorizontal = charctorPlayer.eulerAngles.y;

        //리셋 3종
        //aim
        //fov
        //angle

        resetAimOffset();
        resetFOV();
        resetMaxVAngle();
    }

    public void resetAimOffset()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }

    public void resetFOV()
    {
        this.lerpTargetFov = this.lerpDefaultFov;
    }

    public void resetMaxVAngle()
    {
        maxVerticalAngleTarget = angleMaxV;
    }

    public void recoilBounceAnlgeV(float val)
    {
        angleRecoil = val;
    }

    public void setPosTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }

    public void setFOV(float _val)
    {
        this.lerpTargetFov = _val;
    }

    bool ckViewingPos(Vector3 ckPos, float playerHeight)
    {
        Vector3 target = charctorPlayer.position + (Vector3.up * playerHeight);

        if(Physics.SphereCast(ckPos, 0.2f, target - ckPos, out RaycastHit hit, posDistanceRealCamera))
        {
            if(hit.transform != charctorPlayer && !hit.transform.GetComponent<Collider>().isTrigger) //통과하지 않았다는 뜻
            {
                return false;
            }
        }
        return true;
    }

    bool ckViewingPosR(Vector3 ckPos, float playerHeight, float maxDistance)
    {
        Vector3 origin = charctorPlayer.position + (Vector3.up * playerHeight);

        if (Physics.SphereCast(origin, 0.2f, ckPos - origin, out RaycastHit hit, maxDistance))
        {
            if(hit.transform != charctorPlayer && !hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger) //통과하지 않았다는 뜻
            {
                return false;
            }
        }
        return true;
    }

    bool ckDoubleViewingPos(Vector3 ckPos, float offset)
    {
        float playerFocusHeight = charctorPlayer.GetComponent<CapsuleCollider>().height * 0.75f;
        return ckViewingPos(ckPos, playerFocusHeight) && ckViewingPosR(ckPos, playerFocusHeight, offset);
    }

    void Update()
    {
        angleHorizontal += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1f) * aimingMouseSpeedH;
        angleVertical += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1f) * aimingMouseSpeedV;

        angleVertical = Mathf.Clamp(angleVertical, angleMinV, maxVerticalAngleTarget);

        angleVertical = Mathf.LerpAngle(angleVertical, angleVertical + angleRecoil, 10 * Time.deltaTime);

        Quaternion camRotationY = Quaternion.Euler(0, angleHorizontal, 0);
        Quaternion aimRotation = Quaternion.Euler(-angleVertical, angleHorizontal, 0f);

        transformCamera.rotation = aimRotation;

        fovCamera.fieldOfView = Mathf.Lerp(fovCamera.fieldOfView, lerpTargetFov, Time.deltaTime);
        Vector3 posBaseTemp = charctorPlayer.position + camRotationY * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset;

        for (float offsetZ = targetCamOffset.z; offsetZ <= 0f; offsetZ += 0.5f)
        {
            noCollisionOffset.z = offsetZ;

            if(ckDoubleViewingPos(posBaseTemp + aimRotation * noCollisionOffset, Mathf.Abs(offsetZ)) || offsetZ == 0)
            {
                break;
            }
        }

        lerpCamOffset = Vector3.Lerp(lerpCamOffset, noCollisionOffset, smooth * Time.deltaTime);
        lerpPivotOffset = Vector3.Lerp(lerpPivotOffset, targetPivotOffset, smooth * Time.deltaTime);

        transformCamera.position = charctorPlayer.position + camRotationY * lerpPivotOffset + aimRotation * lerpCamOffset;

        if (angleRecoil > 0)
        {
            angleRecoil -= angleBounceRecoil * Time.deltaTime;
        }
        else if (angleRecoil < 0)
        {
            angleRecoil += angleBounceRecoil * Time.deltaTime;
        }
    }

    public float getCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - lerpPivotOffset).magnitude);
    }

}
