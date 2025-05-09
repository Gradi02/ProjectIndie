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
    [SerializeField] private string dash = "Dash";
    [SerializeField] private string attack = "Attack";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction interactAction;
    private InputAction dashAction;
    private InputAction attackAction;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpPressed { get; private set; }
    public bool jumpHeld { get; private set; }
    public bool jumpReleased { get; private set; }
    public bool crouchPressed { get; private set; }
    public bool crouchHeld { get; private set; }
    public bool crouchReleased { get; private set; }
    public bool interactTrigger { get; private set; }
    public bool dashTrigger { get; private set; }
    public bool attackTrigger { get; private set; }


    public static PlayerInputHandler Instance { get; private set; }
    private const string BINDINGS_KEY = "PlayerControlsBinding";


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
            return;
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(look);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        crouchAction = playerControls.FindActionMap(actionMapName).FindAction(crouch);
        interactAction = playerControls.FindActionMap(actionMapName).FindAction(interact);
        dashAction = playerControls.FindActionMap(actionMapName).FindAction(dash);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        //Skok przenioslem do update

        //Crouch tez w update

        interactAction.performed += context => interactTrigger = true;
        interactAction.canceled += context => interactTrigger = false;

        attackAction.performed += context => attackTrigger = true;
        attackAction.canceled += context => attackTrigger = false;

        dashAction.performed += context => dashTrigger = true;
        dashAction.canceled += context => dashTrigger = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
        interactAction.Enable();
        dashAction.Enable();
        attackAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
        interactAction.Disable();
        dashAction.Disable();
        attackAction.Disable();
    }

    private void Update()
    {
        if (jumpAction != null)
        {
            jumpPressed = jumpAction.WasPressedThisFrame();
            jumpHeld = jumpAction.IsPressed();
            jumpReleased = jumpAction.WasReleasedThisFrame();
        }
        else
        {
            jumpPressed = false;
            jumpHeld = false;
            jumpReleased = false;
        }

        if(crouchAction != null)
        {
            crouchPressed = crouchAction.WasPerformedThisFrame();
            crouchHeld = crouchAction.IsPressed();
            crouchReleased = crouchAction.WasReleasedThisFrame();
        }
        else
        {
            crouchPressed = false;
            crouchHeld = false;
            crouchReleased = false;
        }
    }

    public void SaveControlOverrides()
    {
        if (playerControls == null)
        {
            Debug.LogError("InputActionAsset nie jest przypisany w InputSettingsManager!");
            return;
        }
        
        string bindingsJson = playerControls.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BINDINGS_KEY, bindingsJson);
        PlayerPrefs.Save(); // Opcjonalnie: wymuœ zapis od razu

        Debug.Log("Zapisano ustawienia sterowania.");
    }

    public void LoadControlOverrides()
    {
        if (playerControls == null)
        {
            Debug.LogError("InputActionAsset nie jest przypisany w InputSettingsManager!");
            return;
        }

        if (PlayerPrefs.HasKey(BINDINGS_KEY))
        {
            string bindingsJson = PlayerPrefs.GetString(BINDINGS_KEY);
            playerControls.LoadBindingOverridesFromJson(bindingsJson);

            Debug.Log("Wczytano ustawienia sterowania.");
        }
        else
        {
            Debug.Log("Nie znaleziono zapisanych ustawieñ sterowania. U¿ywane s¹ domyœlne.");
        }
    }


    /// <summary>
    /// Funkcja do vibracji pada - nie wiem czy dzia³a narazie zostawiam takie cos
    /// </summary>
    /// <param name="lowFrequency"></param>
    /// <param name="highFrequency"></param>
    public void ShakeGamepad(float lowFrequency, float highFrequency)
    {
        Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
    }
}
