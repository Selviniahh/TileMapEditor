using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;

namespace imguiTut
{
    public class SetCollisionsWindow
    {
        public bool SetCollisions = false;
        private Tile _tile;

        public SetCollisionsWindow(Tile tile)
        {
            _tile = tile;
        }

        public void DrawWindow()
        {
            ImGui.SetNextWindowPos(new Num.Vector2(0,0));
            ImGui.SetNextWindowSize(new Num.Vector2(250,60));

            ImGui.Begin("Set Collisions Window");

            if (ImGui.Button("SetCollisions"))
            {
                SetCollisions = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("StopCollisions"))
            {
                SetCollisions = false;
            }

            ImGui.End();
        }

        public void Update()
        {
            if (SetCollisions && Globals.CurrentMouse.LeftButton == ButtonState.Pressed)
            {
                Vector2 clickPosition = new Vector2(Globals.CurrentMouse.X, Globals.CurrentMouse.Y);
                int clickedIndex = _tile.GetClickedIndex(clickPosition);
                if (clickedIndex != -1)
                {
                    _tile.Collisions[clickedIndex] = true;
                }
            }
        }
    }
}