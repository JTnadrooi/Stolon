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
using MonoGame.Extended.Content;

using static Stolon.StolonGame;
using RectangleF = MonoGame.Extended.RectangleF;
using System.Reflection;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using System.Threading.Tasks;
using AsitLib.FormConsole;
using System.Collections.Concurrent;

#nullable enable


namespace Stolon
{
    public class StolonEntity : SLEntity
    {
        private StolonComputer com;
        public override string? Description => "Feijoa sellowiana also known as Acca sellowiana (O.Berg) Burret, is a species of flowering plant in the myrtle family," +
            " Myrtaceae. " + "Feijoa sellowiana also known as Acca sellowiana (O.Berg) Burret, is a species of flowering plant in the myrtle family," +
            " Myrtaceae.";

        public StolonEntity() : base("stolon", "Stolon", "Sl")
        {
            com = new StolonComputer(this);
        }

        public override SLComputer Computer => com;

        public override Texture2D Splash => Instance.Textures.GetReference("textures\\splash\\stolon");

        public override DialogueInfo GetReaction(PrimitiveReactOption reactOption) => throw new NotImplementedException();
    }
    public class StolonComputer : SLComputer
    {
        public StolonComputer(StolonEntity source) : base(source)
        {

        }

        public override void DoMove(Board board)
        {
            throw new NotImplementedException();
        }
    }
}
