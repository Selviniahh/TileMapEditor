using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace imguiTut;

public class Globals
{
    public static ContentManager Content { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static float ElapsedSeconds { get; set; }
    public static float ScreenWidth { get; set; }
    public static float ScreenHeight { get; set; }
    
    public static MouseState PreviousMouse { get;  set; }
    public static MouseState CurrentMouse { get;  set; }
    public static KeyboardState CurrentKeyboardKey { get; private set; }
    public static KeyboardState PreviousKeyboardKey { get; private set; }
    
    public static GraphicsDevice GraphicsDevice;

    private static int _frameCounter;
    private static float _elapsedTime;

    
    public static float FPS { get; private set; }
    
    public static void InitializeGlobals(ContentManager content, SpriteBatch spriteBatch, float screenWidth, float screenHeight)
    {
        Content = content;
        SpriteBatch = spriteBatch;
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
    }
    
    public static void Update(GameTime gameTime)
    {
        PreviousMouse = CurrentMouse;
        CurrentMouse = Mouse.GetState();

        PreviousKeyboardKey = CurrentKeyboardKey;
        CurrentKeyboardKey = Keyboard.GetState();
        
        
        ElapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter++;
        _elapsedTime += ElapsedSeconds;

        if (_elapsedTime >= 1.0f)
        {
            FPS = _frameCounter / _elapsedTime;
            _frameCounter = 0;
            _elapsedTime = 0;
        }
    }
}

