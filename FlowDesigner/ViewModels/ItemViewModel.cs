using Aptacode.CSharp.Common.Utilities.Mvvm;
using System;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ItemViewModel : BindableBase
    {
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public ItemViewModel(Guid id, string label, int x, int y, int width, int height)
        {
            Id = id;
            Label = label;

            X = x;
            Y = y;
            Width = width;
            Height = height;
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

        private int _x;

        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        public int AnchorPointCount => (2 * Width) + (2 * Height);

        public bool CollidesWith((int x, int y) point)
        {
            return point.x >= X && point.x <= X + Width &&
               point.y >= Y && point.y <= Y + Height;
        }
    }
}
