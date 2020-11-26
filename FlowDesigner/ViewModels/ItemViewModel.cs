using Aptacode.CSharp.Common.Utilities.Mvvm;
using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ItemViewModel : BindableBase
    {
        public ItemViewModel(Guid id, string label, Vector2 position, Vector2 size)
        {
            Id = id;
            Label = label;
            Position = position;
            Size = size;
            Z = 10;
        }

        public Guid Id { get; set; }
        public string Label { get; set; }

        private int _z;

        public int Z
        {
            get { return _z; }
            set { SetProperty(ref _z, value); }
        }

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        private Vector2 _size;

        public Vector2 Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public int AnchorPointCount => (int)(2 * (Size.X + Size.Y));

        public bool CollidesWith(Vector2 position)
        {
            return position.X >= Position.X && position.X <= Position.X + Size.X &&
               position.Y >= Position.Y && position.Y <= Position.Y + Size.Y;
        }
    }
}
