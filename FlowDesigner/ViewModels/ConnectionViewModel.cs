using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.PathFinder;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectionViewModel : BindableBase
    {
        private string _path;

        public ConnectionViewModel(Guid id, string label, DesignerViewModel designer, ItemViewModel item1,
            ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out, item1.AnchorPointCount / 2);
            Item2 = new ConnectedItem(item2, ConnectionMode.In, item2.AnchorPointCount / 2);
            Designer = designer;

            Item1.PropertyChanged += Item1_PropertyChanged;
            Item2.PropertyChanged += Item1_PropertyChanged;

        }

        private void Item1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Refresh();
        }

        public Guid Id { get; set; }
        public string Label { get; set; }

        public ConnectedItem Item1 { get; set; }
        public ConnectedItem Item2 { get; set; }
        public DesignerViewModel Designer { get; set; }

        public string Path => _path;

        public void Refresh()
        {
            var startPoint = Item1.GetConnectionPoint();
            var endPoint = Item2.GetConnectionPoint();

            var obstacles = new List<Obstacle>();
            foreach (var item in Designer.Items.ToList())
            {
                obstacles.Add(new Obstacle(item.Id, item.Position, item.Size));
            }

            var map = new Map(Designer.Width, Designer.Height, startPoint + Vector2.One, endPoint + Vector2.One,
                obstacles.ToArray());

            var path = new StringBuilder();

            path.Append("M ");

            path.Add(Item2.ConnectionPoint);

            foreach (var point in map.FindPath())
            {
                path.Add(point);
            }

            path.Add(Item1.ConnectionPoint);


            SetProperty(ref _path, path.ToString());
        }

        internal bool IsConnectedTo(ItemViewModel item) => Item1.Item == item || Item2.Item == item;
    }
}