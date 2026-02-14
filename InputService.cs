using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

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

    public void BindAction(InputAction action, InputTrigger trigger, params Keys[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        var bindings = new List<ActionBinding>(keys.Length);
        foreach (var key in keys)
        {
            bindings.Add(new ActionBinding(key, trigger));
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
    }

    private bool Matches(ActionBinding binding)
    {
        return binding.Trigger switch
        {
            InputTrigger.Down => IsKeyDown(binding.Key),
            InputTrigger.Pressed => IsKeyPressed(binding.Key),
            InputTrigger.Released => IsKeyReleased(binding.Key),
            _ => false
        };
    }

    private readonly record struct ActionBinding(Keys Key, InputTrigger Trigger);
}

public enum InputTrigger
{
    Down,
    Pressed,
    Released
}
