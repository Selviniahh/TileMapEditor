using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGui;
using Num = System.Numerics;
using imguiTut;

namespace imguiTut;

public class Window
{
    public bool SetCollision = false; 
    private int _cellSize;
    private bool _showSecWindow; 
    public static bool IsWindowFocused;
    private Texture2D _firstTexture;
    private IntPtr _texPointer;
    private readonly ImGuiRenderer _imGuiRenderer;
    private Texture2D _clickedTexture;
    private SpriteFont _font;
    private string _clickedTextureName;
    public readonly Tile Tile;
    private Texture2D _itemsAtlas, _terrainAtlas, _dungeon, _newdungeon;
    private readonly Dictionary<string, List<Rectangle>> _atlasSourceRectanglesDict;
    private int _windowWidth = 300, _windowHeight, _gridSize = 10, cellWidth, cellHeight;
    private Camera _camera;
    
    private Rectangle _rectTotal;
    private List<Rectangle> _clickedArea = new List<Rectangle>();
    private Num.Vector2 _firstClick2 = new Num.Vector2();

        public Window(ImGuiRenderer imGuiRenderer, Camera camera)
    {
        _imGuiRenderer = imGuiRenderer;
        _camera = camera;
        _clickedTexture = Globals.Content.Load<Texture2D>("Final");
        Tile = new Tile(() => _clickedTexture, () => IsWindowFocused, () => _clickedTextureName, _camera);
        _atlasSourceRectanglesDict = new Dictionary<string, List<Rectangle>>();
        _windowHeight = (int)Globals.ScreenHeight;
    }

    public void ImGuiDraw(GameTime gameTime)
    {
        
        // Tab bars
        ImGui.SetNextWindowPos(new Num.Vector2(Globals.ScreenWidth - 300, 0));
        ImGui.SetNextWindowSize(new Num.Vector2(_windowWidth + 100, _windowHeight));
        ImGui.Begin("Main Dock space", ImGuiWindowFlags.DockNodeHost | ImGuiWindowFlags.NoTitleBar);
        if (ImGui.BeginTabBar("MainTabBar"))
        {
            
            if (ImGui.BeginTabItem("Debug"))
            {
                ImGui.Text("Global mouse position: " + Globals.CurrentMouse.Position);
                ImGui.Text("Clicked Texture: " + _clickedTexture);
                ImGui.Text("Last clicked index: " + Tile.LastClickedIndex);
                ImGui.Text("CurrentMouse.ScrollWheelValue " + Globals.CurrentMouse.ScrollWheelValue); 
                ImGui.Text("PreviousMouse.ScrollWheelValue " + Globals.PreviousMouse.ScrollWheelValue); 
                ImGui.Text("_rectTotal.Width " + _rectTotal.Width); 
                
                
                //display Textures properties
                if (ImGui.TreeNode("TextureProperties"))
                {
                    ImGui.Columns(2, "TexturesColumns", true); // 5 columns for Name, Tag, Width, Height, and Position
                    ImGui.Text("Index"); ImGui.NextColumn();
                    ImGui.Text("Tag"); ImGui.NextColumn();
                    ImGui.Separator();
                    foreach (var texture in Tile.DefaultGrid.Textures)
                    {
                        int index = Tile.DefaultGrid.Textures.IndexOf(texture);
                        ImGui.Text(index.ToString()); ImGui.NextColumn();
                        ImGui.Text(texture is null || texture.Tag is null ? "null" : texture.Tag.ToString()); ImGui.NextColumn();
                    }

                    ImGui.Columns(1);
                    ImGui.Separator();
                    ImGui.TreePop();
                }
                
                if (ImGui.TreeNode("2"))
                {
                    ImGui.Columns(2, "2", true); // 5 columns for Name, Tag, Width, Height, and Position
                    ImGui.Text("Index"); ImGui.NextColumn();
                    ImGui.Text("Tag"); ImGui.NextColumn();
                    ImGui.Separator();
                    foreach (var texture in Tile.DefaultGrid.Textures)
                    {
                        ImGui.Text(Tile.DefaultGrid.Rectangles.ToString()); ImGui.NextColumn();
                        ImGui.Text(texture is null || texture.Tag is null ? "null" : texture.Tag.ToString()); ImGui.NextColumn();
                    }

                    ImGui.Columns(1);
                    ImGui.Separator();
                    ImGui.TreePop();
                }
                ImGui.EndTabItem();

            }

            if (ImGui.BeginTabItem("Tab 2"))
            {
                ImGui.PushItemWidth(150);
                if (ImGui.InputInt("", ref _cellSize, 1, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {}
 
                ImGui.PopItemWidth();
                ImGui.SameLine();
                
                if (ImGui.Button("CanOverlap"))
                    Tile.OverlapOption = !Tile.OverlapOption;
                if (ImGui.Button("Save as Json"))
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filePath = Path.Combine(documentsPath, "textures.json");
                    Tile.SaveGrid(filePath);

                }
                if (ImGui.Button("Set Collisions"))
                {
                    SetCollision = true;
                }
                if (ImGui.Button("Stop setting Collisions"))
                {
                    SetCollision = false;
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Tab 3"))
            {
                if (ImGui.BeginTabBar("ImagesTabBar"))
                {
                    if (ImGui.BeginTabItem("1"))
                    {
                        ImGui.BeginChild("Atlas1", new Num.Vector2(300, 0), true, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                        DrawAtlasImagesToWindow("Atlas3", _dungeon);
                        ImGui.EndChild();
                        
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("2"))
                    {
                        ImGui.BeginChild("Atlas2", new Num.Vector2(300, 0), true, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
                        DrawAtlasImagesToWindow("Atlas4", _newdungeon);
                        ImGui.EndChild();
                        
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
            
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        ImGui.End();
    }

    private void DrawAtlasImagesToWindow(string atlasId, Texture2D textureAtlas)
    {
        if (!_atlasSourceRectanglesDict.ContainsKey(atlasId))
        {
            _atlasSourceRectanglesDict[atlasId] = AtlasImageExtractor.ExtractRectanglesFromAtlas(textureAtlas);
        }

        List<Rectangle> sourceRectangles = _atlasSourceRectanglesDict[atlasId];

        float spacing =  8.0f;
        float currentX = 0.0f;

        for (int i = 0; i < sourceRectangles.Count; i++)
        {
            float drawnWidth = DrawTextureFromAtlasWithGrid(textureAtlas, sourceRectangles[i], atlasId + i);

            currentX += drawnWidth + spacing;

            if (i < sourceRectangles.Count - 1 && currentX + sourceRectangles[i + 1].Width > ImGui.GetWindowSize().X)
            {
                currentX = 0.0f;
                ImGui.NewLine();
            }
            else
            {
                ImGui.SameLine(0.0f, spacing);
            }
        }   
    }
    private readonly Dictionary<string, Texture2D> _generatedTexturesdict = new Dictionary<string, Texture2D>();

    private Texture2D CalculateTextureFromAtlas(string texturename, Rectangle sourceRectangle, Texture2D textureAtlas)
    {
        if (_generatedTexturesdict.ContainsKey(texturename))
        {
            return _generatedTexturesdict[texturename];
        }
        
        var texture = new Texture2D(Globals.SpriteBatch.GraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
        Color[] pixelData = new Color[sourceRectangle.Width * sourceRectangle.Height];
        textureAtlas.GetData(0, sourceRectangle, pixelData, 0, pixelData.Length);
        texture.SetData(pixelData);
        _generatedTexturesdict.Add(texturename, texture);
        return texture;
    }

    //THIS CLASS DOES EVERYTHING WHAT I WOULD POSSIBLY NEED
    private float DrawTextureFromAtlasWithGrid(Texture2D textureAtlas, Rectangle sourceRectangle, string textureName)
    {
        var drawList = ImGui.GetWindowDrawList();
        var texture = CalculateTextureFromAtlas(textureName, sourceRectangle, textureAtlas);
        _texPointer = _imGuiRenderer.BindTexture(texture);
        ImGui.Image(_texPointer, new Num.Vector2(sourceRectangle.Width, sourceRectangle.Height));

        //Draw Red grid indicator.
        if (ImGui.IsItemHovered())
        {
            cellWidth = 32;
            cellHeight = 32;
            Num.Vector2 mousePos = ImGui.GetMousePos() - ImGui.GetItemRectMin();
            int indexX = (int)mousePos.X / cellWidth;
            int indexY = (int)mousePos.Y / cellHeight;

            drawList.AddRect(
                ImGui.GetItemRectMin() + new Num.Vector2(indexX * cellWidth, indexY * cellHeight),
                ImGui.GetItemRectMin() + new Num.Vector2((indexX + 1) * cellWidth, (indexY + 1) * cellHeight),
                ImGui.GetColorU32(new Num.Vector4(1.0f, 0.0f, 0.0f, 1.0f))
            );

            if (Globals.CurrentMouse.RightButton == ButtonState.Pressed)
            {
                Rectangle clickedArea = new Rectangle(sourceRectangle.X + indexX * cellWidth, sourceRectangle.Y + indexY * cellHeight, cellWidth, cellHeight);
                if (!_clickedArea.Contains(clickedArea))
                {
                    _clickedArea.Add(clickedArea);
                    
                    if (_clickedArea.Count != 1)
                    {
                        _clickedTexture.Name = $"{textureName}_Cell_{indexX}_{indexY}";
                        _clickedTextureName = $"{textureName}_Cell_{indexX}_{indexY}";  
                        
                        int width = _clickedArea[_clickedArea.Count - 1].X - _clickedArea[_clickedArea.Count - 2].X;
                        int height = _clickedArea[_clickedArea.Count - 1].Y - _clickedArea[_clickedArea.Count - 2].Y;
                        
                        _rectTotal.Width += width;
                        _rectTotal.Height += height;
                        
                        var sec = (new Num.Vector2(_rectTotal.X, _rectTotal.Y) + ImGui.GetItemRectMin());

                        drawList.AddRect(
                            _firstClick2,
                            ImGui.GetItemRectMin() + new Num.Vector2((indexX + 1) * cellWidth, (indexY + 1) * cellHeight),
                            ImGui.GetColorU32(new Num.Vector4(0.0f, 0.0f, 1.0f, 1.0f))
                        );
                        
                        _clickedTexture = CalculateTextureFromAtlas($"{textureName}_Cell_{indexX}_{indexY}", _rectTotal, textureAtlas);
                    }
                    
                    else
                    {
                        //first click
                        _clickedTexture.Name = $"{textureName}_Cell_{indexX}_{indexY}";
                        _clickedTextureName = $"{textureName}_Cell_{indexX}_{indexY}";
                        _clickedTexture = CalculateTextureFromAtlas($"{textureName}_Cell_{indexX}_{indexY}", clickedArea, textureAtlas);
                        _rectTotal.X += clickedArea.X;
                        _rectTotal.Y += clickedArea.Y;
                        _rectTotal.Height = 32;
                        _rectTotal.Width = 32;
                        _firstClick2 = ImGui.GetItemRectMin() + new Num.Vector2(indexX * cellWidth, indexY * cellHeight);
                    }
                }
            }

                 
            
            if (ImGui.IsItemClicked())
            {
                Rectangle clickedArea = new Rectangle(sourceRectangle.X + indexX * cellWidth, sourceRectangle.Y + indexY * cellHeight, cellWidth, cellHeight);
                _clickedTexture = CalculateTextureFromAtlas($"{textureName}_Cell_{indexX}_{indexY}", clickedArea, textureAtlas);
                _clickedTexture.Name = $"{textureName}_Cell_{indexX}_{indexY}";
                _clickedTextureName = $"{textureName}_Cell_{indexX}_{indexY}";
            }
            
            if (Globals.CurrentKeyboardKey.IsKeyDown(Keys.M)) //reset all selection
            {
                for (int i = 0; i < _clickedArea.Count; i++)
                {
                    _clickedArea.RemoveAt(i);
                }

                _rectTotal = new Rectangle(0, 0, 0, 0);
                Rectangle clickedArea = new Rectangle(sourceRectangle.X + indexX * cellWidth, sourceRectangle.Y + indexY * cellHeight, cellWidth, cellHeight);
                _clickedTexture = CalculateTextureFromAtlas($"{textureName}_Cell_{indexX}_{indexY}", clickedArea, textureAtlas);
                _clickedTexture.Name = $"{textureName}_Cell_{indexX}_{indexY}";
                _clickedTextureName = $"{textureName}_Cell_{indexX}_{indexY}";
                
                MouseState simulatedLeftClick = new MouseState(Globals.CurrentMouse.X, Globals.CurrentMouse.Y, Globals.CurrentMouse.ScrollWheelValue, ButtonState.Pressed, Globals.CurrentMouse.RightButton, Globals.CurrentMouse.MiddleButton, Globals.CurrentMouse.XButton1, Globals.CurrentMouse.XButton2);
                Globals.PreviousMouse = Globals.CurrentMouse;
                Globals.CurrentMouse = simulatedLeftClick;
            }
        }
        
        return sourceRectangle.Width;
    }


    public void LoadContent()
    {
        Tile.LoadContent();
        _font = Globals.Content.Load<SpriteFont>("Font");
        _dungeon = Globals.Content.Load<Texture2D>("Dungeon_Tileset");
        _newdungeon = Globals.Content.Load<Texture2D>("tileset_dungeon");
        
    }

    public void Update(GameTime gameTime)
    {
        Tile.Update();
        if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && !ImGui.IsAnyItemFocused()) //run if not focused and not hovered 
        {
            if (Globals.CurrentMouse.LeftButton == ButtonState.Pressed) //assign every frame ClickedIndex to clicked position
            {
                Vector2 clickPosition = new Vector2(Globals.CurrentMouse.X, Globals.CurrentMouse.Y);
                Tile.ClickedIndex = Tile.GetClickedIndex(clickPosition);
                if (Tile.ClickedIndex != -1)
                {
                    Tile.Collisions[Tile.ClickedIndex] = true;
                    Tile.ClickedIndex = Tile.GetClickedIndex(clickPosition);

                }
            }
        }

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && !ImGui.IsAnyItemFocused())
            IsWindowFocused = true;
        else //Just bool assigning true if Window clicked
            IsWindowFocused = false;
    }
    public void Draw(GameTime gameTime)
    {
        Tile.Draw();
    }
}