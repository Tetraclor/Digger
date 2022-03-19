using Common;
using GameCore;
using SnakeGame2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace WinFormsGameView
{
    public class ImageHelper
    {
        public static Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        public static Dictionary<string, Bitmap> bitmapsForUserSnake = new Dictionary<string, Bitmap>();
        static ImageHelper()
        {
            var imagesDirectory = new DirectoryInfo("../../../Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name.ToLower()] = (Bitmap)Image.FromFile(e.FullName);

            LoadSnakeImageItems();
            foreach (var e in bitmaps.Keys.Where(v => v.Contains("snake")))
            {
                bitmapsForUserSnake[e] = ImageHelper.ColorsChange(bitmaps[e], 0.5f, 0.5f, 2);
            }
        }

        private static void LoadSnakeImageItems()
        {
            // Order matters
            var allDirs = new List<FourDirMove>() { FourDirMove.Left, FourDirMove.Up, FourDirMove.Right, FourDirMove.Down }
                .Select(v => v.ToString().ToLower())
                .ToList();

            var allSnakeSpriteNames = new List<string>();

            foreach (var e in new DirectoryInfo("../../../Images/Snake").GetFiles("*.png"))
            {
                var imagename = e.Name.ToLower().Replace(".png", "");
                var bitmap = (Bitmap)Image.FromFile(e.FullName);

                bitmaps[imagename + ".png"] = bitmap;
                allSnakeSpriteNames.Add(imagename);
            }

            //if (allSnakeSpriteNames.Count > 10) // Негласное правило, означает что преобразования не нужны все уже сохранены
            //    return;

            foreach (var imagename in allSnakeSpriteNames)
            {
                var bitmap = bitmaps[imagename + ".png"];
                var tails = imagename.Split("-");

                _ = tails.Max(v => v.Length);

                
                

                if (tails.Length == 1) continue;

                var spritename = tails[0];
                var fromDir = tails[1];

                var tempToDir = tails.Length == 3 ? tails[2] : null;
                var tempDir = fromDir;

                while (true)
                {
                    var newimagename = $"{spritename}-{tempDir}";
                    if (tempToDir != null) newimagename += $"-{tempToDir}";

                    SaveImage(newimagename, bitmap);

                    if (tempToDir != null)
                    {
                        var flipimagename = $"{spritename}-{tempToDir}-{tempDir}";
                        SaveImage(flipimagename, bitmap);
                        tempToDir = NextRotateDir(tempToDir);
                    }
                    tempDir = NextRotateDir(tempDir);
                    bitmap = (Bitmap)bitmap.Clone();
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

                    if (tempDir == fromDir)
                        break;
                }
            }

            void SaveImage(string imagename, Bitmap bitmap)
            {
                imagename += ".png";
                if (bitmaps.ContainsKey(imagename) == false)
                {
                    bitmaps[imagename] = bitmap;
                    bitmap.Save($"../../../Images/Snake/{imagename}");
                }

            }

            string NextRotateDir(string dir)
            {
                var index = allDirs.IndexOf(dir) + 1;
                return allDirs[index % allDirs.Count];
            }
        }


        public static Bitmap ColorsChange(Bitmap image, float red, float green, float blue)
        {
            ImageAttributes imageAttributes = new();
            int width = image.Width;
            int height = image.Height;

            float[][] colorMatrixElements = {
                    new float[] {red, 0, 0, 0, 0},        // red scaling factor of 2
                    new float[] {0, green,  0,  0, 0},        // green scaling factor of 1
                    new float[] {0,  0, blue,  0, 0},        // blue scaling factor of 1
                    new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                    new float[] {0,  0,  0,  0, 1}};    // three translations of 0.2

            ColorMatrix colorMatrix = new(colorMatrixElements);

            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);

            var newImage = new Bitmap(width, height);
            var g = Graphics.FromImage(newImage);
            g.DrawImage(
               image,
               new Rectangle(0, 0, width, height),  // destination rectangle
               0, 0,        // upper-left corner of source rectangle
               width,       // width of source rectangle
               height,      // height of source rectangle
               GraphicsUnit.Pixel,
               imageAttributes);

            return newImage;
        }
    }

    public class SnakeAnimationManager : IAnimationManager
    {
        public static Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        public static Dictionary<string, Bitmap> bitmapsForUserSnake = new Dictionary<string, Bitmap>();
        public int SpriteSize { get; set; } = 32;

        public ICreature[,] Map;
        public List<GameCore.Point> UserSnake = new();
        public List<List<GameCore.Point>> OtherSnakes = new();

        public Action<SnakeAnimationManager> Updater;

        public SnakeAnimationManager(
            Action<SnakeAnimationManager> Updater)
        {
            this.Updater = Updater;
            bitmaps = ImageHelper.bitmaps;
            bitmapsForUserSnake = ImageHelper.bitmapsForUserSnake;
        }


        CommonImageNameGetter imageNameGetter = new();

        public void Draw(Graphics graphics)
        {
            Updater(this);
            var map = Map;
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var creature = map[j, i];
                    if (creature == null) continue;
                    if (creature is BodySnake || creature is HeadSnake)
                        continue;

                    var sprite = bitmaps[imageNameGetter.Get((dynamic)creature).ToLower()];
                    graphics.DrawImage(sprite, j * SpriteSize, i * SpriteSize);
                }
            }
            var customBitmaps = bitmapsForUserSnake;
            DrawSnake(UserSnake, DrawSprite);
            customBitmaps = bitmaps;
            OtherSnakes.ForEach(v => DrawSnake(v, DrawSprite));


            void DrawSprite(string imagename, GameCore.Point point)
            {
                if (imagename.Contains("None")) return; // Костыль
                var sprite = customBitmaps[imagename.ToLower() + ".png"];
                graphics.DrawImage(sprite, point.X * SpriteSize, point.Y * SpriteSize);
            }
        }

        public void DrawSnake(List<GameCore.Point> snakePoints, Action<string, GameCore.Point> draw)
        {
            if(snakePoints == null || snakePoints.Count == 0) return;

            var head = snakePoints.First();
            if(snakePoints.Count == 1)
            {
                draw("HeadSnake", head);
                return;
            }
  
            var tail = snakePoints.Last();
            draw($"HeadSnake-{head.ToDir(snakePoints[1])}", head);
            draw($"BodySnake-{tail.ToDir(snakePoints[^2])}", tail);

            for (int i = 1; i < snakePoints.Count - 1; i++)
            {
                var prevPoint = snakePoints[i - 1];
                var bodyPoint = snakePoints[i];
                var nextPoint = snakePoints[i + 1];

                var fromDir =  bodyPoint.ToDir(prevPoint);
                var toDir = bodyPoint.ToDir(nextPoint);

                draw($"BodySnake-{fromDir}-{toDir}", bodyPoint);
            }
        }

        public void Tick(int mapWitdh, int mapHeight)
        {
            
        }
    }
}