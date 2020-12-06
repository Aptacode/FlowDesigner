using System.Drawing;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Enums;
using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.FlowDesigner.Core.ViewModels.Components;

namespace Aptacode.FlowDesigner.Core.ViewModels
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

        public void Deselect(DesignerViewModel designer)
        {
            Point1.BorderColor = Color.Black;
            Point2.BorderColor = Color.Black;
            Path.BorderColor = Color.Black;
        }

        public void Select(DesignerViewModel designer)
        {
            Point1.BorderColor = Color.Green;
            Point2.BorderColor = Color.Green;
            Path.BorderColor = Color.Green;
            BringToFront(designer);
        }

        public void BringToFront(DesignerViewModel designer)
        {
            designer.BringToFront(Point1);
            designer.BringToFront(Point2);
            designer.BringToFront(Path);
        }

        public virtual void AddTo(DesignerViewModel designer)
        {
            Point1.Connections.Add(this);
            Point2.Connections.Add(this);
            Path.AddTo(designer);
            designer.Add(this);
        }

        public virtual void RemoveFrom(DesignerViewModel designer)
        {
            Point1.Connections.Remove(this);
            Point2.Connections.Remove(this);
            Path.RemoveFrom(designer);
            designer.Remove(this);
        }
    }
}