using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �̵� ����
/// �浹 
/// </summary>
public class MovePlug : BasePlugAble
{
    //�ȱ� 
    public float spdWalk = 0.15f;
    //�ٱ� 
    public float spdRun = 1.0f;
    //�ν��� �޸��� 
    public float spdBooster = 2.0f;
    //���� 
    public float spdDampTime = 0.1f;

    //���� ���� ���ΰ� ? 
    private bool flagJumpping;

    //�ִϸ����� üũ ���� / ���� �ִ��� 
    private int ckGrounded;
    private int ckJump;

    //������ ���� 
    public float jumpHeight = 1.5f;
    //���� �� ���� 
    public float jumpFrontForce = 10f;
    //���콺 �ٷ� �ӵ� ��
    public float spdMouse, spdSeeker;

    //�浹üũ
    private bool flagColliding;
    private CapsuleCollider capsuleCollider;

    // ĳ���� ĳ�� 
    private Transform playerTransform;


    // Start is called before the first frame update
    void Start()
    {
        playerTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        ckJump = Animator.StringToHash("Jump");
        ckGrounded = Animator.StringToHash("Grounded");
        controllerPlug.getAnimator.SetBool(ckGrounded, true);

        //�ܼ�Ʈ�� �÷��׸� ��� 
        controllerPlug.AddPlugs(this);
        //�ܼ�Ʈ�� �⺻ �÷��׸� ��� 
        controllerPlug.regDefaultPlugs(this.plugsCode);
        //���� ���ǵ�� �޸��� �ӵ��� ���� 
        spdSeeker = spdRun;
    }

    Vector3 rotatingMove(float horizontal, float vertical)
    {
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);

        Vector3 targetDirection = Vector3.zero;
        targetDirection = forward * vertical + right * horizontal;

        if (controllerPlug.getFlagMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(
                                            controllerPlug.getRigidbody.rotation,
                                            targetRotation,
                                            controllerPlug.lerpTurn
                );
            controllerPlug.getRigidbody.MoveRotation(newRotation);
            controllerPlug.setDirLast(targetDirection);
        }
        if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            controllerPlug.resetPostion();
        }
        return targetDirection;
    }

    private void deleteVericalVelocity()
    {
        Vector3 horizonalVelocity = controllerPlug.getRigidbody.velocity;
        horizonalVelocity.y = 0f;
        controllerPlug.getRigidbody.velocity = horizonalVelocity;
    }

    void moveManager(float horizontal, float vertical)
    {
        if (controllerPlug.getFlagGrounded())
        {
            controllerPlug.getRigidbody.useGravity = true;
        }
        else if (!controllerPlug.getAnimator.GetBool(ckJump)
                                                          && controllerPlug.getRigidbody.velocity.y > 0)
        {
            deleteVericalVelocity();
        }

        rotatingMove(horizontal, vertical);

        Vector2 dir = new Vector2(horizontal, vertical);
        spdMouse = Vector2.ClampMagnitude(dir, 1f).magnitude;
        spdSeeker += Input.GetAxis("Mouse ScrollWheel");
        spdSeeker = Mathf.Clamp(spdSeeker, spdWalk, spdRun);
        spdMouse *= spdSeeker;
        if (controllerPlug.getFlagReadyRunning())
        {
            spdMouse = spdBooster;
        }
        Debug.Log(spdFloat);
        controllerPlug.getAnimator.SetFloat("Speed", spdMouse, spdDampTime, Time.deltaTime);


    }

    private void OnCollisionStay(Collision collision)
    {
        flagColliding = true;

        if (controllerPlug.getFlagCurrentPlugs(getPlugsCode)
            && collision.GetContact(0).normal.y <= 0.1f)
        {
            float vel = controllerPlug.getAnimator.velocity.magnitude;
            Vector3 targetMove = Vector3.ProjectOnPlane(
                            playerTransform.forward,
                            collision.GetContact(0).normal
             ).normalized * vel;
            controllerPlug.getRigidbody.AddForce(targetMove, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        flagColliding = false;
    }

    void jumpManager()
    {
        if (flagJumpping && !controllerPlug.getAnimator.GetBool(ckJump)
            && controllerPlug.getFlagGrounded())
        {
            controllerPlug.LockPlugs(plugsCode);
            controllerPlug.getAnimator.SetBool(ckJump, true);
            if (controllerPlug.getAnimator.GetFloat("Speed") > 0.1f)
            {
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;

                deleteVericalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                controllerPlug.getRigidbody.AddForce(Vector3.up * velocity,
                    ForceMode.VelocityChange
                 );
            }
        }
        else if (controllerPlug.getAnimator.GetBool(ckJump))
        {
            if (!controllerPlug.getFlagGrounded() && !flagColliding
                && controllerPlug.getLockStatus())
            {
                controllerPlug.getRigidbody.AddForce(
                    playerTransform.forward * jumpFrontForce * Physics.gravity.magnitude * spdBooster,
                    ForceMode.Acceleration);
            }

            if (controllerPlug.getRigidbody.velocity.y < 0f && controllerPlug.getFlagGrounded())
            {
                controllerPlug.getAnimator.SetBool(ckGrounded, true);

                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.6f;

                flagJumpping = false;

                controllerPlug.getAnimator.SetBool(ckJump, false);

                controllerPlug.UnLockPlugs(this.plugsCode);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!flagJumpping && Input.GetButtonDown("Jump")
            && controllerPlug.getFlagCurrentPlugs(plugsCode) && !controllerPlug.getOverriding())
        {
            flagJumpping = true;
        }
    }

    public override void childFixedUpdate()
    {
        moveManager(controllerPlug.getHorizontal, controllerPlug.getVertical);
        jumpManager();
    }
}
