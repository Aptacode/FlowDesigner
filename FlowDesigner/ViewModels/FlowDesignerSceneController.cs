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
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Extensions;
using Aptacode.PathFinder.Maps.Hpa;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public class FlowDesignerSceneController : SceneController
    {
        public FlowDesignerSceneController(Vector2 size) : base(size)
        {
            UserInteractionController.OnMouseEvent += UserInteractionControllerOnOnMouseEvent;
            UserInteractionController.OnKeyboardEvent += UserInteractionControllerOnOnKeyboardEvent;
            ComponentScene = new Scene(size);
            ConnectionScene = new Scene(size);
            
            var component1 = new ConnectedComponentViewModel(Polygon.Rectangle.FromPositionAndSize(new Vector2(20, 20), new Vector2(10, 5)));
            var component2 = new ConnectedComponentViewModel(Polygon.Rectangle.FromPositionAndSize(new Vector2(40, 40), new Vector2(10, 5)));
            var component3 = new ConnectedComponentViewModel(Polygon.Rectangle.FromPositionAndSize(new Vector2(80, 80), new Vector2(10, 5)));
            var component4 = new ConnectedComponentViewModel(Polygon.Rectangle.FromPositionAndSize(new Vector2(70, 40), new Vector2(10, 5)));
            ComponentScene.Add(component1);
            ComponentScene.Add(component2);
            ComponentScene.Add(component3);
            ComponentScene.Add(component4);
            
            var connectionPoint1 = component1.AddConnectionPoint();
            var connectionPoint2 = component2.AddConnectionPoint();
            var connectionPoint3 = component3.AddConnectionPoint();
            var connectionPoint4 = component4.AddConnectionPoint();

            Map = new HierachicalMap(ComponentScene);
            
   
            var connection1 = ConnectionViewModel.Connect(Map, connectionPoint1, connectionPoint2);
            var connection2 = ConnectionViewModel.Connect(Map, connectionPoint1, connectionPoint3);
            var connection3 = ConnectionViewModel.Connect(Map, connectionPoint2, connectionPoint3);
            var connection4 = ConnectionViewModel.Connect(Map, connectionPoint3, connectionPoint4);
            ConnectionScene.Add(connection1);
            ConnectionScene.Add(connection2);
            ConnectionScene.Add(connection3);
            ConnectionScene.Add(connection4);

            foreach (var component in ComponentScene.Components)      
            {
                if (component is ConnectedComponentViewModel c)
                {
                    component.OnMouseDown += (object? sender, MouseDownEvent e) => { DraggingComponent = component; };
                    component.OnMouseUp += (object? sender, MouseUpEvent e) => { DraggingComponent = null;};
                }
            }
            
            Add(ComponentScene);
            Add(ConnectionScene);
        }

        public Scene ComponentScene { get; set; }
        public Scene ConnectionScene { get; set; }
        
        private void UserInteractionControllerOnOnMouseEvent(object? sender, MouseEvent e)
        {
            if (DraggingComponent != null)
            {
                if (e is MouseMoveEvent moveEvent)
                {
                    var delta = moveEvent.Position - UserInteractionController.LastMousePosition;

                    ComponentScene.Translate(DraggingComponent, delta, new List<ComponentViewModel> { DraggingComponent },
                        new CancellationTokenSource());
                    
                    Map.Update(DraggingComponent);
                    
                    foreach (var component in ConnectionScene.Components)
                    {
                        if (component is ConnectionViewModel conneciton && conneciton.CollidesWith(DraggingComponent))
                        {
                            conneciton.RecalculatePath();
                        }
                    }
                }
            }

            foreach (var componentViewModel in ComponentScene.Components)
            {
                componentViewModel.HandleMouseEvent(e);
            }

            if (e is MouseClickEvent)
            {
                EditingComponent = ComponentScene.Components.First(c => c.CollisionDetectionEnabled && c.CollidesWith(e.Position) && c is ConnectedComponentViewModel);
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
        public readonly HierachicalMap Map;

        #endregion

        #region EventHandlers

        private void UserInteractionControllerOnOnMouseDown(object? sender, Vector2 e)
        {
            if (_isEditingText)
            {
                _isEditingText = false;
            }
            
            DraggingComponent = null;

            foreach (var componentViewModel in ComponentScene.Components.CollidingWith(e))
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

            ComponentScene.BringToFront(DraggingComponent);
        }
        #endregion
    }
}