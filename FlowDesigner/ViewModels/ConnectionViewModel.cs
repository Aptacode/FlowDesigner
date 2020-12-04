using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectionViewModel : BindableBase
    {
        private int _z;
        private Color _borderColor = Color.Black;

        public ConnectionViewModel(Guid id, string label, DesignerViewModel designer, ItemViewModel item1,
            ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out);
            Item2 = new ConnectedItem(item2, ConnectionMode.In);
            Path = new PathViewModel();
            Z = 10;
            Thickness = 3;
            Designer = designer;
        }

        public Guid Id { get; set; }
        public string Label { get; set; }

        public PathViewModel Path { get; set; }
        public ConnectedItem Item1 { get; set; }
        public ConnectedItem Item2 { get; set; }
        public DesignerViewModel Designer { get; set; }
        public float Thickness { get; set; }

        public void Redraw()
        {
            Path.ClearPoints();

            try
            {       
                var mapBuilder = new MapBuilder();

                foreach (var item in Designer.Items.ToList())
                {
                    mapBuilder.AddObstacle(item.Position - Vector2.One, item.Size + (Vector2.One * 2));
                }

                mapBuilder.SetStart(Item1.GetOffset());
                mapBuilder.SetEnd(Item2.GetOffset());
                mapBuilder.SetDimensions(Designer.Width, Designer.Height);
                var mapResult = mapBuilder.Build();
                if(!mapResult.Success)
                {
                    throw new Exception(mapResult.Message);
                }
                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(mapResult.Map, DefaultNeighbourFinder.Straight(0.5f));

                var points = new List<Vector2>();
                points.Add(Item2.AnchorPoint);
                points.AddRange(pathFinder.FindPath());
                points.Add(Item1.AnchorPoint);

                Path.AddPoints(points);
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        internal bool IsConnectedTo(ItemViewModel item) => Item1.Item == item || Item2.Item == item;
    }
}