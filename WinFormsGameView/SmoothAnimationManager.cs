using GameCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace WinFormsGameView
{
    public class SmoothAnimationManager : IAnimationManager
    {
        public static Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();

        List<CreatureAnimation> Animations = new();
        Func<List<CreatureTransformation>> GetCreatureTransformations;

        public int AnimationTickLength = 32;
        public int AnimationTickCount = 0;
        public int SpriteSize { get; set; } = 32;
        public int AnimationDelta { private set; get; }

        public SmoothAnimationManager(int animationLength, Func<List<CreatureTransformation>> getCreatureTransformations)
        {
            AnimationTickLength = animationLength;
            GetCreatureTransformations = getCreatureTransformations;

            var imagesDirectory = new DirectoryInfo("../../../Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
        }

        public void Draw(Graphics graphics)
        {
            for (int i = 0; i < Animations.Count; i++)
            {
                var a = Animations[i];
                graphics.DrawImage(a.Sprite, a.DrawLocation);
            }
        }

        public void Tick(int mapWitdh, int mapHeight)
        {
            if (AnimationTickCount == 0)
            {
                Animations = GetCreatureTransformations()
                    .Select(v => new CreatureAnimation(v))
                    .ToList();
            }

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

        class CreatureAnimation
        {
            private CommonImageNameGetter ImageNameGetter = new();

            public Bitmap Sprite;
            public System.Drawing.Point DrawLocation;

            public GameCore.Point Location;
            public int DeltaX;
            public int DeltaY;

            public CreatureAnimation(CreatureTransformation transformation)
            {
                Sprite = bitmaps[ImageNameGetter.Get((dynamic)transformation.Creature)];
                DeltaX = transformation.Command.DeltaX;
                DeltaY = transformation.Command.DeltaY;
                Location = transformation.Location;
            }

            public CreatureAnimation()
            {
            }
        }
    }
}