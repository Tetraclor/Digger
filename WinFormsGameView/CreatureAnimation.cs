using GameCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace WinFormsGameView
{
    public interface IAnimationManager
    {
        int SpriteSize { get; set; }
        void Draw(Graphics graphics);
        void Tick(int mapWitdh, int mapHeight);
    }

    public class MapAnimationManager : IAnimationManager
    {
        public static Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private readonly Func<ICreature[,]> getMap;

        public int SpriteSize { get; set; } = 32;

        public MapAnimationManager(Func<ICreature[,]> GetMap)
        {
            getMap = GetMap;

            var imagesDirectory = new DirectoryInfo("../../../Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
        }

        CommonImageNameGetter imageNameGetter = new();

        public void Draw(Graphics graphics)
        {
           // graphics.ScaleTransform(1/(float)SpriteSize, 1/(float)SpriteSize);

            var map = getMap();
            var w = map.GetLength(0); 
            var h = map.GetLength(1);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var creature = map[j, i];
                    if (creature == null) continue;
                    var sprite = bitmaps[imageNameGetter.Get((dynamic)creature)];
                    graphics.DrawImage(sprite, j * SpriteSize, i * SpriteSize);
                }
            }
        }

        public void Tick(int mapWitdh, int mapHeight)
        {
            
        }
    }
}