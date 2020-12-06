#nullable enable
using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class UserInteractionManager
    {
        #region State

        public bool ConnectionPointSelected { get; set; }
        public Vector2 LastMousePosition { get; set; }
        public Vector2 MouseDownPosition { get; set; }

        #endregion

        #region Interaction

        #region Mouse

        public void MouseDown(Vector2 position)
        {
            MouseDownPosition = position;

            if (IsPressed("d"))
            {
                DeleteAt?.Invoke(this, position);
                KeyPressed = null;
            }
            else if (IsPressed("c"))
            {
                CreateAt?.Invoke(this, position);
            }
            else if (IsPressed("p"))
            {
                AddPoint?.Invoke(this, position);
            }
            else
            {
                SelectAt?.Invoke(this, position);
            }

            LastMousePosition = position;
        }

        public void MouseUp(Vector2 position)
        {
            MouseReleased?.Invoke(this, position);
            LastMousePosition = position;
        }

        public void MouseMove(Vector2 position)
        {
            MouseMoved?.Invoke(this, position);
            LastMousePosition = position;
        }

        #endregion

        #region Keyboard

        public string? KeyPressed;
        public bool ControlPressed => KeyPressed == "Control";
        public bool IsPressed(string key) => string.Equals(KeyPressed, key, StringComparison.OrdinalIgnoreCase);
        public bool NothingPressed => string.IsNullOrEmpty(KeyPressed);

        public void KeyDown(string key)
        {
            KeyPressed = key;
        }

        public void KeyUp(string key)
        {
            if (ControlPressed)
            {
                KeyPressed = null;
            }

            KeyPressed = null;
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<Vector2> CreateAt;
        public event EventHandler<Vector2> DeleteAt;
        public event EventHandler<Vector2> SelectAt;
        public event EventHandler<Vector2> AddPoint;
        public event EventHandler<Vector2> MouseMoved;
        public event EventHandler<Vector2> MouseReleased;

        #endregion
    }
}