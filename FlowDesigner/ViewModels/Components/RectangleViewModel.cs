using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class RectangleViewModel : BaseShapeViewModel
    {
        public RectangleViewModel(Guid id, Vector2 position, Vector2 size) : base(id, position, size) { }
    }
}