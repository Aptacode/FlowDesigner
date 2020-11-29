using System;
using System.ComponentModel;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public static class Constants
    {
        public static readonly float Tolerance = 0.01f;
    }

    public class ConnectedItem : BindableBase
    {
        private Vector2 _anchorPoint;

        private Vector2 _anchorPointDelta;

        private Vector2 _connectionPointSize;

        public ConnectedItem(ItemViewModel item, ConnectionMode mode)
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;
            Mode = mode;
            ConnectionPointSize = new Vector2(1, 1);
            UpdateAnchorPointDelta(Item.Position);
        }

        public ItemViewModel Item { get; set; }
        public ConnectionMode Mode { get; set; }

        public Vector2 ConnectionPointSize
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
            Vector2 tempAnchorPoint;

            var topIntersect = GetIntersection(Item.TopLeft, Item.TopRight, Item.MidPoint, mousePosition);
            if (topIntersect.X >= Item.TopLeft.X && topIntersect.X <= Item.TopRight.X)
            {
                tempAnchorPoint = mousePosition.Y <= Item.MidPoint.Y
                    ? topIntersect
                    : GetIntersection(Item.BottomLeft, Item.BottomRight, Item.MidPoint, mousePosition);
            }
            else
            {
                var leftIntersect = GetIntersection(Item.TopLeft, Item.BottomLeft, Item.MidPoint, mousePosition);
                tempAnchorPoint = mousePosition.X <= Item.MidPoint.X
                    ? leftIntersect
                    : GetIntersection(Item.TopRight, Item.BottomRight, Item.MidPoint, mousePosition);
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
            var c = -m * start.X + start.Y;
            return (m, c);
        }

        public float GetX((float m, float c) equation, Vector2 point)
        {
            var (m, c) = equation;
            if (float.IsPositiveInfinity(m))
            {
                return c;
            }

            return (c - point.Y) * m;
        }

        public float GetY((float m, float c) equation, Vector2 point)
        {
            var (m, c) = equation;
            if (float.IsPositiveInfinity(m))
            {
                return point.Y;
            }

            return m * point.X + c;
        }

        public Vector2 GetIntersection(Vector2 edgeStart, Vector2 edgeEnd, Vector2 itemCenter, Vector2 mousePosition)
        {
            var (m, c) = ToLineEquation(edgeStart, edgeEnd);
            var mouseLine = ToLineEquation(itemCenter, mousePosition);

            //Vertical Edge
            var xIntersect = float.IsPositiveInfinity(m) ? c : mousePosition.X;

            //Vertical mouse line
            var yIntersect = float.IsPositiveInfinity(mouseLine.m) ? edgeStart.Y : mousePosition.Y;

            xIntersect = Math.Clamp(xIntersect, Item.Position.X, Item.Position.X + Item.Size.X);
            yIntersect = Math.Clamp(yIntersect, Item.Position.Y, Item.Position.Y + Item.Size.Y);

            return new Vector2(xIntersect, yIntersect);
        }

        public bool CollidesWith(Vector2 position) =>
            position.X >= AnchorPoint.X - ConnectionPointSize.X &&
            position.X <= AnchorPoint.X + ConnectionPointSize.X &&
            position.Y >= AnchorPoint.Y - ConnectionPointSize.Y && position.Y <= AnchorPoint.Y + ConnectionPointSize.Y;

        public Vector2 GetOffset()
        {
            const float largeOffset = 2.0f;
            const float smallOffset = 2.0f;

            if (AnchorPoint == Item.TopLeft)
            {
                return AnchorPoint + new Vector2(-smallOffset, -smallOffset);
            }

            if (AnchorPoint == Item.TopRight)
            {
                return AnchorPoint + new Vector2(smallOffset, -smallOffset);
            }

            if (AnchorPoint == Item.BottomRight)
            {
                return AnchorPoint + new Vector2(smallOffset, smallOffset);
            }

            if (AnchorPoint == Item.BottomLeft)
            {
                return AnchorPoint + new Vector2(-smallOffset, smallOffset);
            }

            if (Math.Abs(AnchorPoint.Y - Item.TopLeft.Y) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(0, -largeOffset);
            }

            if (Math.Abs(AnchorPoint.Y - Item.BottomRight.Y) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(0, largeOffset);
            }

            if (Math.Abs(AnchorPoint.X - Item.TopLeft.X) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(-largeOffset, 0);
            }

            if (Math.Abs(AnchorPoint.X - Item.BottomRight.X) < Constants.Tolerance)
            {
                return AnchorPoint + new Vector2(largeOffset, 0);
            }

            return Vector2.Zero;
        }
    }
}