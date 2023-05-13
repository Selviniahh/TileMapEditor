using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace imguiTut;

public class Camera
{
    public float Zoom { get; private set; }
    public Vector2 Position;
    public float scrollWheelDelta;
    private Vector2 mouseWorldPosition;

    public Camera()
    {
        Zoom = 1f;
        Position = new Vector2(Globals.ScreenWidth, Globals.ScreenHeight);
    }

    public void Update(GameTime gameTime)
    {
        //zoom 
        scrollWheelDelta = Globals.CurrentMouse.ScrollWheelValue - Globals.PreviousMouse.ScrollWheelValue;
        if (scrollWheelDelta != 0 && !Window.IsWindowFocused)
        { 
            mouseWorldPosition = Vector2.Transform(Globals.CurrentMouse.Position.ToVector2(), Matrix.Invert(GetTransform()));
            float zoomFactor = 1 + (scrollWheelDelta / 1200f);
            Zoom *= zoomFactor;
            Vector2 mouseWorldPositionAfterZoom = Vector2.Transform(Globals.CurrentMouse.Position.ToVector2(), Matrix.Invert(GetTransform()));

            Position += (mouseWorldPosition - mouseWorldPositionAfterZoom);
        }
 
        if (Globals.CurrentKeyboardKey.IsKeyDown(Keys.D))
            Position.X += 5;
        if (Globals.CurrentKeyboardKey.IsKeyDown(Keys.A))
            Position.X += -5;
        if (Globals.CurrentKeyboardKey.IsKeyDown(Keys.W))
            Position.Y -= 5;
        if (Globals.CurrentKeyboardKey.IsKeyDown(Keys.S))
            Position.Y += 5;
    }

    public Matrix GetTransform()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
               Matrix.CreateScale(Zoom) *
               Matrix.CreateTranslation(mouseWorldPosition.X, mouseWorldPosition.Y, 0);
    }
}