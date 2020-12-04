using System;
using System.Drawing;
using System.Numerics;
using Aptacode.CSharp.Common.Utilities.Mvvm;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class PointViewModel : BindableBase
    {
        private Vector2 _position;

        private Color _borderColor = Color.Black;

        private int _z;
        private bool _isShown;
        private float _borderThickness;
        private float _margin;
        private bool _collisionsAllowed;
        private Guid _id;

        public PointViewModel(Guid id, Vector2 position)
        {
            Position = position;
            Id = id;
            Z = 10;
            IsShown = false;
            _borderThickness = 0.3f;
            _margin = 2;
            CollisionsAllowed = true;
        }

        public bool IsShown
        {
            get => _isShown;
            set => SetProperty(ref _isShown, value);
        }

        public bool CollisionsAllowed
        {
            get => _collisionsAllowed;
            set => SetProperty(ref _collisionsAllowed, value);
        }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
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

        public bool CollidesWith(Vector2 position) =>
            CollisionsAllowed &&
            position.X == Position.X &&
            position.Y == Position.Y;

        public bool CollidesWith(RectangleViewModel rectangle)
        {
            return CollisionsAllowed && rectangle.CollidesWith(this.Position);
        }
    }
}