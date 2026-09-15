using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeadbangHeroes.Input
{
    public sealed class HeadbangInput : MonoBehaviour
    {
        public event Action<float> Bang;

        void Update()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                    Emit(touch.position.ReadValue().x);
            }

#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Emit(Mouse.current.position.ReadValue().x);
#endif
        }

        void Emit(float x) => Bang?.Invoke(x < Screen.width * 0.5f ? -1f : 1f);
    }
}
