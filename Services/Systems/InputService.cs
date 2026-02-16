using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack.Services.Systems;

public class InputService : IUpdatable
{
    private readonly Dictionary<InputAction, List<ActionBinding>> _actionBindings = new();

    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;
    private MouseState _currentMouse;
    private MouseState _previousMouse;
    private bool _initialized;

    public KeyboardState CurrentKeyboard => _currentKeyboard;
    public MouseState CurrentMouse => _currentMouse;

    public Point MousePosition => new Point(_currentMouse.X, _currentMouse.Y);
    public Point MouseDelta => new Point(_currentMouse.X - _previousMouse.X, _currentMouse.Y - _previousMouse.Y);
    public int ScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;

    public bool IsKeyDown(Keys key) => _currentKeyboard.IsKeyDown(key);
    public bool IsKeyUp(Keys key) => _currentKeyboard.IsKeyUp(key);
    public bool IsKeyPressed(Keys key) => _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    public bool IsKeyReleased(Keys key) => !_currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);

    public bool IsMouseDown(MouseButton button) => GetMouseButtonState(_currentMouse, button);
    public bool IsMousePressed(MouseButton button) => GetMouseButtonState(_currentMouse, button) && !GetMouseButtonState(_previousMouse, button);
    public bool IsMouseReleased(MouseButton button) => !GetMouseButtonState(_currentMouse, button) && GetMouseButtonState(_previousMouse, button);

    public void BindAction(InputAction action, InputTrigger trigger, params Keys[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        var bindings = new List<ActionBinding>(keys.Length);
        foreach (var key in keys)
        {
            bindings.Add(ActionBinding.ForKey(key, trigger));
        }

        _actionBindings[action] = bindings;
    }

    public void BindAction(InputAction action, InputTrigger trigger, params MouseButton[] mouseButtons)
    {
        ArgumentNullException.ThrowIfNull(mouseButtons);
        var bindings = new List<ActionBinding>(mouseButtons.Length);
        foreach (var button in mouseButtons)
        {
            bindings.Add(ActionBinding.ForMouse(button, trigger));
        }

        _actionBindings[action] = bindings;
    }

    public void BindAction(InputAction action, params Keys[] keys)
    {
        BindAction(action, InputTrigger.Down, keys);
    }

    public bool IsAction(InputAction action)
    {
        if (!_actionBindings.TryGetValue(action, out var bindings) || bindings.Count == 0)
        {
            return false;
        }

        foreach (var binding in bindings)
        {
            if (Matches(binding))
            {
                return true;
            }
        }

        return false;
    }

    public void Update(GameTime gameTime)
    {
        if (!_initialized)
        {
            _currentKeyboard = Keyboard.GetState();
            _previousKeyboard = _currentKeyboard;
            _currentMouse = Mouse.GetState();
            _previousMouse = _currentMouse;
            _initialized = true;
            return;
        }

        _previousKeyboard = _currentKeyboard;
        _previousMouse = _currentMouse;
        _currentKeyboard = Keyboard.GetState();
        _currentMouse = Mouse.GetState();
    }

    public void WarpMouse(Point position, bool resetDelta = true)
    {
        Mouse.SetPosition(position.X, position.Y);
        if (!resetDelta) return;

        var state = Mouse.GetState();
        _currentMouse = state;
        _previousMouse = state;
    }

    public InputService()
    {
        BindAction(InputAction.Exit, InputTrigger.Down, Keys.Escape);
        BindAction(InputAction.ToggleDebugPanel, InputTrigger.Pressed, Keys.OemTilde);
        BindAction(InputAction.MoveForward, InputTrigger.Down, Keys.W);
        BindAction(InputAction.MoveBackward, InputTrigger.Down, Keys.S);
        BindAction(InputAction.MoveLeft, InputTrigger.Down, Keys.A);
        BindAction(InputAction.MoveRight, InputTrigger.Down, Keys.D);
        BindAction(InputAction.Run, InputTrigger.Down, Keys.LeftShift, Keys.RightShift);
        BindAction(InputAction.PrimaryClick, InputTrigger.Pressed, MouseButton.Left);
        BindAction(InputAction.Interact, InputTrigger.Pressed, Keys.E);
    }

    private bool Matches(ActionBinding binding)
    {
        return binding.Device switch
        {
            ActionBindingDevice.Keyboard => binding.Trigger switch
            {
                InputTrigger.Down => IsKeyDown(binding.Key),
                InputTrigger.Pressed => IsKeyPressed(binding.Key),
                InputTrigger.Released => IsKeyReleased(binding.Key),
                _ => false
            },
            ActionBindingDevice.Mouse => binding.Trigger switch
            {
                InputTrigger.Down => IsMouseDown(binding.MouseButton),
                InputTrigger.Pressed => IsMousePressed(binding.MouseButton),
                InputTrigger.Released => IsMouseReleased(binding.MouseButton),
                _ => false
            },
            _ => false
        };
    }

    private static bool GetMouseButtonState(MouseState state, MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => state.LeftButton == ButtonState.Pressed,
            MouseButton.Middle => state.MiddleButton == ButtonState.Pressed,
            MouseButton.Right => state.RightButton == ButtonState.Pressed,
            _ => false
        };
    }

    private readonly record struct ActionBinding(ActionBindingDevice Device, InputTrigger Trigger, Keys Key, MouseButton MouseButton)
    {
        public static ActionBinding ForKey(Keys key, InputTrigger trigger) => new(ActionBindingDevice.Keyboard, trigger, key, MouseButton.Left);
        public static ActionBinding ForMouse(MouseButton button, InputTrigger trigger) => new(ActionBindingDevice.Mouse, trigger, Keys.None, button);
    }

    private enum ActionBindingDevice
    {
        Keyboard,
        Mouse
    }
}

public enum InputTrigger
{
    Down,
    Pressed,
    Released
}

public enum MouseButton
{
    Left,
    Middle,
    Right
}
