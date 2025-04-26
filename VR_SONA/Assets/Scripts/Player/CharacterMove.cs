using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMove : MonoBehaviour
{
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public float runMultiplier = 2f;
    public float rotationSpeed = 120f;
    public float jumpPower = 10.0f;
    public float gravity = -0.1f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity;
    private GameObject viewCamera;
    private Transform lightTransform;
    private SkinnedMeshRenderer meshRenderer;

    // Animator Hash
    private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
    private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
    private static readonly int JumpState = Animator.StringToHash("Base Layer.jump");
    private static readonly int DamageState = Animator.StringToHash("Base Layer.damage");
    private static readonly int DownState = Animator.StringToHash("Base Layer.down");
    private static readonly int FaintState = Animator.StringToHash("Base Layer.faint");
    private static readonly int StandUpFaintState = Animator.StringToHash("Base Layer.standup_faint");

    private static readonly int JumpTag = Animator.StringToHash("Jump");
    private static readonly int DamageTag = Animator.StringToHash("Damage");
    private static readonly int FaintTag = Animator.StringToHash("Faint");

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int JumpPoseParameter = Animator.StringToHash("JumpPose");

    private Dictionary<int, bool> status = new Dictionary<int, bool>
    {
        {1, false}, // Jump
        {2, false}, // Damage
        {3, false}, // Faint
    };

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        viewCamera = GameObject.Find("MainCamera");
        lightTransform = GameObject.Find("Directional Light").transform;
        meshRenderer = transform.Find("Player").GetComponent<SkinnedMeshRenderer>();
        StartCoroutine(SnapToGroundAfterPhysics());
    }

    void Update()
    {
        CAMERA();
        DIRECTION_LIGHT();
        GRAVITY();
        STATUS();

        if (!status.ContainsValue(true))
        {
            MOVE();
            JUMP();
            DAMAGE();
            FAINT();
        }
        else
        {
            int activeStatus = 0;
            foreach (var s in status)
            {
                if (s.Value) { activeStatus = s.Key; break; }
            }
            if (activeStatus == 1) { MOVE(); JUMP(); FAINT(); }
            else if (activeStatus == 2) { DAMAGE(); }
            else if (activeStatus == 3) { FAINT(); }
        }
    }

    private void STATUS()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        status[1] = stateInfo.tagHash == JumpTag;
        status[2] = stateInfo.tagHash == DamageTag;
        status[3] = stateInfo.tagHash == FaintTag;
    }

    private void CAMERA()
    {
        viewCamera.transform.position = transform.position + new Vector3(0, 0.5f, 2.0f);
    }

    private void DIRECTION_LIGHT()
    {
        Vector3 pos = lightTransform.position - transform.position;
        meshRenderer.material.SetVector("_LightDir", pos);
    }

    private void GRAVITY()
    {
        if (CheckGrounded())
        {
            if (moveDirection.y < -0.1f) moveDirection.y = -0.1f;
        }
        moveDirection.y += gravity;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private bool CheckGrounded()
    {
        if (controller.isGrounded) return true;
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        return Physics.Raycast(ray, 0.11f);
    }

    private void MOVE()
    {
        float speed = animator.GetFloat(SpeedParameter);
        if (Input.GetKey(KeyCode.Z)) { speed = Mathf.Min(speed + 0.01f, 2f); }
        else { speed = Mathf.Max(speed - 0.01f, 1f); }
        animator.SetFloat(SpeedParameter, speed);

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != MoveState)
            {
                animator.CrossFade(MoveState, 0.1f, 0, 0);
            }

            float currentSpeed = animator.GetFloat(SpeedParameter); // ★ 이름 다르게
            Vector3 velocity = transform.rotation * new Vector3(0, 0, currentSpeed);
            MOVE_XZ(velocity);
            MOVE_RESET();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag)
                animator.CrossFade(MoveState, 0.1f, 0, 0);
        }

        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up, 1.0f);
        else if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up, -1.0f);

        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            KEY_UP();
    }

    private void KEY_UP()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != JumpState && !animator.IsInTransition(0))
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                    animator.CrossFade(IdleState, 0.1f, 0, 0);
            }
            else if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    if (Input.GetKey(KeyCode.LeftArrow)) animator.CrossFade(MoveState, 0.1f, 0, 0);
                    else if (Input.GetKey(KeyCode.RightArrow)) animator.CrossFade(MoveState, 0.1f, 0, 0);
                    else animator.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
        }
    }

    private void MOVE_XZ(Vector3 velocity)
    {
        moveDirection = new Vector3(velocity.x, moveDirection.y, velocity.z);
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void MOVE_RESET()
    {
        moveDirection.x = 0;
        moveDirection.z = 0;
    }

    private void JUMP()
    {
        if (CheckGrounded())
        {
            if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space))
                && animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag
                && !animator.IsInTransition(0))
            {
                animator.CrossFade(JumpState, 0.1f, 0, 0);
                moveDirection.y = jumpPower;
                animator.SetFloat(JumpPoseParameter, moveDirection.y);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState && !animator.IsInTransition(0) && JumpPoseParameter < 0)
            {
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                    animator.CrossFade(MoveState, 0.3f, 0, 0);
                else
                    animator.CrossFade(IdleState, 0.3f, 0, 0);
            }
        }
        else
        {
            if (!CheckGrounded())
            {
                if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState && !animator.IsInTransition(0))
                {
                    animator.SetFloat(JumpPoseParameter, moveDirection.y);
                }
            }
        }
    }

    private void DAMAGE()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            animator.CrossFade(DamageState, 0.1f, 0, 0);
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && animator.GetCurrentAnimatorStateInfo(0).tagHash == DamageTag && !animator.IsInTransition(0))
            animator.CrossFade(IdleState, 0.3f, 0, 0);
    }

    private void FAINT()
    {
        if (Input.GetKeyDown(KeyCode.W))
            animator.CrossFade(DownState, 0.1f, 0, 0);
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == DownState && !animator.IsInTransition(0))
            animator.CrossFade(FaintState, 0.3f, 0, 0);
        if (Input.GetKeyDown(KeyCode.E) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == FaintState && !animator.IsInTransition(0))
            animator.CrossFade(StandUpFaintState, 0.1f, 0, 0);
    }

    private IEnumerator SnapToGroundAfterPhysics()
    {
        yield return new WaitForFixedUpdate();

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Debug.DrawRay(origin, Vector3.down * 10f, Color.red, 3f);

        if (Physics.Raycast(origin, Vector3.down, out hit, 10f))
        {
            Vector3 newPos = hit.point + Vector3.up * controller.height / 2f;
            transform.position = newPos;
        }
        else
        {
            Debug.LogWarning("Ray가 아무것도 맞추지 못했어요!");
        }
    }
}