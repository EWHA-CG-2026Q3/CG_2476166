using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Mid_MoonAnchor : MonoBehaviour
{
    void Update()
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame) pressed = true;
#else
        if (Input.GetKeyDown(KeyCode.M)) pressed = true;
#endif
        if (pressed && Camera.main != null)
        {
            transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        }
    }
}