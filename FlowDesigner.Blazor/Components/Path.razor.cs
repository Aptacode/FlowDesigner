using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Microsoft.AspNetCore.Components;

namespace Aptacode.FlowDesigner.Blazor.Components
{
    public class PathBase : ComponentBase, IDisposable
    {
        [Parameter] public PathViewModel Path { get; set; }

        public void Dispose()
        {
            Path.PropertyChanged -= Item_PropertyChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Path.PropertyChanged += Item_PropertyChanged;

            await base.OnInitializedAsync();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }
    }
}