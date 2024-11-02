using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using AsitLib;
using static Stolon.StolonGame;

using Math = System.Math;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable enable

namespace Stolon
{
    public class StoEntity : EntityBase
    {
        public override SLComputer Computer => null;
        public override Texture2D Splash => Instance.Textures.GetReference("textures\\splash\\goldsilk"); // unrelevant in first ver
        public override string? Description => "This also shoulden't be readable in the current verion.";

        public StoEntity() : base("sto", "Sto", "St")
        {

        }

        public override DialogueInfo GetReaction(PrimitiveReactOption reactOption)
        {
            return new DialogueInfo(this, "");
        }
    }
}