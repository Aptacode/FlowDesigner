using System.Numerics;
using System.Threading.Tasks;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Utilities;
using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace FlowDesigner.Blazor.Demo.Pages
{
    public class IndexBase : ComponentBase
    {
        #region Properties

        public FlowDesignerSceneController SceneController { get; set; }

        #endregion


        protected override async Task OnInitializedAsync()
        {
            var width = 200;
            var height = 100;

            SceneController = new FlowDesignerSceneController(new Vector2(width, height)) {ShowGrid = true};
                
            await base.OnInitializedAsync();
        }
    }
}