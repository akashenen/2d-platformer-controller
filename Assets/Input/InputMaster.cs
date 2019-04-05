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
    private bool m_Initialized;
    private void Initialize()
    {
        // Player
        m_Player = asset.GetActionMap("Player");
        m_Player_Movement = m_Player.GetAction("Movement");
        m_Player_Jump = m_Player.GetAction("Jump");
        m_Player_EndJump = m_Player.GetAction("EndJump");
        m_Player_Dash = m_Player.GetAction("Dash");
        m_Player_PickUp = m_Player.GetAction("Pick Up");
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
        m_Player_EndJump = null;
        m_Player_Dash = null;
        m_Player_PickUp = null;
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
    private InputAction m_Player_EndJump;
    private InputAction m_Player_Dash;
    private InputAction m_Player_PickUp;
    public struct PlayerActions
    {
        private InputMaster m_Wrapper;
        public PlayerActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement { get { return m_Wrapper.m_Player_Movement; } }
        public InputAction @Jump { get { return m_Wrapper.m_Player_Jump; } }
        public InputAction @EndJump { get { return m_Wrapper.m_Player_EndJump; } }
        public InputAction @Dash { get { return m_Wrapper.m_Player_Dash; } }
        public InputAction @PickUp { get { return m_Wrapper.m_Player_PickUp; } }
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
                EndJump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndJump;
                EndJump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndJump;
                EndJump.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndJump;
                Dash.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                Dash.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                Dash.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDash;
                PickUp.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPickUp;
                PickUp.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPickUp;
                PickUp.cancelled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPickUp;
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
                EndJump.started += instance.OnEndJump;
                EndJump.performed += instance.OnEndJump;
                EndJump.cancelled += instance.OnEndJump;
                Dash.started += instance.OnDash;
                Dash.performed += instance.OnDash;
                Dash.cancelled += instance.OnDash;
                PickUp.started += instance.OnPickUp;
                PickUp.performed += instance.OnPickUp;
                PickUp.cancelled += instance.OnPickUp;
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
    void OnEndJump(InputAction.CallbackContext context);
    void OnDash(InputAction.CallbackContext context);
    void OnPickUp(InputAction.CallbackContext context);
}
