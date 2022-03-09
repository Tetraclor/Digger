using Common;
using GameCore;
using GameDigger;
using GameSnake;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WinFormsGameView
{
    public class GameWindow : Form
    {
        private static int ElementSize = 32;
       // private GameService gameService = new DiggerGameService(DiggerGameService.mapWithPlayerTerrain);
        private GameService gameService = new SnakeGameService(10, 10);


        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();


        private UserPlayer userPlayer = new UserPlayer();

        private int MapWidth;
        private int MapHeight;

        public GameWindow()
        {
            InitLocalGame();
            InitForm(MapWidth, MapHeight);
            StartGame(LocalGameTimerTick);
        }

        void InitForm(int MapWidth, int MapHeight) 
        {
            ClientSize = new Size(
                    ElementSize * MapWidth,
                    ElementSize * MapHeight + ElementSize);

            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        void InitLocalGame()
        {
            MapWidth = gameService.GameState.MapWidth;
            MapHeight = gameService.GameState.MapHeight; 

            var bot = ListBotPlayer.DownLeft;
            var randomBot = new RandomBotPlayer();
            gameService.AddPlayer(userPlayer);
        }

        void StartGame(Action<object, EventArgs> action)
        {
            var timer = new Timer();
            timer.Interval = 15;
            timer.Tick += (o, e) => action(o, e);
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Digger";
            DoubleBuffered = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            userPlayer.PressedKey = e.KeyCode;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            userPlayer.PressedKey = pressedKeys.Any() ? pressedKeys.Min() : Keys.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(0, ElementSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, ElementSize * MapWidth, ElementSize * MapHeight);

            for (int i = 0; i < CreatureAnimation.Animations.Count; i++)
            {
                var a = CreatureAnimation.Animations[i];
                e.Graphics.DrawImage(a.Sprite, a.DrawLocation);
            }

            e.Graphics.ResetTransform();
           // e.Graphics.DrawString(GameState.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }

        public class CreatureAnimation
        {
            public static readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();

            public static List<CreatureAnimation> Animations = new();

            public static int AnimationTickLength = 32;
            public static int AnimationTickCount = 0;
            public static int SpriteSize = ElementSize;

            public static int AnimationDelta { private set; get; }

            static CreatureAnimation()
            {
                var imagesDirectory = new DirectoryInfo("../../../Images");

                foreach (var e in imagesDirectory.GetFiles("*.png"))
                    bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
            }


            public Bitmap Sprite;
            public System.Drawing.Point DrawLocation;

            public GameCore.Point Location;
            public int DeltaX;
            public int DeltaY;

            public CreatureAnimation(CreatureTransformation transformation)
            {
                Sprite = bitmaps[CreatureImageNameGetter.Get((dynamic)transformation.Creature)];
                DeltaX = transformation.Command.DeltaX;
                DeltaY = transformation.Command.DeltaY;
                Location = transformation.Location;
            }

            public static void Tick(int mapWitdh, int mapHeight)
            {
                AnimationTickCount++;
                AnimationDelta = (ElementSize / AnimationTickLength) * (AnimationTickCount + 1);

                foreach (var animation in Animations)
                {
                    var location = animation.Location;
                    var deltaX = animation.DeltaX;
                    var deltaY = animation.DeltaY;

                    var temp = new System.Drawing.Point(
                        location.X * SpriteSize + AnimationDelta * deltaX,
                        location.Y * SpriteSize + AnimationDelta * deltaY);

                    if (Math.Abs(deltaX) == mapWitdh - 1 ||
                        Math.Abs(deltaY) == mapHeight - 1) //Телепортация с конца на другой конец
                    {
                        var xs = -Math.Sign(deltaX);
                        var ys = -Math.Sign(deltaY);

                        temp = new System.Drawing.Point(
                            location.X * SpriteSize + AnimationDelta * xs, 
                            location.Y * SpriteSize + AnimationDelta * ys);
                    }

                    animation.DrawLocation = temp;
                }

                if (AnimationTickCount == AnimationTickLength) AnimationTickCount = 0;
            }
        }

        private void LocalGameTimerTick(object sender, EventArgs args)
        {
            if (CreatureAnimation.AnimationTickCount == 0)
            {
                gameService.MakeGameTick();

                CreatureAnimation.Animations = gameService.Game.Animations
                    .Select(v => new CreatureAnimation(v))
                    .ToList();
            }
               
            CreatureAnimation.Tick(gameService.GameState.MapWidth, gameService.GameState.MapHeight);

            Invalidate();
        }
    }

    static class CreatureImageNameGetter
    {
        public static string Get(Terrain _) => "Terrain.png";
        public static string Get(Digger _) => "Digger.png";
        public static string Get(HeadSnake _) => "HeadSnake.png";
        public static string Get(BodySnake _) => "BodySnake.png";
        public static string Get(Apple _) => "Apple.png";
    }

    public class UserPlayer : IPlayer
    {
        public Keys PressedKey;

        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            return new PlayerCommand() { Move = KeyToMoveCommand(PressedKey) };
        }

        FourDirMove KeyToMoveCommand(Keys key)
        {
            return key switch
            {
                Keys.Left => FourDirMove.Left,
                Keys.Right => FourDirMove.Right,
                Keys.Up => FourDirMove.Up,
                Keys.Down => FourDirMove.Down,
                _ => FourDirMove.None,
            };
        }
    }
}