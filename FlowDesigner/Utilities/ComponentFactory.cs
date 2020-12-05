using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Aptacode.FlowDesigner.Core.Utilities
{
    public static class ComponentFactory
    {
        public static ConnectedComponentViewModel CreateConnectedComponent(this DesignerViewModel designer, string name, Vector2 position, Vector2 size)
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
        
        public static ConnectionPointViewModel CreateConnectionPoint(this DesignerViewModel designer, ConnectedComponentViewModel connectedComponent)
        {
            var newConnectionPoint = new ConnectionPointViewModel(Guid.NewGuid(), connectedComponent);
            connectedComponent.ConnectionPoints.Add(newConnectionPoint);
            newConnectionPoint.AddTo(designer);
            return newConnectionPoint;
        }   
        
        public static ConnectionViewModel CreateConnection(this DesignerViewModel designer, ConnectionPointViewModel point1, ConnectionPointViewModel point2)
        {
            var newConnection = new ConnectionViewModel(designer, point1, point2);
            designer.Connections.Add(newConnection);
            point1.Connections.Add(newConnection);
            point2.Connections.Add(newConnection);

            return newConnection;
        }

    }
}
