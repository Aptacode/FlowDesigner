using System;
using System.Collections.Generic;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ConnectedComponentViewModel : BaseShapeViewModel
    {
        public ConnectedComponentViewModel(Guid id, string label, Vector2 position, Vector2 size) : base(id, position,
            size)
        {
            Label = label;
        }

        public string Label { get; set; }

        public List<ConnectionPointViewModel> ConnectionPoints { get; set; } = new List<ConnectionPointViewModel>();

        public override void AddTo(DesignerViewModel designer)
        {
            designer.Add(this);

            foreach (var connectionPoint in ConnectionPoints.ToArray())
            {
                connectionPoint.AddTo(designer);
            }
        }

        public override void RemoveFrom(DesignerViewModel designer)
        {
            designer.Remove(this);

            foreach (var connectionPoint in ConnectionPoints.ToArray())
            {
                connectionPoint.RemoveFrom(designer);
            }
        }
    }
}