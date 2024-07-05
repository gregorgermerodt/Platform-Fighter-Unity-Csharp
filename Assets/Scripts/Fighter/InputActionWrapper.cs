using UnityEngine;
using UnityEngine.InputSystem;


public class InputActionWrapper
{
    public InputAction inputAction { get; private set; }

    public bool isStarted { get; private set; } = false;
    public bool isPerformed { get; private set; } = false;
    public bool isCanceled { get; private set; } = false;
    public bool isActive { get => isStarted || isPerformed; }
    public bool isNoAction { get => !isStarted && !isPerformed && !isCanceled; }

    public InputActionWrapper(InputAction inputAction)
    {
        this.inputAction = inputAction;
        inputAction.started += SetStarted;
        inputAction.performed += SetPerformed;
        inputAction.canceled += SetCanceled;
        //allActiveInputActioWrapper.Add(this);
    }

    ~InputActionWrapper()
    {
        inputAction.started -= SetStarted;
        inputAction.performed -= SetPerformed;
        inputAction.canceled -= SetCanceled;
        //allActiveInputActioWrapper.Remove(this);
    }

    public void ResetInputStates()
    {
        isStarted = false;
        isCanceled = false;
    }

    private void SetStarted(InputAction.CallbackContext callbackContext) {
        //Debug.Log(inputAction.name + " is started!");
        isStarted = true;
    }

    private void SetPerformed(InputAction.CallbackContext callbackContext) => isPerformed = true;
    private void SetCanceled(InputAction.CallbackContext callbackContext)
    {
        isPerformed = false;
        isCanceled = true;
    }


    //static void UpdateInputActionStates()
    //{
    //    foreach (var iaw in allActiveInputActioWrapper)
    //    {
    //        iaw.ResetInputStates();
    //    }
    //}
    //private static List<InputActionWrapper> allActiveInputActioWrapper;
}