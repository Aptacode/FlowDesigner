using System;
using System.Collections.Generic;
using System.Numerics;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionPointViewModel : EllipseViewModel
    {
        #region Ctor

        public ConnectionPointViewModel(ConnectedComponentViewModel component, Ellipse ellipse) : base(ellipse)
        {
            Component = component;
            Margin = 1;
            Connections = new List<ConnectionViewModel>();
        }

        #endregion

        public override void Translate(Vector2 delta)
        {
            base.Translate(delta);
            RecalculatePaths();
        }

        public void RecalculatePaths()
        {
            foreach (var connectionViewModel in Connections)
            {
                connectionViewModel.RecalculatePath();
            }
        }

        public Vector2 GetConnectionPoint()
        {
            const int buffer = 2;

            Vector2 delta;

            if (Ellipse.Position.Y <= Component.Body.TopLeft.Y)
            {
                delta = new Vector2(0, -Ellipse.Radii.Y - buffer);
            }
            else if (Ellipse.Position.Y >= Component.Body.BottomRight.Y)
            {
                delta = new Vector2(0, Ellipse.Radii.Y + buffer);
            }
            else if (Ellipse.Position.X >= Component.Body.BottomRight.X)
            {
                delta = new Vector2(Ellipse.Radii.X + buffer, 0);
            }
            else
            {
                delta = new Vector2(-Ellipse.Radii.X - buffer, 0);
            }

            return Ellipse.Position + delta;
        }

        public void Move(Vector2 mousePos)
        {
            Vector2 newPos;

            if (mousePos.Y <= Component.Body.TopLeft.Y)
            {
                newPos = new Vector2(Math.Clamp(mousePos.X, Component.Body.TopLeft.X, Component.Body.TopRight.X), Component.Body.TopLeft.Y);
            }
            else if (mousePos.Y >= Component.Body.BottomRight.Y)
            {
                newPos = new Vector2(Math.Clamp(mousePos.X, Component.Body.TopLeft.X, Component.Body.TopRight.X), Component.Body.BottomLeft.Y);
            }
            else if (mousePos.X >= Component.Body.BottomRight.X)
            {
                newPos = new Vector2(Component.Body.BottomRight.X, Math.Clamp(mousePos.Y, Component.Body.TopLeft.Y, Component.Body.BottomLeft.Y));
            }
            else
            {
                newPos = new Vector2(Component.Body.BottomLeft.X, Math.Clamp(mousePos.Y, Component.Body.TopLeft.Y, Component.Body.BottomLeft.Y));
            }

            Ellipse = new Ellipse(newPos, Ellipse.Radii, Ellipse.Rotation);
            RecalculatePaths();
        }

        #region Prop

        public ConnectedComponentViewModel Component { get; set; }
        public List<ConnectionViewModel> Connections { get; set; }

        #endregion
    }
}