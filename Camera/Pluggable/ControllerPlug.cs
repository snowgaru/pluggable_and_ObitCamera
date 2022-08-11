using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPlug : MonoBehaviour
{
    //기능들 
    private List<BasePlugAble> plugs;
    //우선권이 필요한 기능들
    private List<BasePlugAble> overridePlugs;

    //현재 기능 코드들
    private int currentPlugs;
    //기본 기능 코드들
    private int defaultPlugs;
    //잠긴 기능 코드들
    private int plugsLocked;

    //캐싱 
    public Transform playerCamera;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private ObitCamera cameraScript;

    //horizontal axis
    private float h;
    //vertical axis
    private float v;

    //카메라를 향하도록 움직일때 회전 되는 속도 
    public float lerpTurn = 0.05f;
    //달리기 기능이 카메라 시야각이 변경 되었을 때 기록되었나 ?
    private bool flagChangeFOV;
    //달리기 시야각
    public float runFOV = 100;
    //마지막을 향했던 방향 
    private Vector3 dirLast;
    //달리기 중인가 ?
    private bool flagRun;
    //애니메이션 h축 값 
    private int hFloat;
    //애니메이션 v축 값 
    private int vFloat;
    //땅위에 붙어 있는가 ? 
    private int flagOnGround;
    //땅과의 충돌체크를 위한 충돌체 영역 
    private Vector3 colliderGround;

    public float getHorizontal { get => h; }
    public float getVertical { get => v; }
    public ObitCamera getCameraScript { get => cameraScript; }
    public Rigidbody getRigidbody { get => playerRigidbody; }
    public Animator getAnimator { get => playerAnimator; }

    //지금 어떤 기본 플러그가 꽃혀 있는가 ? 
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

        //애니메이터 
        hFloat = Animator.StringToHash("H");
        vFloat = Animator.StringToHash("V");
        flagOnGround = Animator.StringToHash("Grounded");
    }

    // 플레이어가 이동중인가?
    // 우리가 이동하는 중에 플러그인을 붙이기 위해서다 
    public bool getFlagMoving()
    {
        //return (h != 0f) || (v != 0f);
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    //수평으로 앞으로 이동하고 있는가 
    public bool getFlagHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    //달릴수 있는 상태인가 ?
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

    //달리는게 가능한가 ? 
    public bool getFlagReadyRunning()
    {
        return flagRun && getFlagMoving() && getFlagRun();
    }

    //혹시 땅위에 있는가 ?
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
        //나중  
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
    //현재 플러그인가 ?
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

//추상화클래스를 만드는 이유는
//자식들을 하나의 클래스로 형변환하여 거기에 플러그 관리를 할 수 있다 
public abstract class BasePlugAble : MonoBehaviour
{
    //속도 구별 걷고 달리는거 구별 
    protected int spdFloat;

    protected ControllerPlug controllerPlug;

    //해쉬코드로 고유코드 만들기 
    protected int plugsCode;
    //너 뛸수 있어 ? 
    protected bool getFlagRun;

    private void Awake()
    {
        this.controllerPlug = GetComponent<ControllerPlug>();
        getFlagRun = true;
        spdFloat = Animator.StringToHash("Speed");
        //해쉬코드로 고유 코드 만들기 (Reflection Object GetType()) 
        plugsCode = this.GetType().GetHashCode();
    }

    //플러그 고유 코드 가져오기 
    public int getPlugsCode { get => plugsCode; }
    //나 달려도 될까 ?
    public bool flagAllowRun { get => getFlagRun; }

    //여기 있는 플러그도 업데이트 해줘 
    public virtual void childLateUpdate() { }
    public virtual void childFixedUpdate() { }

    //이 기능은 아주 중요하다 먼저 해야 해 
    public virtual void OnOverride() { }
}
