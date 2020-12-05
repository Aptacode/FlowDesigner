using System;
using System.Drawing;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Enums;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectionViewModel : BindableBase
    {
        public ConnectionViewModel(
            DesignerViewModel designer,
            ConnectionPointViewModel point1,
            ConnectionPointViewModel point2)
        {
            Point1 = point1;
            Point2 = point2;

            Path = new PathViewModel();
            Designer = designer;
        }

        public PathViewModel Path { get; set; }
        public ConnectionPointViewModel Point1 { get; set; }
        public ConnectionPointViewModel Point2 { get; set; }
        public ConnectionMode ModeA { get; set; }
        public ConnectionMode ModeB { get; set; }
        public DesignerViewModel Designer { get; set; }

        public void Redraw()
        {
            Path.ClearPoints();
            var startPoint = Point1.GetOffset(Point1.Item.Margin);
            var endPoint = Point2.GetOffset(Point2.Item.Margin);
            var path = Designer.GetPath(startPoint, endPoint);
            Path.AddPoint(Point2.GetOffset(Point2.ConnectionPointSize));
            Path.AddPoints(path);
            Path.AddPoint(Point1.GetOffset(Point2.ConnectionPointSize));
        }

        public bool IsConnectedTo(ConnectedComponentViewModel item) => Point1.Item == item || Point2.Item == item;

        public void Break()
        {
            Point1.Connections.Remove(this);
            Point2.Connections.Remove(this);
        }

        public void Deselect()
        {
            Point1.BorderColor = Color.Black;
            Point2.BorderColor = Color.Black;
            Path.BorderColor = Color.Black;
        }

        public void Select()
        {
            Point1.BorderColor = Color.Green;
            Point2.BorderColor = Color.Green;
            Path.BorderColor = Color.Green;
        }        
        
        public void BringToFront(DesignerViewModel designer)
        {
            designer.BringToFront(Point1);
            designer.BringToFront(Point2);
            designer.BringToFront(Path);
        }
    }
}