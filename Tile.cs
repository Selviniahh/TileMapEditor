using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace imguiTut;

public class DefaultGrid
{
    public List<Texture2D> Textures;
    public List<Rectangle> Rectangles;
}
public class Tile
{
    public readonly DefaultGrid DefaultGrid;
    public List<DrawnTexture> DrawnTextures { get; set; }
    private Camera _camera;
    private Texture2D _pixel;

    private readonly int _column = 50;
    private readonly int _row = 50;
    public int CellSize = 32;
    private bool _showBorders, _showHover, _rotate,_isOverlapped;
    public bool OverlapOption = false;
    private float _rotation;
    private Color _drawnColor = Color.White;
    public List<bool> Collisions = new List<bool>();

    private KeyboardState _previousKeyboard;
    private KeyboardState _currentKeyboard;
    private MouseState _currentMouse;
    private MouseState _previousMouse;
    KeyboardState _previousKeyboardState;

    public readonly Func<Texture2D> ClickedTexture; //_clickedTexture is in Window class. 
    private readonly Func<bool> _isWindowFocused; //if window is hovered. 
    private Func<string> _getClickedTextureName;
    private Dictionary<int, List<int>> _modifiedTagsDict;

    public int ClickedIndex { get; set; } = -1; //Very important stuff 
    public int LastClickedIndex { get; set; }


    public Tile( Func<Texture2D> clickedTexture, Func<bool> isWindowFocused,Func<string> getClickedTextureName, Camera camera)
    {
        ClickedTexture = clickedTexture;
        _isWindowFocused = isWindowFocused;
        _getClickedTextureName = getClickedTextureName;
        _camera = camera;
        DrawnTextures = new List<Tile.DrawnTexture>();
        _modifiedTagsDict = new Dictionary<int, List<int>>();
        DefaultGrid = new DefaultGrid();
    }

    public void LoadContent()
    {
        DefaultGrid.Textures = new List<Texture2D>();
        DefaultGrid.Rectangles = new List<Rectangle>();

        _pixel = new Texture2D(Globals.SpriteBatch.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        for (int i = 0; i < _column * _row; i++)
        {
            DefaultGrid.Textures.Add(new Texture2D(Globals.GraphicsDevice,CellSize,CellSize));
        }
        
        SetupRectangles();
    }

    public void Update()
    {
        //Show borders
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();
        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();

        if (_previousKeyboard.IsKeyDown(Keys.LeftShift) && Globals.CurrentKeyboardKey.IsKeyUp(Keys.LeftShift))
        {
            _showBorders = !_showBorders;
        }

        if (_previousKeyboard.IsKeyDown(Keys.LeftControl) && Globals.CurrentKeyboardKey.IsKeyUp(Keys.LeftControl))
        {
            _showHover = !_showHover;
        }

        KeyboardState currentKeyboardState = Keyboard.GetState();
        if (currentKeyboardState.IsKeyDown(Keys.LeftControl) && currentKeyboardState.IsKeyDown(Keys.Z) && _previousKeyboardState.IsKeyUp(Keys.Z))
        {
            if (DrawnTextures.Count != 0)
            {
                DrawnTextures.RemoveAt(DrawnTextures.Count - 1);

                if (_modifiedTagsDict.Count > 0)
                {
                    int lastIndex = _modifiedTagsDict.Keys.Last(); //last index of dictionary key's 

                    // Get the list of values with the last key
                    List<int> lastValues = _modifiedTagsDict[lastIndex];

                    // Set the tag of the last clicked index to null
                    DefaultGrid.Textures[lastIndex].Tag = null;

                    foreach (int value in lastValues)
                    {
                        // Update the tags of the cells
                        DefaultGrid.Textures[value].Tag = null;
                    }

                    _modifiedTagsDict.Remove(lastIndex);
                }
            }
        }
        _previousKeyboardState = currentKeyboardState;
    }

    public virtual void Draw()
    {
        foreach (var drawnImage in DrawnTextures)
        {
            if (drawnImage is not null)
            {
                Globals.SpriteBatch.Draw(drawnImage.Texture, drawnImage.Rectangle, null, Color.White, _rotation, Vector2.One, SpriteEffects.None, 0);
            }
        }

        for (int i = 0; i < DefaultGrid.Rectangles.Count; i++)
        {

            bool isClicked;
            if (i == ClickedIndex && !_isWindowFocused())
                isClicked = true;
            else
                isClicked = false;

            if (isClicked)
            {
                var (width, height) = GetClickedTextureSize();
               
                var coveredColumnCell = (width + CellSize - 1) / CellSize;
                var coveredRowCell = (height + CellSize - 1) / CellSize;
                var currentRow = i / _column; // Swapped the calculation
                var currentColumn = i % _column; // Swapped the calculation

                var texture = new Tile.DrawnTexture()
                {
                    Texture = ClickedTexture(),
                    Rectangle = new Rectangle(currentColumn * CellSize + 2, currentRow * CellSize + 2, width, height),
                };
                
                _isOverlapped = false;

                if (OverlapOption)
                {
                    for (var rowOffset = 0; rowOffset < coveredRowCell; rowOffset++)
                    {
                        for (var colOffset = 0; colOffset < coveredColumnCell; colOffset++)
                        {
                            var coveringIndex = (currentRow + rowOffset) * _column + (currentColumn + colOffset);

                            if (coveringIndex <= DefaultGrid.Textures.Count && (string)DefaultGrid.Textures[coveringIndex].Tag == ClickedTexture().Name)
                            {
                                _isOverlapped = true;
                                break;
                            }
                        }

                        if (_isOverlapped) break;
                    }
                }

                
                if (!_isOverlapped)
                {
                    Globals.SpriteBatch.Draw(ClickedTexture(), DefaultGrid.Rectangles[i], _drawnColor);
                    DrawnTextures.Add(texture);

                    for (var rowOffset = 0; rowOffset < coveredRowCell; rowOffset++)
                    {
                        for (var colOffset = 0; colOffset < coveredColumnCell; colOffset++)
                        {
                            var coveringIndex = (currentRow + rowOffset) * _column + (currentColumn + colOffset);
                            if (coveringIndex < DefaultGrid.Textures.Count)
                            {
                                DefaultGrid.Textures[coveringIndex].Tag = _getClickedTextureName();
                                texture.CoveredIndicesTextureNames[coveringIndex] = _getClickedTextureName();
                            }
                        }
                    }
                }
            }
        }

        ClickedIndex = -1;
        DrawBorders();
    }


    private void DrawBorders()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.LeftAlt)) return;
        _drawnColor = Color.DarkGray;
            foreach (var rectangle in DefaultGrid.Rectangles)
        {
            Globals.SpriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1),
                _drawnColor);
            Globals.SpriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 1),
                _drawnColor);
            Globals.SpriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height),
                _drawnColor);
            Globals.SpriteBatch.Draw(_pixel, new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Height),
                _drawnColor);
        }


           
    }

    public void SetupRectangles()
    {
        for (int i = 0; i < _row; i++)
        {
            for (int j = 0; j < _column; j++)
            {
                int x = j * CellSize;
                int y = i * CellSize;

                Rectangle rect = new Rectangle(x, y, CellSize, CellSize); //10x10
                DefaultGrid.Rectangles.Add(rect);
                Collisions.Add(false);
            }
        }
    }

    public int GetClickedIndex(Vector2 clickPosition)
    {
        if (Globals.CurrentMouse.LeftButton == ButtonState.Pressed)
        {
            //reverse the camera transformation on the click position all of these codes later on after you fixed offset issue and understood main functionality
            var inverseTransform = Matrix.Invert(_camera.GetTransform());
            var transformedClickPosition = Vector2.Transform(clickPosition, inverseTransform);
            for (int i = 0; i < DefaultGrid.Rectangles.Count; i++) //1600
            {
                if (DefaultGrid.Rectangles[i].Contains(transformedClickPosition)) //if each rectangle clicked? 
                {
                    LastClickedIndex = i;
                    return i;
                }
            }
        }
        
        return -1;
    }
    
    private (int width, int height) GetClickedTextureSize()
    {
        var texture = ClickedTexture();
        return (texture.Width, texture.Height);
    }
    

    public class DrawnTexture
    {
        public Texture2D Texture { get; set; }
        public Rectangle Rectangle { get; set; }
        public float Rotation = 0.0f;
        public Dictionary<int, string> CoveredIndicesTextureNames { get; set; }
        
        public DrawnTexture()
        {
            CoveredIndicesTextureNames = new Dictionary<int, string>();
        }
        
    }
    
    public class SerializableTile
    {
        public string TextureName { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Collision { get; set; }
    }

    public void SaveGrid(string filePath)
    {
        List<SerializableTile> serializableTiles = new List<SerializableTile>();

        for (int i = 0; i < DefaultGrid.Textures.Count; i++)
        {
            //get texture of default grid and rectangle for each and assign them to serializable tile
            Texture2D texture = DefaultGrid.Textures[i];
            Rectangle rectangle = DefaultGrid.Rectangles[i];
            
                
            SerializableTile tile = new SerializableTile()
            {
                TextureName = (string)texture.Tag,
                Collision = Collisions[i],
                Rectangle = rectangle
            };
            serializableTiles.Add(tile);
        }
        //convert string value to json variables to be parsed in the future
        string json = JsonConvert.SerializeObject(serializableTiles, Formatting.Indented);
        //create a file in the specified folder with specified data 
        File.WriteAllText(filePath,json);
    }

    // public void LoadGrid(string filePath)
    // {
    //     string json = File.ReadAllText(filePath);
    //     List<SerializableTile> serializableTiles = JsonConvert.DeserializeObject<List<SerializableTile>>(json);
    //
    //     for (int i = 0; i < serializableTiles.Count; i++)
    //     {
    //         SerializableTile tile = serializableTiles[i];
    //         Texture2D texture = LoadTextureByName(tile.TextureName);
    //         Rectangle rectangle = tile.Rectangle;
    //
    //         DefaultGrid.Textures[i] = texture;
    //         DefaultGrid.Rectangles[i] = rectangle;
    //     }
    // }

    private Texture2D LoadTextureByName(string TextureName)
    {
        return new Texture2D(Globals.GraphicsDevice,1,1);
    }
}