using System;
using System.Collections.Generic;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectedComponentViewModel : PolygonViewModel
    {
        #region Ctor
        public ConnectedComponentViewModel(Rectangle body) : base(body)
        {
            Body = body;
            Margin = 1;
            ConnectionPoints = new List<ConnectionPointViewModel>();
        }
        #endregion

        #region Prop

        public Rectangle Body { get; private set; }
        public List<ConnectionPointViewModel> ConnectionPoints { get; private set; }
        
        #endregion

        public ConnectionPointViewModel AddConnectionPoint()
        {
            var ellipse = new Ellipse(Body.TopLeft + new Vector2(4,0), new Vector2(1, 1), 0);
            var connectionPoint = new ConnectionPointViewModel(this, ellipse);
            ConnectionPoints.Add(connectionPoint);
            Add(connectionPoint);
            return connectionPoint;
        }
    }
}
