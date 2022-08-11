using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPlug : MonoBehaviour
{
    //��ɵ� 
    private List<BasePlugAble> plugs;
    //�켱���� �ʿ��� ��ɵ�
    private List<BasePlugAble> overridePlugs;

    //���� ��� �ڵ��
    private int currentPlugs;
    //�⺻ ��� �ڵ��
    private int defaultPlugs;
    //��� ��� �ڵ��
    private int plugsLocked;

    //ĳ�� 
    public Transform playerCamera;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private ObitCamera cameraScript;

    //horizontal axis
    private float h;
    //vertical axis
    private float v;

    //ī�޶� ���ϵ��� �����϶� ȸ�� �Ǵ� �ӵ� 
    public float lerpTurn = 0.05f;
    //�޸��� ����� ī�޶� �þ߰��� ���� �Ǿ��� �� ��ϵǾ��� ?
    private bool flagChangeFOV;
    //�޸��� �þ߰�
    public float runFOV = 100;
    //�������� ���ߴ� ���� 
    private Vector3 dirLast;
    //�޸��� ���ΰ� ?
    private bool flagRun;
    //�ִϸ��̼� h�� �� 
    private int hFloat;
    //�ִϸ��̼� v�� �� 
    private int vFloat;
    //������ �پ� �ִ°� ? 
    private int flagOnGround;
    //������ �浹üũ�� ���� �浹ü ���� 
    private Vector3 colliderGround;

    public float getHorizontal { get => h; }
    public float getVertical { get => v; }
    public ObitCamera getCameraScript { get => cameraScript; }
    public Rigidbody getRigidbody { get => playerRigidbody; }
    public Animator getAnimator { get => playerAnimator; }

    //���� � �⺻ �÷��װ� ���� �ִ°� ? 
    public int getDefaultPlugs { get => defaultPlugs; }

    private void Awake()
    {
        playerTransform = transform;
        plugs = new List<BasePlugAble>();
        overridePlugs = new List<BasePlugAble>();
        playerAnimator = GetComponent<Animator>();
        cameraScript = playerCamera.GetComponent<ObitCamera>();
        playerRigidbody = GetComponent<Rigidbody>();
        colliderGround = GetComponent<Collider>().bounds.extents;

        //�ִϸ����� 
        hFloat = Animator.StringToHash("H");
        vFloat = Animator.StringToHash("V");
        flagOnGround = Animator.StringToHash("Grounded");
    }

    // �÷��̾ �̵����ΰ�?
    // �츮�� �̵��ϴ� �߿� �÷������� ���̱� ���ؼ��� 
    public bool getFlagMoving()
    {
        //return (h != 0f) || (v != 0f);
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    //�������� ������ �̵��ϰ� �ִ°� 
    public bool getFlagHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    //�޸��� �ִ� �����ΰ� ?
    public bool getFlagRun()
    {
        foreach (BasePlugAble basePlugAble in plugs)
        {
            if (!basePlugAble.flagAllowRun)
            {
                return false;
            }
        }

        foreach (BasePlugAble overPlugAble in overridePlugs)
        {
            if (!overPlugAble.flagAllowRun)
            {
                return false;
            }
        }
        return true;
    }

    //�޸��°� �����Ѱ� ? 
    public bool getFlagReadyRunning()
    {
        return flagRun && getFlagMoving() && getFlagRun();
    }

    //Ȥ�� ������ �ִ°� ?
    public bool getFlagGrounded()
    {
        Ray ray = new Ray(playerTransform.position + Vector3.up * 2 * colliderGround.x, Vector3.down);
        return Physics.SphereCast(ray, colliderGround.x, colliderGround.x + 0.2f);
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        playerAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);
        //����  
        flagRun = Input.GetKey(KeyCode.Space);

        if (getFlagReadyRunning())
        {
            flagChangeFOV = true;
            cameraScript.setFOV(runFOV);
        }
        else if (flagChangeFOV)
        {
            cameraScript.resetFOV();
            flagChangeFOV = false;
        }
    }


    public void resetPostion()
    {
        if (dirLast != Vector3.zero)
        {
            dirLast.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(dirLast);
            Quaternion newRotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, lerpTurn);
            playerRigidbody.MoveRotation(newRotation);
        }
    }

    private void FixedUpdate()
    {
        bool flagAnyPlygActive = false;

        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePlugAble basePlugAble in plugs)
            {
                if (basePlugAble.isActiveAndEnabled && currentPlugs == basePlugAble.getPlugsCode)
                {
                    flagAnyPlygActive = true;
                    basePlugAble.childFixedUpdate();
                }
            }
        }
        else
        {
            foreach (BasePlugAble basePlugAble in overridePlugs)
            {
                basePlugAble.childFixedUpdate();
            }
        }

        if (!flagAnyPlygActive && overridePlugs.Count == 0)
        {
            playerRigidbody.useGravity = true;
            resetPostion();
        }
    }

    private void LateUpdate()
    {
        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePlugAble basePlugAble in plugs)
            {
                if (basePlugAble.isActiveAndEnabled && currentPlugs == basePlugAble.getPlugsCode)
                {
                    basePlugAble.childLateUpdate();
                }
            }
        }
        else
        {
            foreach (BasePlugAble basePlugAble in overridePlugs)
            {
                basePlugAble.childLateUpdate();
            }
        }
    }

    public void AddPlugs(BasePlugAble basePlugAble)
    {
        plugs.Add(basePlugAble);
    }

    public void regDefaultPlugs(int plugCode)
    {
        defaultPlugs = plugCode;
        currentPlugs = plugCode;
    }

    public void regPlugs(int plugCode)
    {
        if (currentPlugs == defaultPlugs)
        {
            currentPlugs = plugCode;
        }
    }

    public void UnRegPlugs(int plugCode)
    {
        if (currentPlugs == plugCode)
        {
            currentPlugs = defaultPlugs;
        }
    }

    public bool OverrideWithPlugs(BasePlugAble basePlugAble)
    {
        if (!overridePlugs.Contains(basePlugAble))
        {
            if (overridePlugs.Count == 0)
            {
                foreach (BasePlugAble plugAble in plugs)
                {
                    if (plugAble.isActiveAndEnabled && currentPlugs == plugAble.getPlugsCode)
                    {
                        plugAble.OnOverride();
                        break;
                    }
                }
            }
            overridePlugs.Add(basePlugAble);
            return true;
        }
        return false;
    }

    public bool UnOverridingPlugs(BasePlugAble plugAble)
    {
        if (overridePlugs.Contains(plugAble))
        {
            overridePlugs.Remove(plugAble);
            return true;
        }
        return false;
    }

    public bool getOverriding(BasePlugAble basePlugAble = null)
    {
        if (basePlugAble == null)
        {
            return overridePlugs.Count > 0;
        }
        return overridePlugs.Contains(basePlugAble);
    }
    //���� �÷����ΰ� ?
    public bool getFlagCurrentPlugs(int plugCode)
    {
        return currentPlugs == plugCode;
    }

    public bool getLockStatus(int plugCode = 0)
    {
        return (plugsLocked != 0 && plugsLocked != plugCode);
    }

    public void LockPlugs(int plugCode)
    {
        if (plugsLocked == 0)
        {
            plugsLocked = plugCode;
        }
    }

    public void UnLockPlugs(int plugCode)
    {
        if (plugsLocked == plugCode)
        {
            plugsLocked = 0;
        }
    }

    public Vector3 getDirLast()
    {
        return dirLast;
    }

    public void setDirLast(Vector3 direction)
    {
        dirLast = direction;
    }
}

//�߻�ȭŬ������ ����� ������
//�ڽĵ��� �ϳ��� Ŭ������ ����ȯ�Ͽ� �ű⿡ �÷��� ������ �� �� �ִ� 
public abstract class BasePlugAble : MonoBehaviour
{
    //�ӵ� ���� �Ȱ� �޸��°� ���� 
    protected int spdFloat;

    protected ControllerPlug controllerPlug;

    //�ؽ��ڵ�� �����ڵ� ����� 
    protected int plugsCode;
    //�� �ۼ� �־� ? 
    protected bool getFlagRun;

    private void Awake()
    {
        this.controllerPlug = GetComponent<ControllerPlug>();
        getFlagRun = true;
        spdFloat = Animator.StringToHash("Speed");
        //�ؽ��ڵ�� ���� �ڵ� ����� (Reflection Object GetType()) 
        plugsCode = this.GetType().GetHashCode();
    }

    //�÷��� ���� �ڵ� �������� 
    public int getPlugsCode { get => plugsCode; }
    //�� �޷��� �ɱ� ?
    public bool flagAllowRun { get => getFlagRun; }

    //���� �ִ� �÷��׵� ������Ʈ ���� 
    public virtual void childLateUpdate() { }
    public virtual void childFixedUpdate() { }

    //�� ����� ���� �߿��ϴ� ���� �ؾ� �� 
    public virtual void OnOverride() { }
}
