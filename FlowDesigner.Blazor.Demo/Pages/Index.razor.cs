using System.Numerics;
using System.Threading.Tasks;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Utilities;
using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Aptacode.Geometry.Primitives.Polygons;
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
            var componentBuilder = new ComponentBuilder();
            var width = 200;
            var height = 100;

            var component1 = new ConnectedComponentViewModel(Rectangle.Create(20, 20, 10, 5));
            var component2 = new ConnectedComponentViewModel(Rectangle.Create(40, 40, 10, 5));

            var connectionPoint1 = component1.AddConnectionPoint();
            var connectionPoint2 = component2.AddConnectionPoint();

            var scene = new Scene(new Vector2(width, height));
            var connection = ConnectionViewModel.Connect(scene, connectionPoint1, connectionPoint2);

            scene.Add(component1);
            scene.Add(component2);
            scene.Add(connection);

            SceneController = new FlowDesignerSceneController(scene);

            await base.OnInitializedAsync();
        }
    }
}