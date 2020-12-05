using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.FlowDesigner.Core.Enums;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectionViewModel : BaseComponentViewModel
    {
        public ConnectionViewModel(
            Guid id,
            DesignerViewModel designer,
            ConnectionPointViewModel point1,
            ConnectionPointViewModel point2) : base(id)
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

            try
            {
                var mapBuilder = new MapBuilder();

                foreach (var item in Designer.Items.ToList())
                {
                    mapBuilder.AddObstacle(item.Position - Vector2.One, item.Size + Vector2.One * 2);
                }

                mapBuilder.SetStart(Point1.GetOffset());
                mapBuilder.SetEnd(Point2.GetOffset());
                mapBuilder.SetDimensions(Designer.Width, Designer.Height);
                var mapResult = mapBuilder.Build();
                if (!mapResult.Success)
                {
                    throw new Exception(mapResult.Message);
                }

                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(mapResult.Map, DefaultNeighbourFinder.Straight(0.5f));

                var points = new List<Vector2>();
                points.Add(Point2.AnchorPoint);
                points.AddRange(pathFinder.FindPath());
                points.Add(Point1.AnchorPoint);

                Path.AddPoints(points);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool IsConnectedTo(ConnectedComponentViewModel item) => Point1.Item == item || Point2.Item == item;

        public void Break()
        {
            Point1.Connections.Remove(this);
            Point2.Connections.Remove(this);
        }
    }
}