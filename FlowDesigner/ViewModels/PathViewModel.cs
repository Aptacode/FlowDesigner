using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.CSharp.Common.Utilities.Mvvm;
using Aptacode.FlowDesigner.Core.Extensions;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class PathViewModel : BindableBase
    {
        private List<Vector2> _points = new List<Vector2>();

        private Color _borderColor = Color.Black;

        private int _z;
        private bool _isShown;
        private float _thickness;
        private float _margin;
        private bool _collisionsAllowed;
        private string _path;

        public PathViewModel() : this(new Vector2[0])
        {

        }

        public PathViewModel(IEnumerable<Vector2> points)
        {
            Z = 10;
            IsShown = false;
            _thickness = 0.3f;
            _margin = 2;
            CollisionsAllowed = true;
            AddPoints(points);
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


        public Color BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public float Thickness
        {
            get => _thickness;
            set => SetProperty(ref _thickness, value);
        }      
        
        public float Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }   
        
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public void AddPoint(Vector2 point)
        {
            _points.Add(point);
            Redraw();
        }
        public void AddPoints(IEnumerable<Vector2> points)
        {
            _points.AddRange(points);
            Redraw();
        }
        public void ClearPoints()
        {
            _points.Clear();
            Redraw();
        }

        public void Redraw()
        {
            var pathBuilder = new StringBuilder();
            foreach (var point in _points)
            {
                pathBuilder.Add(point);
            }
            Path = pathBuilder.ToString();
        }

        public bool CollidesWith(RectangleViewModel rectangle)
        {
            return _points.Any(rectangle.CollidesWith);
        }
    }
}