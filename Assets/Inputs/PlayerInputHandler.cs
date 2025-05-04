using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField] private string move = "Move";
    [SerializeField] private string look = "Look";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string crouch = "Crouch";
    [SerializeField] private string interact = "Interact";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string attack = "Attack";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction interactAction;
    private InputAction sprintAction;
    private InputAction attackAction;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpTrigger { get; private set; }
    public bool crouchTrigger { get; private set; }
    public bool interactTrigger { get; private set; }
    public float sprintSpeed { get; private set; }
    public bool attackTrigger { get; private set; }


    public static PlayerInputHandler Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(look);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        crouchAction = playerControls.FindActionMap(actionMapName).FindAction(crouch);
        interactAction = playerControls.FindActionMap(actionMapName).FindAction(interact);
        sprintAction = playerControls.FindActionMap(actionMapName).FindAction(sprint);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        jumpAction.performed += context => jumpTrigger = true;
        jumpAction.canceled += context => jumpTrigger = false;

        crouchAction.performed += context => crouchTrigger = true;
        crouchAction.canceled += context => crouchTrigger = false;

        interactAction.performed += context => interactTrigger = true;
        interactAction.canceled += context => interactTrigger = false;

        attackAction.performed += context => attackTrigger = true;
        attackAction.canceled += context => attackTrigger = false;

        sprintAction.performed += context => sprintSpeed = context.ReadValue<float>();
        sprintAction.canceled += context => sprintSpeed = 0f;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
        interactAction.Enable();
        sprintAction.Enable();
        attackAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
        interactAction.Disable();
        sprintAction.Disable();
        attackAction.Disable();
    }
}
