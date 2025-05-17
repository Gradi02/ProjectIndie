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
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string interact = "Interact";
    [SerializeField] private string dash = "Dash";
    [SerializeField] private string attack = "Attack";
    [SerializeField] private string block = "Block";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction dashAction;
    private InputAction attackAction;
    private InputAction blockAction;

    public Vector2 moveInput { get; private set; }
    public bool jumpPressed { get; private set; }
    public bool jumpHeld { get; private set; }
    public bool jumpReleased { get; private set; }
    public bool interactTrigger { get; private set; }
    public bool dashPressed { get; private set; }
    public bool dashHeld { get; private set; }
    public bool dashReleased { get; private set; }
    public bool attackTrigger { get; private set; }
    public bool blockTrigger { get; private set; }


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
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        interactAction = playerControls.FindActionMap(actionMapName).FindAction(interact);
        dashAction = playerControls.FindActionMap(actionMapName).FindAction(dash);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);
        blockAction = playerControls.FindActionMap(actionMapName).FindAction(block);

        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        //Skok przenioslem do update

        interactAction.performed += context => interactTrigger = true;
        interactAction.canceled += context => interactTrigger = false;

        attackAction.performed += context => attackTrigger = true;
        attackAction.canceled += context => attackTrigger = false;

        blockAction.performed += context => blockTrigger = true;
        blockAction.canceled += context => blockTrigger = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
        dashAction.Enable();
        attackAction.Enable();
        blockAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
        dashAction.Disable();
        attackAction.Disable();
        blockAction.Disable();
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

        if (dashAction != null)
        {
            dashPressed = dashAction.WasPerformedThisFrame();
            dashHeld = dashAction.IsPressed();
            dashReleased = dashAction.WasReleasedThisFrame();
        }
        else
        {
            dashPressed = false;
            dashHeld = false;
            dashReleased = false;
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
