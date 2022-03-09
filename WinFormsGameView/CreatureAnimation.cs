using GameCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace WinFormsGameView
{
    public class CreatureAnimation
    {
        public static readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();

        public static List<CreatureAnimation> Animations = new();

        public static int AnimationTickLength = 32;
        public static int AnimationTickCount = 0;
        public static int SpriteSize = 32;

        public static int AnimationDelta { private set; get; }

        static CreatureAnimation()
        {
            var imagesDirectory = new DirectoryInfo("../../../Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
        }


        public Bitmap Sprite;
        public System.Drawing.Point DrawLocation;

        private GameCore.Point Location;
        private int DeltaX;
        private int DeltaY;

        public CreatureAnimation(CreatureTransformation transformation)
        {
            Sprite = bitmaps[CreatureImageNameGetter.Get((dynamic)transformation.Creature)];
            DeltaX = transformation.Command.DeltaX;
            DeltaY = transformation.Command.DeltaY;
            Location = transformation.Location;
        }

        private CreatureAnimation()
        {
        }

        public static void Tick(int mapWitdh, int mapHeight)
        {
            AnimationTickCount++;
            AnimationDelta = (SpriteSize / AnimationTickLength) * (AnimationTickCount + 1);

            for (int i = 0; i < Animations.Count; i++)
            {
                var animation = Animations[i];

                var deltaX = animation.DeltaX;
                var deltaY = animation.DeltaY;
                var location = animation.Location;

                if (Math.Abs(deltaX) == mapWitdh - 1 ||
                    Math.Abs(deltaY) == mapHeight - 1) //Телепортация с конца карты на другой конец
                {
                    deltaX = -Math.Sign(deltaX);
                    deltaY = -Math.Sign(deltaY);

                    var fakeAnimation = new CreatureAnimation()
                    {
                        Sprite = animation.Sprite,
                        Location = new GameCore.Point(
                            location.X + animation.DeltaX - deltaX, 
                            location.Y + animation.DeltaY - deltaY),
                        DeltaX = deltaX,
                        DeltaY = deltaY,
                    };

                    Animations.Add(fakeAnimation);
                }

                var temp = new System.Drawing.Point(
                    location.X * SpriteSize + AnimationDelta * deltaX,
                    location.Y * SpriteSize + AnimationDelta * deltaY);

                animation.DrawLocation = temp;
            }

            if (AnimationTickCount == AnimationTickLength) AnimationTickCount = 0;
        }
    }
}