using System;
using System.Collections.Generic;

namespace Aptacode.FlowDesigner.Core.ViewModels.Components
{
    public class ComponentGroupViewModel : BaseComponentViewModel
    {
        public ComponentGroupViewModel(Guid id) : base(id) { }

        public List<BaseComponentViewModel> Children { get; set; }

        public void AddChild(BaseComponentViewModel child)
        {
            Children.Add(child);
        }

        public void RemoveChild(BaseComponentViewModel child)
        {
            Children.Remove(child);
        }
    }
}