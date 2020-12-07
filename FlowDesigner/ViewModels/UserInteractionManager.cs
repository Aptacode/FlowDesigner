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
        public DateTime FirstMouseDownTime { get; set; }
        public DateTime SecondMouseDownTime { get; set; }

        #endregion

        #region Interaction

        #region Mouse

        public void MouseClickDown()
        {
            if (DateTime.Now - FirstMouseDownTime > TimeSpan.FromMilliseconds(300))
            {
                FirstMouseDownTime = DateTime.Now;
            }
            else
            {
                SecondMouseDownTime = DateTime.Now;
            }
        }
        public void MouseClickRelease(Vector2 position)
        {
            if (DateTime.Now - SecondMouseDownTime < TimeSpan.FromMilliseconds(150))
            {
                MouseDoubleClicked?.Invoke(this, position);
            }
            else if (DateTime.Now - FirstMouseDownTime < TimeSpan.FromMilliseconds(150))
            {
                MouseClicked?.Invoke(this, position);
            }
        }

        public void MouseDown(Vector2 position)
        {
            MouseDownPosition = position;
            MouseClickDown();

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

            MouseClickRelease(position);
        }

        public void MouseMove(Vector2 position)
        {
            MouseMoved?.Invoke(this, position);
            LastMousePosition = position;
        }

        #endregion

        #region Keyboard

        public string? CurrentKey;
        public bool ControlPressed => CurrentKey == "Control";
        public bool IsPressed(string key) => string.Equals(CurrentKey, key, StringComparison.OrdinalIgnoreCase);
        public bool NothingPressed => string.IsNullOrEmpty(CurrentKey);

        public void KeyDown(string key)
        {
            CurrentKey = key;
            KeyPressed?.Invoke(this, CurrentKey);
        }

        public void KeyUp(string key)
        {
            if (ControlPressed)
            {
                CurrentKey = null;
            }

            CurrentKey = null;
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
        public event EventHandler<Vector2> MouseClicked;
        public event EventHandler<Vector2> MouseDoubleClicked;
        public event EventHandler<string> KeyPressed;

        #endregion
    }
}