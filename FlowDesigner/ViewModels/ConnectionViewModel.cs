using Aptacode.CSharp.Common.Utilities.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

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
        public ConnectionViewModel(Guid id, string label, ItemViewModel item1, ItemViewModel item2)
        {
            Id = id;
            Label = label;
            Item1 = new ConnectedItem(item1, ConnectionMode.Out, 3);
            Item2 = new ConnectedItem(item2, ConnectionMode.In, item1.AnchorPointCount / 2);

            Console.WriteLine(item2.AnchorPointCount);

            Item1.Item.PropertyChanged += Item_PropertyChanged;
            Item2.Item.PropertyChanged += Item_PropertyChanged;
            Refresh();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) 
        { 
            if(e.PropertyName == "X" || e.PropertyName == "Y")
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            var newPath = new List<(int X, int Y)>();

            var startPoint = Item1.GetConnectionPoint();
            var endPoint = Item2.GetConnectionPoint();

            newPath.Add(startPoint);
            newPath.Add(endPoint);


            SetProperty(ref _path, newPath);
        }


        public Guid Id { get; set; }
        public string Label { get; set; }

        public ConnectedItem Item1 { get; set; }
        public ConnectedItem Item2 { get; set; }

        private List<(int X, int Y)> _path;

        public IEnumerable<(int X, int Y)> Path
        {
            get { return _path; }
        }

        public string GetPath()
        {
            var newPath = new List<(int X, int Y)>();
            var startPoint = Item1.GetConnectionPoint();
            var endPoint = Item2.GetConnectionPoint();

            var path = new StringBuilder();

            path.Append("M ");
            path.Append($"{startPoint.X * DesignerViewModel.ScaleFactor} {startPoint.Y * DesignerViewModel.ScaleFactor}");
            path.Append("L ");
            path.Append($"{endPoint.X * DesignerViewModel.ScaleFactor} {endPoint.Y * DesignerViewModel.ScaleFactor}");

            return path.ToString();
        }

        internal bool IsConnectedTo(ItemViewModel item) => Item1.Item == item || Item2.Item == item;
    }
}
