using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionViewModel : PolylineViewModel
    {
        #region Ctor

        protected ConnectionViewModel(SceneViewModel scene, ConnectionPointViewModel connectionPoint1, ConnectionPointViewModel connectionPoint2) : base(new PolyLine(VertexArray.Create(new[]
        {
            connectionPoint1.Ellipse.BoundingCircle.Center,
            connectionPoint2.Ellipse.BoundingCircle.Center
        })))
        {
            Scene = scene;
            ConnectionPoint1 = connectionPoint1;
            ConnectionPoint2 = connectionPoint2;
            CollisionDetectionEnabled = false;
            RecalculatePath();
        }

        #endregion

        public static ConnectionViewModel Connect(SceneViewModel scene, ConnectionPointViewModel connectionPoint1, ConnectionPointViewModel connectionPoint2)
        {
            var connection = new ConnectionViewModel(scene, connectionPoint1, connectionPoint2);
            connectionPoint1.Connections.Add(connection);
            connectionPoint2.Connections.Add(connection);
            return connection;
        }

        public void RecalculatePath()
        {
            var path = Scene.GetPath(ConnectionPoint1.GetConnectionPoint(), ConnectionPoint2.GetConnectionPoint());

            path.Insert(0, ConnectionPoint2.Ellipse.Position);
            path.Add(ConnectionPoint1.Ellipse.Position);

            PolyLine = new PolyLine(VertexArray.Create(path.ToArray()));
            UpdateBoundingRectangle();
            Invalidated = true;
        }

        #region Prop

        public SceneViewModel Scene { get; set; }
        public ConnectionPointViewModel ConnectionPoint1 { get; set; }
        public ConnectionPointViewModel ConnectionPoint2 { get; set; }

        public PolyLine Connection => PolyLine;

        #endregion
    }
}