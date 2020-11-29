using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Extensions;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectionViewModel : BindableBase
    {
        private readonly MapBuilder _mapBuilder = new MapBuilder();
        private string _path;

        public ConnectionViewModel(Guid id, string label, DesignerViewModel designer, ItemViewModel item1,
            ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out);
            Item2 = new ConnectedItem(item2, ConnectionMode.In);

            Designer = designer;

            Item1.PropertyChanged += Item1_PropertyChanged;
            Item2.PropertyChanged += Item1_PropertyChanged;
        }

        public Guid Id { get; set; }
        public string Label { get; set; }

        public ConnectedItem Item1 { get; set; }
        public ConnectedItem Item2 { get; set; }
        public DesignerViewModel Designer { get; set; }

        public string Path => _path;
        public IEnumerable<Vector2> ConnectionPath { get; private set; }

        private void Item1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(ConnectedItem.AnchorPoint))
            //{
            //    Redraw();
            //}
        }

        public void Redraw()
        {
            try
            {
                foreach (var item in Designer.Items.ToList())
                {
                    _mapBuilder.AddObstacle(item.Position, item.Size);
                }

                _mapBuilder.SetStart(Item1.GetOffset());
                _mapBuilder.SetEnd(Item2.GetOffset());
                _mapBuilder.SetDimensions(Designer.Width, Designer.Height);
                var map = _mapBuilder.Build();
                var pathFinder =
                    new PathFinder.Algorithm.PathFinder(map, JumpPointSearchNeighbourFinder.All(1.0f, 1.9f));
                ConnectionPath = pathFinder.FindPath();
                var path = new StringBuilder();

                path.Add(Item2.AnchorPoint);
                foreach (var point in ConnectionPath)
                {
                    path.Add(point);
                }

                path.Add(Item1.AnchorPoint);
                SetProperty(ref _path, path.ToString());
            }
            catch { }
        }

        internal bool IsConnectedTo(ItemViewModel item) => Item1.Item == item || Item2.Item == item;
    }
}