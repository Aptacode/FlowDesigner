using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Aptacode.FlowDesigner.Core.Enums;
using Aptacode.FlowDesigner.Core.Extensions;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class PathViewModel : BaseComponentViewModel, ICollider
    {
        private string _path;
        private List<Vector2> _points = new List<Vector2>();

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

        public void Translate(Vector2 delta)
        {
            _points = _points.ConvertAll(p => p + delta);
            Redraw();
        }

        public void Redraw()
        {
            var pathBuilder = new StringBuilder();
            _points.ForEach(point => pathBuilder.Add(point));
            Path = pathBuilder.ToString();
        }

        #region Collision

        public virtual bool CollidesWith(Vector2 position) =>
           CollisionsAllowed && _points.Any(p => Math.Abs(position.X - p.X) < Constants.Tolerance && Math.Abs(position.Y - p.Y) < Constants.Tolerance);

        public virtual bool CollidesWithEdge(Vector2 position) => CollidesWith(position);

        public virtual bool CollidesWith(params Vector2[] points) =>
            CollisionsAllowed &&
            points.Any(CollidesWith);

        public virtual bool CollidesWith(Vector2 point, Vector2 shape)
        {
            return _points.Any(p =>
                p.X >= point.X && point.X <= point.X + shape.X &&
                p.Y >= point.Y && point.Y <= point.Y + shape.Y
            );
        }

        public override void Move(DesignerViewModel designer, Vector2 delta) { }

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