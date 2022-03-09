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

        private readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private int animationTickCount;

        private UserPlayer userPlayer = new UserPlayer();
        private List<CreatureTransformation> Transformations = new List<CreatureTransformation>();
        private int MapWidth;
        private int MapHeight;

        public GameWindow()
        {
            InitLocalGame();
            InitForm(MapWidth, MapHeight);
            StartGame(LocalGameTimerTick);
        }

        void InitForm(int MapWidth, int MapHeight, DirectoryInfo imagesDirectory = null) 
        {
            ClientSize = new Size(
                    ElementSize * MapWidth,
                    ElementSize * MapHeight + ElementSize);

            FormBorderStyle = FormBorderStyle.FixedDialog;
            if (imagesDirectory == null)
                imagesDirectory = new DirectoryInfo("../../../Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
        }

        void InitLocalGame()
        {
            MapWidth = gameService.GameState.MapWidth;
            MapHeight = gameService.GameState.MapHeight; 

            var bot = ListBotPlayer.DownLeft;
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

        List<GameCore.Point> drawPoints = new List<GameCore.Point>();

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(0, ElementSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, ElementSize * MapWidth, ElementSize * MapHeight);

            for (int i = 0; i < Transformations.Count; i++)
            {
                var a = Transformations[i];
                var sprite = bitmaps[CreatureImageNameGetter.Get((dynamic)a.Creature)];
                var drawLocation = drawPoints[i];
                e.Graphics.DrawImage(sprite, drawLocation.X, drawLocation.Y);
            }

            e.Graphics.ResetTransform();
           // e.Graphics.DrawString(GameState.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }

        int animationTickLength = 32;

        private void MapToAnimation()
        {
            var size = ElementSize;
            var d = (size / animationTickLength) * (animationTickCount + 1);
            drawPoints.Clear();

            foreach (var a in Transformations)
            {
                var temp = new GameCore.Point(a.Location.X * size + d * a.Command.DeltaX, a.Location.Y * size + d * a.Command.DeltaY);
                drawPoints.Add(temp);
            }
        }

        private void LocalGameTimerTick(object sender, EventArgs args)
        {
            if (animationTickCount == 0)
                gameService.MakeGameTick();

            Transformations = gameService.Game.Animations;
            MapToAnimation();

            animationTickCount++;
            if (animationTickCount == animationTickLength) animationTickCount = 0;

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