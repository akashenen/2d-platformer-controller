// GENERATED AUTOMATICALLY FROM 'Assets/Input/InputMaster.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class InputMaster : InputActionAssetReference
{
    public InputMaster()
    {
    }
    public InputMaster(InputActionAsset asset)
        : base(asset)
    {
    }
    [NonSerialized] private bool m_Initialized;
    private void Initialize()
    {
        // Player
        m_Player = asset.GetActionMap("Player");
        m_Player_Movement = m_Player.GetAction("Movement");
        m_Player_Jump = m_Player.GetAction("Jump");
        m_Player_Dash = m_Player.GetAction("Dash");
        m_Player_Interact = m_Player.GetAction("Interact");
        m_Player_AttackA = m_Player.GetAction("Attack A");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        if (m_PlayerActionsCallbackInterface != null)
        {
            Player.SetCallbacks(null);
        }
        m_Player = null;
        m_Player_Movement = null;
        m_Player_Jump = null;
        m_Player_Dash = null;
        m_Player_Interact = null;
        m_Player_AttackA = null;
        m_Initialized = false;
    }
    public void SetAsset(InputActionAsset newAsset)
    {
        if (newAsset == asset) return;
        var PlayerCallbacks = m_PlayerActionsCallbackInterface;
        if (m_Initialized) Uninitialize();
        asset = newAsset;
        Player.SetCallbacks(PlayerCallbacks);
    }
    public override void MakePrivateCopyOfActions()
    {
        SetAsset(ScriptableObject.Instantiate(asset));
    }
    // Player
    private InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private InputAction m_Player_Movement;
    private InputAction m_Player_Jump;
    private InputAction m_Player_Dash;
    private InputAction m_Player_Interact;
    private InputAction m_Player_AttackA;
    public struct PlayerActions
    {
        private InputMaster m_Wrapper;
        public PlayerActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement { get { return m_Wrapper.m_Player_Movement; } }
        public InputAction @Jump { get { return m_Wrapper.m_Player_Jump; } }
        public InputAction @Dash { get { return m_Wrapper.m_Player_Dash; } }
        public InputAction @Interact { get { return m_Wrapper.m_Player_Interact; } }
        public InputAction @AttackA { get { return m_Wrapper.m_Player_AttackA; } }
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                Movement.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMovement;
                Movement.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMovement;
                Movement.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMovement;
                Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                Jump.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                Dash.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                Dash.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                Dash.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                Interact.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
                AttackA.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackA;
                AttackA.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackA;
                AttackA.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttackA;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                Movement.started += instance.OnMovement;
                Movement.performed += instance.OnMovement;
                Movement.cancelled += instance.OnMovement;
                Jump.started += instance.OnJump;
                Jump.performed += instance.OnJump;
                Jump.cancelled += instance.OnJump;
                Dash.started += instance.OnDash;
                Dash.performed += instance.OnDash;
                Dash.cancelled += instance.OnDash;
                Interact.started += instance.OnInteract;
                Interact.performed += instance.OnInteract;
                Interact.cancelled += instance.OnInteract;
                AttackA.started += instance.OnAttackA;
                AttackA.performed += instance.OnAttackA;
                AttackA.cancelled += instance.OnAttackA;
            }
        }
    }
    public PlayerActions @Player
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new PlayerActions(this);
        }
    }
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get

        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.GetControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get

        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.GetControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
}
public interface IPlayerActions
{
    void OnMovement(InputAction.CallbackContext context);
    void OnJump(InputAction.CallbackContext context);
    void OnDash(InputAction.CallbackContext context);
    void OnInteract(InputAction.CallbackContext context);
    void OnAttackA(InputAction.CallbackContext context);
}
