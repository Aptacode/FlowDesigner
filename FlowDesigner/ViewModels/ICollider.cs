using System.Numerics;

namespace Aptacode.FlowDesigner.Core.ViewModels
{
    public interface ICollider
    {
        bool CollisionsAllowed { get; set; }
        bool CollidesWithEdge(Vector2 point);
        bool CollidesWith(Vector2 point);
        bool CollidesWith(params Vector2[] points);
        bool CollidesWith(Vector2 point, Vector2 shape);
    }
}