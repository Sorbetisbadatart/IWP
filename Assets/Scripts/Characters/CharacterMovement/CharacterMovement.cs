using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Components
    private Rigidbody2D _rb;
    private Vector2 moveInput;
    private bool isDisabled = false;

    [SerializeField] private DialogueUI dialogueUI;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {

        dialogueUI.onDialogueStart.AddListener(DisableMovement);
        dialogueUI.onDialogueEnd.AddListener(EnableMovement);
    }

    /// <summary>
    /// Gets and caches required components
    /// </summary>
    private void InitializeComponents()
    {
        _rb = GetComponent<Rigidbody2D>();

        //setup rb incase u forgot
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();


    }

    private void Update()
    {
        if (isDisabled)
            return;

        _rb.linearVelocity = moveInput * moveSpeed;
    }





    public void Move(InputAction.CallbackContext inputContext)
    {
        if (isDisabled)
            return;

        animator.SetBool("isWalking", true);

        if (inputContext.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = inputContext.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    public void DisableMovement()
    {
        isDisabled = true;
    }

    public void EnableMovement()
    {
        isDisabled = false;
    }
}