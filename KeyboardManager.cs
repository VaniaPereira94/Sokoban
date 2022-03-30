using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Sokoban
{
    class KeyboardManager
    {
        private static KeyboardManager instance;
        enum KeyState
        {
            PRESSED,
            HELD,
            UP,
            NONE
        }

        Dictionary<Keys, KeyState> keysAndState;

        public KeyboardManager()
        {

            if (instance == null)
            {
                keysAndState = new Dictionary<Keys, KeyState>();
                instance = this;

            }

            else throw new Exception("Instance already created!");
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();
            Keys[] pressedKeys = state.GetPressedKeys();

            foreach (Keys k in pressedKeys)
            {
                if (!keysAndState.ContainsKey(k))
                    keysAndState.Add(k, KeyState.PRESSED);
                else
                {

                    if (keysAndState[k] == KeyState.PRESSED)
                        keysAndState[k] = KeyState.HELD;

                    else if(keysAndState[k] == KeyState.UP || keysAndState[k] == KeyState.NONE)
                        keysAndState[k] = KeyState.PRESSED;



                }
            }

            foreach (Keys k in keysAndState.Keys.ToArray())
            {
                if (!pressedKeys.Contains(k))
                {
                    if (keysAndState[k] == KeyState.UP)
                        keysAndState[k] = KeyState.NONE;

                    else if (keysAndState[k] == KeyState.PRESSED || keysAndState[k] == KeyState.HELD)
                        keysAndState[k] = KeyState.UP;

                }
            }
        }


        public bool IsKeyPressed(Keys k) => keysAndState.ContainsKey(k) && keysAndState[k] == KeyState.PRESSED;
        public bool IsKeyUp(Keys k) => keysAndState.ContainsKey(k) && keysAndState[k] == KeyState.UP;

        bool isKeyHeld(Keys k) => keysAndState.ContainsKey(k) && keysAndState[k] == KeyState.HELD;

    }
}
