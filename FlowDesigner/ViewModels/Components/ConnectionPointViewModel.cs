using System.Collections.Generic;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionPointViewModel : EllipseViewModel
    {
        #region Ctor

        public ConnectionPointViewModel(ConnectedComponentViewModel component, Ellipse ellipse) : base(ellipse)
        {
            this.Component = component;
            Margin = 1;
            Connections = new List<ConnectionViewModel>();
        }
        #endregion

        #region Prop
        
        public ConnectedComponentViewModel Component { get; set; }
        public List<ConnectionViewModel> Connections { get; set; }

        #endregion

        public override void Translate(Vector2 delta)
        {
            base.Translate(delta);
            foreach (var connectionViewModel in Connections)
            {
                connectionViewModel.Calculate();
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
    }
}