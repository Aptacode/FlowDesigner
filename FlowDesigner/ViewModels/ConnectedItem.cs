using System;
using System.ComponentModel;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ConnectedItem : BindableBase
    {
        private Vector2 _anchorPoint;

        private Vector2 _anchorPointDelta;

        private Vector2 _connectionPointSize;

        public ConnectedItem(ItemViewModel item, ConnectionMode mode, int anchorPoint)
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

        public float AnchorMagnitude { get; set; }
        public float AnchorPhase { get; set; }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemViewModel.Position))
            {
                AnchorPoint = Item.MidPoint - AnchorPointDelta;
            }

            if (e.PropertyName == nameof(ItemViewModel.Size))
            {
                //UpdateAnchorPointDelta(AnchorPoint);
            }
        }

        public float Determinant(Vector4 vector) => vector.X * vector.W - vector.Z * vector.Y;

        public Vector4 Inverse(Vector4 vector) =>
            new Vector4(vector.W, -vector.Y, -vector.Z, vector.X) * (1.0f / Determinant(vector));

        public Vector2 Mul(Vector4 vector, Vector2 position) =>
            new Vector2(vector.X * position.X + vector.Y * position.Y, vector.Z * position.X + vector.W * position.Y);

        public void UpdateAnchorPointDelta(Vector2 mousePosition)
        {
            var tempAnchorPoint = AnchorPoint;

            var topIntersect = GetIntersection(Item.TopLeft, Item.TopRight, Item.MidPoint, mousePosition);
            if (topIntersect.X >= Item.TopLeft.X && topIntersect.X <= Item.TopRight.X)
            {
                if (mousePosition.Y <= Item.MidPoint.Y)
                {
                    tempAnchorPoint = topIntersect;
                }
                else
                {
                    tempAnchorPoint = GetIntersection(Item.BottomLeft, Item.BottomRight, Item.MidPoint, mousePosition);
                }
            }
            else
            {
                var leftIntersect = GetIntersection(Item.TopLeft, Item.BottomLeft, Item.MidPoint, mousePosition);
                if (mousePosition.X <= Item.MidPoint.X)
                {
                    tempAnchorPoint = leftIntersect;
                }
                else
                {
                    tempAnchorPoint = GetIntersection(Item.TopRight, Item.BottomRight, Item.MidPoint, mousePosition);
                }
            }

            AnchorPointDelta = Item.MidPoint - tempAnchorPoint;
            AnchorPoint = tempAnchorPoint;
        }


        public (float m, float c) ToLineEquation(Vector2 start, Vector2 end)
        {
            if (end.X == start.X)
            {
                return (float.PositiveInfinity, start.X);
            }

            var m = (end.Y - start.Y) / (end.X - start.X);
            var c = -m * start.X + start.Y;
            return (m, c);
        }

        public float GetX((float m, float c) equation, Vector2 point)
        {
            if (equation.m == float.PositiveInfinity)
            {
                return equation.c;
            }

            return (equation.c - point.Y) * equation.m;
        }

        public float GetY((float m, float c) equation, Vector2 point)
        {
            if (equation.m == float.PositiveInfinity)
            {
                return point.Y;
            }

            return equation.m * point.X + equation.c;
        }

        public Vector2 GetIntersection(Vector2 edgeStart, Vector2 edgeEnd, Vector2 itemCenter, Vector2 mousePosition)
        {
            var edge = ToLineEquation(edgeStart, edgeEnd);
            var mouseLine = ToLineEquation(itemCenter, mousePosition);

            float xIntersect;

            //Vertical Edge
            if (edge.m == float.PositiveInfinity)
            {
                xIntersect = edge.c;
            }
            //Horizontal Edge
            else
            {
                xIntersect = mousePosition.X;
            }


            float yIntersect;
            //Vertical mouse line
            if (mouseLine.m == float.PositiveInfinity)
            {
                yIntersect = edgeStart.Y;
            }
            //Horizontal mouse line
            else
            {
                yIntersect = mousePosition.Y;
            }

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
            var largeOffset = 2.0f;
            var smallOffset = 1.0f;
            var AnchorOffset = Vector2.Zero;

            if (AnchorPoint == Item.TopLeft)
            {
                AnchorOffset = AnchorPoint + new Vector2(-smallOffset, -smallOffset);
            }
            else if (AnchorPoint == Item.TopRight)
            {
                AnchorOffset = AnchorPoint + new Vector2(smallOffset, -smallOffset);
            }
            else if (AnchorPoint == Item.BottomRight)
            {
                AnchorOffset = AnchorPoint + new Vector2(smallOffset, smallOffset);
            }
            else if (AnchorPoint == Item.BottomLeft)
            {
                AnchorOffset = AnchorPoint + new Vector2(-smallOffset, smallOffset);
            }
            else if (AnchorPoint.Y == Item.TopLeft.Y)
            {
                AnchorOffset = AnchorPoint + new Vector2(0, -largeOffset);
            }
            else if (AnchorPoint.Y == Item.BottomRight.Y)
            {
                AnchorOffset = AnchorPoint + new Vector2(0, largeOffset);
            }
            else if (AnchorPoint.X == Item.TopLeft.X)
            {
                AnchorOffset = AnchorPoint + new Vector2(-largeOffset, 0);
            }
            else if (AnchorPoint.X == Item.BottomRight.X)
            {
                AnchorOffset = AnchorPoint + new Vector2(largeOffset, 0);
            }

            return AnchorOffset;
        }
    }
}