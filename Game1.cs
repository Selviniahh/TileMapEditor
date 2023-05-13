using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGui;
using ImGuiNET;
using Microsoft.Xna.Framework.Content;
using Num = System.Numerics;

namespace imguiTut;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private ImGuiRenderer _imGuiRenderer;
    private Window _window;
    private Camera _camera;
    private SetCollisionsWindow _setCollisionsWindow;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Globals.Content = Content;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _camera = new Camera();
        double targetFPS = 170;
        double targetElapsedTimeInSeconds = 1.0 / targetFPS;
        TimeSpan targetElapsedTime = TimeSpan.FromSeconds(targetElapsedTimeInSeconds);

        // Apply the target elapsed time
        TargetElapsedTime = targetElapsedTime;
        
        // _graphics.SynchronizeWithVerticalRetrace = true;
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 500;
        Globals.ScreenHeight = _graphics.PreferredBackBufferHeight;
        Globals.ScreenWidth = _graphics.PreferredBackBufferWidth;
        Globals.GraphicsDevice = GraphicsDevice;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
        
       
    }

    protected override void Initialize()
    {
        _imGuiRenderer = new ImGuiRenderer(this).Initialize().RebuildFontAtlas();
        _window = new Window(_imGuiRenderer, _camera);
        _setCollisionsWindow = new SetCollisionsWindow(_window.Tile);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.SpriteBatch = _spriteBatch;
        Globals.GraphicsDevice = GraphicsDevice;
        _window.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);
       _camera.Update(gameTime);
       if (_setCollisionsWindow.SetCollisions)
       {
           _setCollisionsWindow.Update();
       }
       else
       {
            _window.Update(gameTime);           
       }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(transformMatrix: _camera.GetTransform(), samplerState: SamplerState.PointWrap);
        _window.Draw(gameTime);
        _spriteBatch.End();

        //Window section
        _imGuiRenderer.BeginLayout(gameTime);
        _setCollisionsWindow.DrawWindow();
        
        if (_setCollisionsWindow.SetCollisions)
        {
            _setCollisionsWindow.DrawWindow();
        }
        else
        {
            _window.ImGuiDraw(gameTime);
        }
        
        _imGuiRenderer.EndLayout();
        base.Draw(gameTime);
    }

    
}