using System;
using System.Collections.Generic;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ItemViewModel : RectangleViewModel
    {
        public List<ConnectionViewModel> Connections { get; set; }
        public ItemViewModel(Guid id, string label, Vector2 position, Vector2 size) : base(position, size)
        {
            Id = id;
            Label = label;
            Connections = new List<ConnectionViewModel>();
        }

        public Guid Id { get; set; }
        public string Label { get; set; }
    }
}