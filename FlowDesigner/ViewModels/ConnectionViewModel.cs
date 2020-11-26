using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aptacode.FlowDesigner.Core.ViewModels
{

    public class ConnectionViewModel : BindableBase
    {
        public ConnectionViewModel(Guid id, string label, DesignerViewModel designer, ItemViewModel item1, ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out, 0);
            Item2 = new ConnectedItem(item2, ConnectionMode.In, item1.AnchorPointCount / 2);
            Designer = designer;
        }

        public void Refresh()
        {
            var startPoint = Item1.GetConnectionPoint();
            var endPoint = Item2.GetConnectionPoint();

            var obstacles = new List<Obstacle>();
            foreach (var item in Designer.Items.ToList())
            {
                obstacles.Add(new Obstacle(item.Id, item.Position, item.Size));
            }

            var map = new Map(Designer.Width, Designer.Height, startPoint - Vector2.One, endPoint + Vector2.One, obstacles.ToArray());
            var path = new StringBuilder();

            path.Append("M ");
            foreach (var point in map.FindPath())
            {
                path.Append(point.X * DesignerViewModel.ScaleFactor).Append(' ').Append(point.Y * DesignerViewModel.ScaleFactor);
                path.Append("L ");
            }

            SetProperty(ref _path, path.ToString());
        }


        public Guid Id { get; set; }
        public string Label { get; set; }

        public ConnectedItem Item1 { get; set; }
        public ConnectedItem Item2 { get; set; }
        public DesignerViewModel Designer { get; set; }

        private string _path;

        public string Path
        {
            get { return _path; }
        }

        internal bool IsConnectedTo(ItemViewModel item) => Item1.Item == item || Item2.Item == item;
    }
}
