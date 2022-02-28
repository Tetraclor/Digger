using GameCore;
using GameDigger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsGameView
{
    public class DiggerWindow : Form
    {
        private readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private readonly Game game;
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private int tickCount;

        public static int ElementSize = 32;

        private readonly GameState GameState;

        private const string mapWithPlayerTerrain = @"
TTT T
TTP T
T T T
TT TT";

        private const string bigmapWithPlayerTerrain = @"
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T";

        private const string mapWithPlayerTerrainSackGold = @"
PTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

        private const string mapWithPlayerTerrainSackGoldMonster = @"
PTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT";

        public DiggerWindow(DirectoryInfo imagesDirectory = null)
        {
            GameState = new GameState();
            GameState.Map = CreatureMapCreator.CreateMap(mapWithPlayerTerrain);
            
            game = new Game(GameState);
            ClientSize = new Size(
                ElementSize * GameState.MapWidth,
                ElementSize * GameState.MapHeight + ElementSize);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            if (imagesDirectory == null)
                imagesDirectory = new DirectoryInfo("Images");
            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap) Image.FromFile(e.FullName);
            var timer = new Timer();
            timer.Interval = 15;
            timer.Tick += TimerTick;
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Digger";
            DoubleBuffered = true;
        }

        DiggerMove KeyToMoveCommand(Keys key)
        {
            return key switch
            {
                Keys.Left => DiggerMove.Left,
                Keys.Right => DiggerMove.Rigth,
                Keys.Up => DiggerMove.Up,
                Keys.Down => DiggerMove.Down,
                _ => DiggerMove.None,
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            PlayerCommand.Move = KeyToMoveCommand(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            PlayerCommand.Move = KeyToMoveCommand(pressedKeys.Any() ? pressedKeys.Min() : Keys.None);
        }

        List<GameCore.Point> drawPoints = new List<GameCore.Point>();

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(0, ElementSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, ElementSize * GameState.MapWidth, ElementSize * GameState.MapHeight);

            for (int i = 0; i < game.Animations.Count; i++)
            {
                var a = game.Animations[i];
                var sprite = bitmaps[CreatureImageNameGetter.Get((dynamic)a.Creature)];
                var drawLocation = drawPoints[i];
                e.Graphics.DrawImage(sprite, drawLocation.X, drawLocation.Y);
            }

            e.Graphics.ResetTransform();
            e.Graphics.DrawString(GameState.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }

        private void TimerTick(object sender, EventArgs args)
        {
            if (tickCount == 0)
                game.BeginAct();

            var size = ElementSize;
            var d = 4 * (tickCount + 1);
            drawPoints.Clear();

            foreach (var a in game.Animations)
            {
                var temp = new GameCore.Point(a.Location.X * size + d * a.Command.DeltaX, a.Location.Y * size + d * a.Command.DeltaY);
                drawPoints.Add(temp);
            }

            if (tickCount == 7)
                game.EndAct();

            tickCount++;
            if (tickCount == 8) tickCount = 0;

            Invalidate();
        }
    }

    static class CreatureImageNameGetter
    {
        public static string Get(Terrain terrain) => "Terrain.png";
        public static string Get(Digger digger) => "Digger.png";
    }
}