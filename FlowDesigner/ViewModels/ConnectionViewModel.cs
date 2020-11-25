using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public enum ConnectionMode
    {
        In, Out, Both
    }

    public class ConnectedItem
    {
        public ConnectedItem(ItemViewModel item, ConnectionMode mode, int anchorPoint)
        {
            Item = item;
            AnchorPoint = anchorPoint;
            Mode = mode;
        }

        public ItemViewModel Item { get; set; }
        public int AnchorPoint { get; set; }
        public ConnectionMode Mode { get; set; }

        public (int X, int Y) GetConnectionPoint()
        {
            if(AnchorPoint <= Item.Width)
            {
                return (Item.X + AnchorPoint, Item.Y);
            }

            if (AnchorPoint <= Item.Width + Item.Height)
            {
                return (Item.X + Item.Width, Item.Y + AnchorPoint - Item.Width);
            }

            if (AnchorPoint <= Item.Width * 2 + Item.Height)
            {
                var xOffset = AnchorPoint - 2 * Item.Width - Item.Height;
                return (Item.X + Item.Width - xOffset, Item.Y);
            }

            var yOffset = AnchorPoint - 2 * Item.Width - 2 * Item.Height;
            return (Item.X, Item.Y + yOffset);
        }
    }

    public class ConnectionViewModel : BindableBase
    {
        public ConnectionViewModel(Guid id, string label, DesignerViewModel designer, ItemViewModel item1, ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out, 0);
            Item2 = new ConnectedItem(item2, ConnectionMode.In, item1.AnchorPointCount / 2);
            Designer = designer;

            Item1.Item.PropertyChanged += Item_PropertyChanged;
            Item2.Item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) 
        { 
            if(e.PropertyName == "X" || e.PropertyName == "Y")
            {
                //Refresh();
            }
        }

        public void Refresh()
        {
            new TaskFactory().StartNew(() =>
            {
                var startPoint = Item1.GetConnectionPoint();
                var endPoint = Item2.GetConnectionPoint();

                var obstacles = new List<Obstacle>();
                foreach (var item in Designer.Items.ToList())
                {
                   obstacles.Add(new Obstacle(item.Id, new Vector2(item.X, item.Y), new Vector2(item.Width, item.Height)));
                }

                var map = new Map(Designer.Width, Designer.Height, new Vector2(startPoint.X - 1, startPoint.Y - 1), new Vector2(endPoint.X + 1, endPoint.Y + 1), obstacles.ToArray());
                var pathFinder = new PathFinder.PathFinder();

                var path = new StringBuilder();

                path.Append("M ");
                foreach (var point in pathFinder.FindPath(map))
                {
                    path.Append(point.X * DesignerViewModel.ScaleFactor).Append(' ').Append(point.Y * DesignerViewModel.ScaleFactor);
                    path.Append("L ");
                }

                SetProperty(ref _path, path.ToString());
            });
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
