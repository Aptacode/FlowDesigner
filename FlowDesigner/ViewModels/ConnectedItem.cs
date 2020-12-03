using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{

    public class ConnectedItem : BindableBase
    {
        private Vector2 _anchorPoint;

        private Vector2 _anchorPointDelta;

        private float _connectionPointSize;

        public ConnectedItem(ItemViewModel item, ConnectionMode mode)
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;
            Mode = mode;
            ConnectionPointSize = 0.5f;
            UpdateAnchorPointDelta(new Vector2(Item.TopRight.X, Item.MidPoint.Y));
        }

        public ItemViewModel Item { get; set; }
        public ConnectionMode Mode { get; set; }

        public float ConnectionPointSize
        {
            get => _connectionPointSize;
            set => SetProperty(ref _connectionPointSize, value);
        }

        public Vector2 AnchorPoint
        {
            get => _anchorPoint;
            set => SetProperty(ref _anchorPoint, value);
        }

        public Vector2 AnchorPointDelta
        {
            get => _anchorPointDelta;
            set => SetProperty(ref _anchorPointDelta, value);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ItemViewModel.Position):
                    AnchorPoint = Item.MidPoint - AnchorPointDelta;
                    break;
                case nameof(ItemViewModel.Size):
                    UpdateAnchorPointDelta(AnchorPoint - AnchorPointDelta);
                    break;
            }
        }

        public void UpdateAnchorPointDelta(Vector2 mousePosition)
        {
            var tempAnchorPoint = AnchorPoint;

            if(mousePosition.Y <= Item.TopLeft.Y && mousePosition.X >= Item.TopLeft.X && mousePosition.X <= Item.TopRight.X) 
            {
                tempAnchorPoint = GetIntersection(Item.TopLeft, Item.TopRight, Item.MidPoint, mousePosition);
            }else if(mousePosition.X >= Item.TopRight.X && mousePosition.Y >= Item.TopRight.Y && mousePosition.Y <= Item.BottomRight.Y)
            {
                tempAnchorPoint = GetIntersection(Item.TopRight, Item.BottomRight, Item.MidPoint, mousePosition);

            }
            else if(mousePosition.Y >= Item.BottomRight.Y && mousePosition.X >= Item.TopLeft.X && mousePosition.X <= Item.TopRight.X)
            {
                tempAnchorPoint = GetIntersection(Item.BottomRight, Item.BottomLeft, Item.MidPoint, mousePosition);
            }else if(mousePosition.X <= Item.TopLeft.X && mousePosition.Y >= Item.TopRight.Y && mousePosition.Y <= Item.BottomRight.Y)
            {
                tempAnchorPoint = GetIntersection(Item.BottomLeft, Item.TopLeft, Item.MidPoint, mousePosition);
            }

            AnchorPointDelta = Item.MidPoint - tempAnchorPoint;
            AnchorPoint = tempAnchorPoint;
     
        }

        public (float m, float c) ToLineEquation(Vector2 start, Vector2 end)
        {
            if (Math.Abs(end.X - start.X) < Constants.Tolerance)
            {
                return (float.PositiveInfinity, start.X);
            }

            var m = (end.Y - start.Y) / (end.X - start.X);
            var c = ((-m) * start.X) + start.Y;
            return (m, c);
        }

        public Vector2 GetIntersection(Vector2 edgeStart, Vector2 edgeEnd, Vector2 itemCenter, Vector2 mousePosition)
        {
            var (edgeM, edgeC) = ToLineEquation(edgeStart, edgeEnd);
            var (mouseM, mouseC) = ToLineEquation(itemCenter, mousePosition);

            //Vertical Edge
            var xIntersect = float.IsPositiveInfinity(edgeM) ? edgeC : mousePosition.X;

            //Vertical mouse line
            var yIntersect = float.IsPositiveInfinity(mouseM) ? edgeStart.Y : mousePosition.Y;

            var minX = Item.Position.X;
            var minY = Item.Position.Y;
            var maxX = Item.Position.X + Item.Size.X;
            var maxY = Item.Position.Y + Item.Size.Y;

            if (xIntersect <= minX && yIntersect <= minY)
            {
                xIntersect = minX + 1;
                yIntersect = minY;
            }else if (xIntersect >= maxX && yIntersect <= minY)
            {
                xIntersect = maxX;
                yIntersect = minY + 1;
            }else if (xIntersect >= maxX && yIntersect >= maxY)
            {
                xIntersect = maxX - 1;
                yIntersect = maxY;
            }else if (xIntersect <= minX && yIntersect >= maxY)
            {
                xIntersect = minX;
                yIntersect = maxY - 1;
            }
            else
            {
                xIntersect = Math.Clamp(xIntersect, Item.Position.X, Item.Position.X + Item.Size.X);
                yIntersect = Math.Clamp(yIntersect, Item.Position.Y, Item.Position.Y + Item.Size.Y);
            }

            return new Vector2(xIntersect, yIntersect);
        }

        public bool CollidesWith(Vector2 position) =>
            position.X >= AnchorPoint.X - ConnectionPointSize &&
            position.X <= AnchorPoint.X + ConnectionPointSize &&
            position.Y >= AnchorPoint.Y - ConnectionPointSize && position.Y <= AnchorPoint.Y + ConnectionPointSize;

        public Vector2 GetOffset()
        {
            if (AnchorPoint == Item.TopLeft)
            {
                return AnchorPoint + new Vector2(-Item.Margin, -Item.Margin);
            }

            if (AnchorPoint == Item.TopRight)
            {
                return AnchorPoint + new Vector2(Item.Margin, -Item.Margin);
            }

            if (AnchorPoint == Item.BottomRight)
            {
                return AnchorPoint + new Vector2(Item.Margin, Item.Margin);
            }

            if (AnchorPoint == Item.BottomLeft)
            {
                return AnchorPoint + new Vector2(-Item.Margin, Item.Margin);
            }

            if (Math.Abs(AnchorPoint.Y - Item.TopLeft.Y) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(0, -Item.Margin);
            }

            if (Math.Abs(AnchorPoint.Y - Item.BottomRight.Y) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(0, Item.Margin);
            }

            if (Math.Abs(AnchorPoint.X - Item.TopLeft.X) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(-Item.Margin, 0);
            }

            if (Math.Abs(AnchorPoint.X - Item.BottomRight.X) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(Item.Margin, 0);
            }

            return Vector2.Zero;
        }
    }
}