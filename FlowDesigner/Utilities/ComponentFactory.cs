using System;
using System.Numerics;
using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;

namespace Aptacode.FlowDesigner.Core.Utilities
{
    public static class ComponentFactory
    {
        public static ConnectedComponentViewModel CreateConnectedComponent(this DesignerViewModel designer, string name,
            Vector2 position, Vector2 size)
        {
            var connectedComponent = new ConnectedComponentViewModel(Guid.NewGuid(), name, position, size);
            connectedComponent.AddTo(designer);
            return connectedComponent;
        }

        public static PointViewModel CreatePoint(this DesignerViewModel designer, Vector2 position)
        {
            var newPoint = new PointViewModel(Guid.NewGuid(), position);
            newPoint.AddTo(designer);
            return newPoint;
        }

        public static ConnectionPointViewModel CreateConnectionPoint(this DesignerViewModel designer,
            ConnectedComponentViewModel connectedComponent, Vector2 direction)
        {
            var newConnectionPoint = new ConnectionPointViewModel(Guid.NewGuid(), connectedComponent, direction);
            newConnectionPoint.AddTo(designer);
            return newConnectionPoint;
        }

        public static ConnectionPointViewModel CreateConnectionPoint(this DesignerViewModel designer,
            ConnectedComponentViewModel connectedComponent)
        {
            var newConnectionPoint = new ConnectionPointViewModel(Guid.NewGuid(), connectedComponent);
            newConnectionPoint.AddTo(designer);
            return newConnectionPoint;
        }

        public static ConnectionViewModel CreateConnection(this DesignerViewModel designer,
            ConnectionPointViewModel point1, ConnectionPointViewModel point2)
        {
            var newConnection = new ConnectionViewModel(designer, point1, point2);
            newConnection.AddTo(designer);
            return newConnection;
        }
    }
}