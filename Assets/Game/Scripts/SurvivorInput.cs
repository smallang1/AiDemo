using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Nightfall
{
    public static class SurvivorInput
    {
        public static Vector2 Movement
        {
            get
            {
                Vector2 legacy = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical"));

                if (legacy.sqrMagnitude > 0.01f)
                {
                    return legacy.normalized;
                }

#if ENABLE_INPUT_SYSTEM
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    return Vector2.zero;
                }

                float x = 0f;
                float y = 0f;
                if (keyboard[Key.A].isPressed || keyboard[Key.LeftArrow].isPressed)
                {
                    x -= 1f;
                }

                if (keyboard[Key.D].isPressed || keyboard[Key.RightArrow].isPressed)
                {
                    x += 1f;
                }

                if (keyboard[Key.S].isPressed || keyboard[Key.DownArrow].isPressed)
                {
                    y -= 1f;
                }

                if (keyboard[Key.W].isPressed || keyboard[Key.UpArrow].isPressed)
                {
                    y += 1f;
                }

                Vector2 modern = new Vector2(x, y);
                return modern.sqrMagnitude > 1f ? modern.normalized : modern;
#else
                return Vector2.zero;
#endif
            }
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            if (Input.GetKeyDown(keyCode))
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            Key? key = ToInputKey(keyCode);
            return key.HasValue &&
                   Keyboard.current != null &&
                   Keyboard.current[key.Value].wasPressedThisFrame;
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static Key? ToInputKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.W:
                    return Key.W;
                case KeyCode.A:
                    return Key.A;
                case KeyCode.S:
                    return Key.S;
                case KeyCode.D:
                    return Key.D;
                case KeyCode.UpArrow:
                    return Key.UpArrow;
                case KeyCode.DownArrow:
                    return Key.DownArrow;
                case KeyCode.LeftArrow:
                    return Key.LeftArrow;
                case KeyCode.RightArrow:
                    return Key.RightArrow;
                case KeyCode.Space:
                    return Key.Space;
                case KeyCode.Return:
                    return Key.Enter;
                case KeyCode.KeypadEnter:
                    return Key.NumpadEnter;
                case KeyCode.Escape:
                    return Key.Escape;
                case KeyCode.P:
                    return Key.P;
                case KeyCode.R:
                    return Key.R;
                case KeyCode.Alpha1:
                    return Key.Digit1;
                case KeyCode.Alpha2:
                    return Key.Digit2;
                case KeyCode.Alpha3:
                    return Key.Digit3;
                case KeyCode.Keypad1:
                    return Key.Numpad1;
                case KeyCode.Keypad2:
                    return Key.Numpad2;
                case KeyCode.Keypad3:
                    return Key.Numpad3;
                case KeyCode.F8:
                    return Key.F8;
                case KeyCode.F9:
                    return Key.F9;
                default:
                    return null;
            }
        }
#endif
    }
}
