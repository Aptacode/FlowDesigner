using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Extensions;
using Aptacode.Geometry.Primitives.Extensions;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class FlowDesignerSceneController : SceneControllerViewModel
    {
        public FlowDesignerSceneController(SceneViewModel scene) : base(scene)
        {
            UserInteractionController.OnMouseDown += UserInteractionControllerOnOnMouseDown;
            UserInteractionController.OnMouseUp += UserInteractionControllerOnOnMouseUp;
            UserInteractionController.OnMouseMoved += UserInteractionControllerOnOnMouseMoved;
        }

        public ComponentViewModel SelectedComponent { get; set; }

        public ConnectedComponentViewModel SelectedConnectedComponent { get; set; }
        public ConnectionPointViewModel SelectedConnectionPoint { get; set; }

        private void UserInteractionControllerOnOnMouseMoved(object? sender, Vector2 e)
        {
            if (SelectedConnectionPoint != null)
            {
                SelectedConnectionPoint.Move(e);
            }


            if (SelectedComponent == null)
            {
                return;
            }

            var delta = e - UserInteractionController.LastMousePosition;

            Translate(SelectedComponent, delta, new List<ComponentViewModel> {SelectedComponent},
                new CancellationTokenSource());
        }

        private void UserInteractionControllerOnOnMouseUp(object? sender, Vector2 e)
        {
            foreach (var componentViewModel in Scene.Components)
            {
                componentViewModel.BorderColor = Color.Black;
            }

            SelectedComponent = null;
            SelectedConnectionPoint = null;
            SelectedConnectedComponent = null;
        }

        private void UserInteractionControllerOnOnMouseDown(object? sender, Vector2 e)
        {
            SelectedComponent = null;

            foreach (var componentViewModel in Scene.Components.CollidingWith(e, CollisionDetector))
            {
                SelectedComponent = componentViewModel;
                componentViewModel.BorderColor = Color.Green;
            }

            if (SelectedComponent is ConnectedComponentViewModel connectedComponent)
            {
                SelectedConnectedComponent = connectedComponent;
                if (!SelectedConnectedComponent.Body.CollidesWith(e.ToPoint(), CollisionDetector))
                {
                    var connectionPoints = SelectedConnectedComponent.ConnectionPoints.Where(p => p.CollidesWith(e.ToPoint(), CollisionDetector));
                    if (connectionPoints.Count() > 0)
                    {
                        SelectedConnectionPoint = connectionPoints.First();
                        SelectedComponent = null;
                    }
                }
            }

            Scene.BringToFront(SelectedComponent);
        }
    }
}