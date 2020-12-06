using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class PointBase : ComponentBase, IDisposable
    {
        [Parameter] public PointViewModel Point { get; set; }

        public void Dispose()
        {
            Point.PropertyChanged -= Item_PropertyChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Point.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }
    }
}