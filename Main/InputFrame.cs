using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Main
{
    public enum Command
    {
        Up,
        Right,
        Down,
        Left,
        Confirm,
        Cancel,
        Menu,
        Interact,
        Run
    }

    public enum PlayerNumber
    {
        PlayerOne,
        PlayerTwo
    }

    public class InputFrame
    {
        private class CommandState
        {
            public List<Buttons> buttonBindings;
            public List<Keys> keyBindings;

            public bool down;
            public bool previouslyDown;
            public bool pressed;
            public bool released;
        }

        private const int INPUT_ACTIVITY_HISTORY_LENGTH = 40;

        private static Dictionary<Command, List<Keys>> MANDATORY_KEYBOARD_BINDINGS = new Dictionary<Command, List<Keys>>()
        {
            { Command.Up, new List<Keys>() { Keys.Up, Keys.W } },
            { Command.Right, new List<Keys>() { Keys.Right, Keys.D } },
            { Command.Down, new List<Keys>() { Keys.Down, Keys.S } },
            { Command.Left, new List<Keys>() { Keys.Left, Keys.A } },
            { Command.Confirm, new List<Keys>() { Keys.Enter, Keys.Space } },
            { Command.Cancel, new List<Keys>() { Keys.Escape } },
            { Command.Menu, new List<Keys>() { Keys.Escape } },
            { Command.Interact, new List<Keys>() { Keys.Enter, Keys.Space } }
        };

        private static Dictionary<Command, List<Buttons>> MANDATORY_GAMEPAD_BINDINGS = new Dictionary<Command, List<Buttons>>()
        {
            { Command.Up, new List<Buttons>() { Buttons.DPadUp } },
            { Command.Right, new List<Buttons>() { Buttons.DPadRight } },
            { Command.Down, new List<Buttons>() { Buttons.DPadDown } },
            { Command.Left, new List<Buttons>() { Buttons.DPadLeft } },
            { Command.Confirm, new List<Buttons>() { Buttons.A } },
            { Command.Cancel, new List<Buttons>() { Buttons.B } },
            { Command.Menu, new List<Buttons>() { Buttons.B } },
            { Command.Interact, new List<Buttons>() { Buttons.A } }
        };

        private List<float> keyActivity = new List<float>();
        private KeyboardState newKeyState;
        private KeyboardState oldKeyState;

        private List<float> buttonActivity = new List<float>();
        private GamePadState newGamePadState;
        private GamePadState oldGamePadState;

        private float axisX;
        private float axisY;
        private CommandState[] commandStates = new CommandState[Enum.GetNames(typeof(Command)).Length];

        public void ApplySettings()
        {
            foreach (Command command in Enum.GetValues(typeof(Command)))
            {
                CommandState commandState = new CommandState();
                commandState.keyBindings = new List<Keys>();
                commandState.buttonBindings = new List<Buttons>();

                Keys keyBinding;
                List<Keys> mandatoryKeyList;
                if (Settings.KeyboardBindings.TryGetValue(command, out keyBinding)) commandState.keyBindings.Add(keyBinding);
                if (MANDATORY_KEYBOARD_BINDINGS.TryGetValue(command, out mandatoryKeyList))
                {
                    foreach (Keys key in mandatoryKeyList)
                    {
                        if (!commandState.keyBindings.Contains(key)) commandState.keyBindings.Add(key);
                    }
                }

                Buttons buttonBinding;
                List<Buttons> mandatoryButtonList;
                if (Settings.GamePadBindings.TryGetValue(command, out buttonBinding)) commandState.buttonBindings.Add(buttonBinding);
                if (MANDATORY_GAMEPAD_BINDINGS.TryGetValue(command, out mandatoryButtonList))
                {
                    foreach (Buttons button in mandatoryButtonList)
                    {
                        if (!commandState.buttonBindings.Contains(button)) commandState.buttonBindings.Add(button);
                    }
                }

                commandStates[(int)command] = commandState;
            }
        }

        public void Update(GameTime gameTime)
        {
            axisX = axisY = 0.0f;
            foreach (CommandState commandState in commandStates)
            {
                commandState.previouslyDown = commandState.down;
                commandState.down = commandState.pressed = commandState.released = false;
            }

            oldKeyState = newKeyState;
            newKeyState = Keyboard.GetState();

            float keyPresses = 0;
            float buttonPresses = 0;

            bool gamePadEnabled = GamePad.GetCapabilities(PlayerIndex.One).IsConnected;
            if (gamePadEnabled)
            {
                string padName = GamePad.GetCapabilities(PlayerIndex.One).DisplayName;

                oldGamePadState = newGamePadState;
                newGamePadState = GamePad.GetState(PlayerIndex.One);

                axisX = newGamePadState.ThumbSticks.Left.X;
                axisY = newGamePadState.ThumbSticks.Left.Y;

                if (axisX < -Input.FULL_THUMBSTICK_THRESHOLD) commandStates[(int)Command.Left].down = true;
                if (axisX > Input.FULL_THUMBSTICK_THRESHOLD) commandStates[(int)Command.Right].down = true;
                if (axisY < -Input.FULL_THUMBSTICK_THRESHOLD) commandStates[(int)Command.Down].down = true;
                if (axisY > Input.FULL_THUMBSTICK_THRESHOLD) commandStates[(int)Command.Up].down = true;

                buttonPresses += (Math.Abs(axisX) + Math.Abs(axisY)) / 2.0f;
            }

            foreach (CommandState commandState in commandStates)
            {
                foreach (Keys key in commandState.keyBindings)
                {
                    if (newKeyState.IsKeyDown(key))
                    {
                        Input.MOUSE_MODE = false;
                        commandState.down = true;
                        keyPresses++;
                        break;
                    }
                }

                if (gamePadEnabled && !commandState.down)
                {
                    foreach (Buttons button in commandState.buttonBindings)
                    {
                        if (newGamePadState.IsButtonDown(button))
                        {
                            Input.MOUSE_MODE = false;
                            commandState.down = true;
                            buttonPresses++;
                            break;
                        }
                    }
                }

                if (commandState.down && !commandState.previouslyDown) commandState.pressed = true;
                else if (!commandState.down && commandState.previouslyDown) commandState.released = true;
            }

            keyActivity.Add(keyPresses);
            if (keyActivity.Count > INPUT_ACTIVITY_HISTORY_LENGTH) keyActivity.RemoveAt(0);

            buttonActivity.Add(buttonPresses);
            if (buttonActivity.Count > INPUT_ACTIVITY_HISTORY_LENGTH) buttonActivity.RemoveAt(0);
        }

        public bool CommandDown(Command command)
        {
            return commandStates[(int)command].down;
        }

        public bool CommandPressed(Command command)
        {
            return commandStates[(int)command].pressed;
        }

        public bool CommandReleased(Command command)
        {
            return commandStates[(int)command].released;
        }

        public bool AnythingPressed()
        {
            if (newKeyState.GetPressedKeys().Length > 0) return true;

            foreach (Buttons buttons in Enum.GetValues(typeof(Buttons)))
            {
                if (newGamePadState.IsButtonDown(buttons)) return true;
            }

            if (Input.LeftMouseClicked || Input.RightMouseClicked)
                return true;

            return false;
        }

        public Keys KeyBinding(Command command)
        {
            return commandStates[(int)command].keyBindings[0];
        }

        public Buttons ButtonBinding(Command command)
        {
            return commandStates[(int)command].buttonBindings[0];
        }

        public Keys GetKey()
        {
            for (Keys key = Keys.D0; key <= Keys.D9; key++)
            {
                if (oldKeyState.IsKeyUp(key) && newKeyState.IsKeyDown(key)) return key;
            }

            for (Keys key = Keys.A; key <= Keys.Z; key++)
            {
                if (oldKeyState.IsKeyUp(key) && newKeyState.IsKeyDown(key)) return key;
            }

            return Keys.None;
        }

        public bool KeyDown(Keys key)
        {
            return newKeyState.IsKeyDown(key);
        }

        public bool KeyPressed(Keys key)
        {
            return newKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
        }

        public bool KeyReleased(Keys key)
        {
            return newKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
        }

        public float AxisX { get => axisX; }
        public float AxisY { get => axisY; }
    }
}
