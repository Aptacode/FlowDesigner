using System.Linq;
using System.Net;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.AppFramework.Scene;
using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;
using Aptacode.PathFinder.Maps.Hpa;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionViewModel : PolylineViewModel
    {
        #region Ctor

        protected ConnectionViewModel(HierachicalMap map, ConnectionPointViewModel connectionPoint1, ConnectionPointViewModel connectionPoint2) : base(new PolyLine(VertexArray.Create(new[]
        {
            connectionPoint1.Ellipse.BoundingCircle.Center,
            connectionPoint2.Ellipse.BoundingCircle.Center
        })))
        {
            Map = map;
            ConnectionPoint1 = connectionPoint1;
            ConnectionPoint2 = connectionPoint2;
            CollisionDetectionEnabled = false;
            RecalculatePath();
        }

        #endregion

        public static ConnectionViewModel Connect(HierachicalMap map, ConnectionPointViewModel connectionPoint1, ConnectionPointViewModel connectionPoint2)
        {
            var connection = new ConnectionViewModel(map, connectionPoint1, connectionPoint2);
            connectionPoint1.Connections.Add(connection);
            connectionPoint2.Connections.Add(connection);
            return connection;
        }

        public void RecalculatePath()
        {
            var points = Map.FindPath(ConnectionPoint1.Ellipse.Position, ConnectionPoint2.Ellipse.Position, 1);

            var path = points.ToList();

            path.Insert(0, ConnectionPoint1.Ellipse.Position);
            path.Add(ConnectionPoint2.Ellipse.Position);

            PolyLine = new PolyLine(VertexArray.Create(path.ToArray()));
            UpdateBoundingRectangle();
            Invalidated = true;
        }

        #region Prop

        public HierachicalMap Map { get; set; }
        public ConnectionPointViewModel ConnectionPoint1 { get; set; }
        public ConnectionPointViewModel ConnectionPoint2 { get; set; }

        public PolyLine Connection => PolyLine;

        #endregion
    }
}