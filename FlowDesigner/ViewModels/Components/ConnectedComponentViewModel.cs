using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Aptacode.FlowDesigner.Core.Enums;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectedComponentViewModel : BaseShapeViewModel
    {
        private string _label;

        public ConnectedComponentViewModel(Guid id, string label, Vector2 position, Vector2 size) : base(id, position,
            size)
        {
            Label = label;
        }

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public List<ConnectionPointViewModel> ConnectionPoints { get; set; } = new List<ConnectionPointViewModel>();

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);
            ConnectionPoints.ForEach(c => c.AddTo(designer));
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);
            ConnectionPoints.ToList().ForEach(c => c.RemoveFrom(designer));
        }

        public override void Resize(DesignerViewModel designer, Vector2 delta, ResizeDirection direction)
        {
            base.Resize(designer, delta, direction);
            ConnectionPoints.ForEach(c => c.Redraw());
        }

        public override void Select(DesignerViewModel designer)
        {
            if (IsSelected)
            {
                return;
            }

            IsSelected = true;
            BorderColor = Color.Green;
            ConnectionPoints.ForEach(c => c.Select(designer));
            base.Select(designer);
        }

        public override void Deselect(DesignerViewModel designer)
        {
            if (!IsSelected)
            {
                return;
            }

            IsSelected = false;
            BorderColor = Color.Black;
            ConnectionPoints.ForEach(c => c.Deselect(designer));
            base.Deselect(designer);
        }
    }
}