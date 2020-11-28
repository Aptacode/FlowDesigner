using Aptacode.CSharp.Common.Utilities.Mvvm;
using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectedItem : BindableBase
    {
        public ConnectedItem(ItemViewModel item, ConnectionMode mode, int anchorPoint)
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;
            AnchorPoint = anchorPoint;
            Mode = mode;
            ConnectionPoint = GetConnectionPoint();
            ConnectionPointSize = new Vector2(5, 5);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ItemViewModel.Position))
            {
                ConnectionPoint = GetConnectionPoint();
            }
        }

        public ItemViewModel Item { get; set; }
        public ConnectionMode Mode { get; set; }

        private Vector2 _connectionPoint;

        public Vector2 ConnectionPoint
        {
            get => _connectionPoint;
            set
            {
                SetProperty(ref _connectionPoint, value);
            }
        }

        private Vector2 _connectionPointSize;

        public Vector2 ConnectionPointSize
        {
            get => _connectionPointSize;
            set
            {
                SetProperty(ref _connectionPointSize, value);
            }
        }

        private int _anchorPoint;

        public int AnchorPoint
        {
            get => _anchorPoint;
            set
            {
                SetProperty(ref _anchorPoint, value);
                ConnectionPoint = GetConnectionPoint();
            }
        }

        public Vector2 GetConnectionPoint()
        {
            //Top
            if (AnchorPoint <= Item.Size.X)
            {
                var adjustedAnchorPoint = AnchorPoint;
                return new Vector2(Item.Position.X + adjustedAnchorPoint, Item.Position.Y);
            }

            //Right
            if (AnchorPoint <= Item.Size.X + Item.Size.Y)
            {
                var adjustedAnchorPoint = AnchorPoint - Item.Size.X;
                return new Vector2(Item.Position.X + Item.Size.X, Item.Position.Y + adjustedAnchorPoint);
            }

            //Bottom
            if (AnchorPoint <= Item.Size.X * 2 + Item.Size.Y)
            {
                var adjustedAnchorPoint = AnchorPoint - Item.Size.X - Item.Size.Y;
                return new Vector2(Item.Position.X + Item.Size.X - adjustedAnchorPoint, Item.Position.Y + Item.Size.Y);
            }
            //Left
            else
            {
                var adjustedAnchorPoint = AnchorPoint - Item.Size.X - Item.Size.Y - Item.Size.X ;
                return new Vector2(Item.Position.X, Item.Position.Y + Item.Size.Y - adjustedAnchorPoint);
            }


        }

        public int ToAnchorPoint(Vector2 point)
        {
            var halfSize = Item.Size / 2;
            var midPoint = point + halfSize;
            var delta = midPoint - point;
            var topLeft = Item.Position;
            var bottomRight = Item.Position + Item.Size;

            //   1  2  3 
            //    \ | /
            // 8 -     - 4
            //    / | \
            //   7  6  5

            //Top
            if(point.Y <= Item.Position.Y)
            {
                var deltaX = Math.Clamp(Vector2.Abs(point - topLeft).X, 0, Item.Size.X);
                return (int)(deltaX);
            }
            //Bottom
            else if(point.Y >= bottomRight.Y)
            {
                var deltaX = Math.Clamp(Vector2.Abs(point - bottomRight).X, 0, Item.Size.X);
                return (int)(Item.Size.X + Item.Size.Y + deltaX);
            }
            //Middle
            else
            {
                //Left
                if(point.X <= midPoint.X)
                {
                    var deltaY = Math.Clamp(Vector2.Abs(point - Item.Position).Y, 0, Item.Size.Y);
                    return (int)(Item.Size.X + Item.Size.Y + Item.Size.X + Item.Size.Y - deltaY);
                }
                //Right
                else
                {
                    var deltaY = Math.Clamp(Vector2.Abs(point - Item.Position).Y, 0, Item.Size.Y);
                    return (int)(Item.Size.X + deltaY);
                }
            }
        }

        public bool CollidesWith(Vector2 position) =>
            position.X >= ConnectionPoint.X && position.X <= ConnectionPoint.X + ConnectionPointSize.X &&
            position.Y >= ConnectionPoint.Y && position.Y <= ConnectionPoint.Y + ConnectionPointSize.Y;
    }
}