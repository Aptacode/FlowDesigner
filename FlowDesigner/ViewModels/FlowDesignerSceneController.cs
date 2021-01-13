using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Extensions;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Scene.Events;
using Aptacode.FlowDesigner.Core.ViewModels.Components;
using Aptacode.Geometry.Primitives.Extensions;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class FlowDesignerSceneController : SceneController
    {
        public FlowDesignerSceneController(Scene scene) : base(scene)
        {
            UserInteractionController.OnMouseEvent += UserInteractionControllerOnOnMouseEvent;
            UserInteractionController.OnKeyboardEvent += UserInteractionControllerOnOnKeyboardEvent;

            foreach (var component in scene.Components)      
            {
                if (component is ConnectedComponentViewModel c)
                {
                    component.OnMouseDown += (object? sender, MouseDownEvent e) => { DraggingComponent = component; };
                    component.OnMouseUp += (object? sender, MouseUpEvent e) => { DraggingComponent = null;};
                }
            }
        }

        private void UserInteractionControllerOnOnMouseEvent(object? sender, MouseEvent e)
        {
            if (DraggingComponent != null)
            {
                if (e is MouseMoveEvent moveEvent)
                {
                    var delta = moveEvent.Position - UserInteractionController.LastMousePosition;

                    Translate(DraggingComponent, delta, new List<ComponentViewModel> { DraggingComponent },
                        new CancellationTokenSource());
                }
            }

            foreach (var componentViewModel in Scene.Components)
            {
                componentViewModel.HandleMouseEvent(e);
            }

            if (e is MouseClickEvent)
            {
                EditingComponent = Scene.Components.First(c => c.CollisionDetectionEnabled && c.CollidesWith(e.Position) && c is ConnectedComponentViewModel);
                _isEditingText = EditingComponent != null;
            }
        }

        private void UserInteractionControllerOnOnKeyboardEvent(object? sender, KeyboardEvent keyboardEvent)
        {
            var e = keyboardEvent.Key;
            
            if (_isEditingText && EditingComponent != null)
            {
                if (e.Length == 1 && rg.IsMatch(e))
                {
                    EditingComponent.Text += e;
                }
                else if (string.Equals(e, "backspace", System.StringComparison.OrdinalIgnoreCase) && EditingComponent.Text.Length > 0)
                {
                    EditingComponent.Text = DraggingComponent.Text.Substring(0, EditingComponent.Text.Length - 1);
                }
                EditingComponent.Invalidated = true;
            }
        }

        #region Properties

        private Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
        private bool _isEditingText = false;
        public ComponentViewModel EditingComponent { get; set; }
        public ComponentViewModel DraggingComponent { get; set; }
        public ConnectedComponentViewModel SelectedConnectedComponent { get; set; }
        public ConnectionPointViewModel SelectedConnectionPoint { get; set; }

        #endregion

        #region EventHandlers

        private void UserInteractionControllerOnOnMouseDown(object? sender, Vector2 e)
        {
            if (_isEditingText)
            {
                _isEditingText = false;
            }
            
            DraggingComponent = null;

            foreach (var componentViewModel in Scene.Components.CollidingWith(e))
            {
                DraggingComponent = componentViewModel;
                componentViewModel.BorderColor = Color.Green;
            }

            if (DraggingComponent is ConnectedComponentViewModel connectedComponent)
            {
                SelectedConnectedComponent = connectedComponent;
                if (!SelectedConnectedComponent.Body.CollidesWith(e.ToPoint()))
                {
                    var connectionPoints = SelectedConnectedComponent.ConnectionPoints.Where(p => p.CollidesWith(e));
                    if (connectionPoints.Any())
                    {
                        SelectedConnectionPoint = connectionPoints.First();
                        DraggingComponent = null;
                    }
                }
            }

            Scene.BringToFront(DraggingComponent);
        }
        #endregion
    }
}