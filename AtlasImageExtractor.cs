using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace imguiTut;

public static class AtlasImageExtractor
{

    public static List<Rectangle> ExtractRectanglesFromAtlas(Texture2D textureAtlas)
    {
        List<Rectangle> rectangles = new List<Rectangle>();
        Color[] pixelData = new Color[textureAtlas.Width * textureAtlas.Height];
        textureAtlas.GetData(pixelData);

        bool[,] visited = new bool[textureAtlas.Width, textureAtlas.Height];

        for (int y = 0; y < textureAtlas.Height; y++)
        {
            for (int x = 0; x < textureAtlas.Width; x++)
            {
                if (!visited[x, y] && pixelData[y * textureAtlas.Width + x].A != 0)
                {
                    Rectangle rect = FloodFill(pixelData, visited, x, y, textureAtlas.Width, textureAtlas.Height);
                    rectangles.Add(rect);
                }
            }
        }

        return rectangles;
    }

    private static Rectangle FloodFill(Color[] pixelData, bool[,] visited, int x, int y, int width, int height)
    {
        int left = x;
        int right = x;
        int top = y;
        int bottom = y;

        Stack<(int, int)> stack = new Stack<(int, int)>();
        stack.Push((x, y));

        while (stack.Count > 0)
        {
            (int currentX, int currentY) = stack.Pop();
            if (currentX < 0 || currentY < 0 || currentX >= width || currentY >= height) continue;
            if (visited[currentX, currentY]) continue;
            if (pixelData[currentY * width + currentX].A == 0) continue;

            visited[currentX, currentY] = true;

            left = Math.Min(left, currentX);
            right = Math.Max(right, currentX);
            top = Math.Min(top, currentY);
            bottom = Math.Max(bottom, currentY);

            stack.Push((currentX - 1, currentY));
            stack.Push((currentX + 1, currentY));
            stack.Push((currentX, currentY - 1));
            stack.Push((currentX, currentY + 1));
        }

        return new Rectangle(left, top, right - left + 1, bottom - top + 1);
    }

    

}