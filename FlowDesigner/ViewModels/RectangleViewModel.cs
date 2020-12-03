using System;
using System.Drawing;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public abstract class RectangleViewModel : BindableBase
    {
        private Vector2 _position;

        private Vector2 _size;
        private Color _borderColor = Color.Black;

        private int _z;
        private bool _isShown;
        private float _borderThickness;
        private float _margin;
        private float _edgeThickness;

        protected RectangleViewModel(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
            Z = 10;
            IsShown = false;
            _borderThickness = 1;
            _margin = 2;
            _edgeThickness = 1;
        }

        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }
        
        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public Vector2 Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public Color BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public float BorderThickness
        {
            get => _borderThickness;
            set => SetProperty(ref _borderThickness, value);
        }      
        
        public float Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        public Vector2 MidPoint => Position + Size / 2;

        public Vector2 TopLeft => Position;
        public Vector2 TopRight => Position + Size * new Vector2(1, 0);
        public Vector2 BottomRight => Position + Size;
        public Vector2 BottomLeft => Position + Size * new Vector2(0, 1);

        public bool CollidesWith(Vector2 position) =>
            position.X >= Position.X && position.X <= Position.X + Size.X &&
            position.Y >= Position.Y && position.Y <= Position.Y + Size.Y;

        public bool CollidesWithEdge(Vector2 position) =>
            CollidesWith(position) &&
            (position.X <= Position.X + 1 || position.X >= Position.X + Size.X - 1)
            &&
            (position.Y <= Position.Y + 1 || position.Y >= Position.Y + Size.Y - 1);

        public bool CollidesWith(RectangleViewModel rectangle)
        {
            return Position.X < rectangle.TopRight.X &&
               TopRight.X > rectangle.TopLeft.X &&
               Position.Y < rectangle.BottomRight.Y &&
               BottomRight.Y > rectangle.Position.Y;
        }

        public bool CollidesWith(RectangleViewModel rectangle, Vector2 perimeter)
        {
            return Position.X < rectangle.TopRight.X + perimeter.X &&
               TopRight.X > rectangle.TopLeft.X - perimeter.X &&
               Position.Y < rectangle.BottomRight.Y + perimeter.Y &&
               BottomRight.Y > rectangle.Position.Y - perimeter.Y;
        }
    }
}