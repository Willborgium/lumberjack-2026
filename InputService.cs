using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public class InputService : IUpdatable
{
    private readonly Dictionary<InputAction, List<Keys>> _actionBindings = new();

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

    public void BindAction(InputAction action, params Keys[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        _actionBindings[action] = [.. keys];
    }

    public bool IsActionDown(InputAction action) => AnyBoundKey(action, IsKeyDown);
    public bool IsActionPressed(InputAction action) => AnyBoundKey(action, IsKeyPressed);
    public bool IsActionReleased(InputAction action) => AnyBoundKey(action, IsKeyReleased);

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
        BindAction(InputAction.Exit, Keys.Escape);
        BindAction(InputAction.ToggleDebugPanel, Keys.OemTilde);
        BindAction(InputAction.MoveForward, Keys.W);
        BindAction(InputAction.MoveBackward, Keys.S);
        BindAction(InputAction.MoveLeft, Keys.A);
        BindAction(InputAction.MoveRight, Keys.D);
        BindAction(InputAction.Run, Keys.LeftShift, Keys.RightShift);
    }

    private bool AnyBoundKey(InputAction action, Func<Keys, bool> predicate)
    {
        if (!_actionBindings.TryGetValue(action, out var keys) || keys.Count == 0)
        {
            return false;
        }

        foreach (var key in keys)
        {
            if (predicate(key)) return true;
        }

        return false;
    }
}
