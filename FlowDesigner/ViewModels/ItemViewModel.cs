using System;
using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class ItemViewModel : RectangleViewModel
    {

        public ItemViewModel(Guid id, string label, Vector2 position, Vector2 size) : base(position, size)
        {
            Id = id;
            Label = label;
        }

        public Guid Id { get; set; }
        public string Label { get; set; }
    }
}