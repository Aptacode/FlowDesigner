using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.FlowDesigner.Core.Enums;
using Aptacode.FlowDesigner.Core.Extensions;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class PathViewModel : ComponentViewModel
    {
        private string _path;


        public PathViewModel() : this(Guid.NewGuid(), new Vector2[0]) { }

        public PathViewModel(Guid id, IEnumerable<Vector2> points) : base(id)
        {
            AddPoints(points);
            CollisionsAllowed = true;
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public bool CollisionsAllowed { get; set; }

        public override bool CollidesWith(CollisionType type, params Vector2[] vertices)
        {
            return CollisionsAllowed && Points.Any(point => Collider.Collides(vertices, point));
        }


        public void AddPoint(Vector2 point)
        {
            var newPoints = _points.ToList();
            newPoints.Add(point);
            _points = newPoints.ToArray();
            Redraw();
        }

        public void AddPoints(IEnumerable<Vector2> points)
        {
            var newPoints = _points.ToList();
            newPoints.AddRange(points);
            _points = newPoints.ToArray();
            Redraw();
        }

        public void ClearPoints()
        {
            _points = new Vector2[0];
            Redraw();
        }

        public void Translate(Vector2 delta)
        {
            _points = _points.ToList().ConvertAll(p => p + delta).ToArray();
            Redraw();
        }

        public void Redraw()
        {
            var pathBuilder = new StringBuilder();
            _points.ToList().ForEach(point => pathBuilder.Add(point));
            Path = pathBuilder.ToString();
        }

        #region Collision

        public override void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction) { }

        public override void Resize(DesignerViewModel designer, Vector2 delta) { }

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);
        }

        #endregion
    }
}