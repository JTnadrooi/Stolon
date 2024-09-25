using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib.XNA;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System;
using System.Runtime.Versioning;
using System.Reflection.Metadata;
using AsitLib;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Diagnostics;
using System.Collections;
using MonoGame.Extended;
using AsitLib.Collections;

using static Stolon.StolonGame;
using RectangleF = MonoGame.Extended.RectangleF;
#nullable enable

namespace Stolon
{
    public class SLScene : AxComponent
    {
        public Board Board => board;

        private Board board;

        public SLScene(Player[] players) : base(Instance.Environment)
        {
            board = new Board(this, new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        }

        public override void Update(int elapsedMiliseconds)
        {
            Board.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }

        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            Board.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}
