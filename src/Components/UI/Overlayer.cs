using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsitLib.XNA;
using Betwixt;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;

#nullable enable

namespace Stolon
{
    public class SLOverlayer : AxComponent
    {
        private Dictionary<string, IOverlay> overlays;
        private List<string> initialized;


        public SLOverlayer() : base(SLEnvironment.Instance)
        {
            overlays = new Dictionary<string, IOverlay>();
            initialized = new List<string>();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            Instance.DebugStream.WriteLine("\t[s]adding overlay of id " + overlay.ID + ".");
            overlays.Add(overlay.ID, overlay);
            Instance.DebugStream.Succes(2);
        }

        public void RemoveOverlay(string overlayId)
        {
            Instance.DebugStream.WriteLine("\t[s]removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            overlays.Remove(overlayId);
        }

        public void Activate(string overlayId, params object?[] args)
        {
            Instance.DebugStream.WriteLine("\t[s]activating overlay of id " + overlayId + ".");
            overlays[overlayId].Initialize(this, args);
            initialized.Add(overlayId);
        }

        public bool IsActive(IOverlay overlay) => IsActive(overlay.ID);
        public bool IsActive(string overlayId)
        {
            return initialized.Contains(overlayId);
        }

        public void Deactivate(string overlayId) // ensure
        {
            Instance.DebugStream.WriteLine("\t[s]deactivating overlay of id " + overlayId + ".");
            if (initialized.Contains(overlayId))
            {
                overlays[overlayId].Reset();
            }
            initialized.Remove(overlayId);
        }

        public override void Update(int elapsedMiliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < initialized.Count; i++) // for all initialized overlays
            {
                overlay = overlays[initialized[i]];
                overlay.Update(elapsedMiliseconds);
                if (overlay.Ended)
                {
                    Instance.DebugStream.WriteLine("\t[s]deactivating and resetting ended overlay of id " + overlay.ID + ".");
                    Deactivate(overlay.ID);
                }
            }
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            for (int i = 0; i < initialized.Count; i++)
            {
                overlays[initialized[i]].Draw(spriteBatch, elapsedMiliseconds);
            }
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }

    public interface IOverlay
    {
        public void Initialize(SLOverlayer overlayer, params object?[] args);
        public void Update(int elapsedMiliseconds);
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds);
        public void Reset();

        public string ID { get; }
        public bool Ended { get; }
    }
    public class LoadOverlay : IOverlay
    {
        public string ID => "loading";
        public bool Ended { get; private set; }

        private Texture2D lineTexture;

        private float rotation;
        private float rotationSpeed;
        private Vector2 pos;
        private float scale;

        public LoadOverlay()
        {
            lineTexture = Instance.Textures.GetReference("textures\\loading1");
            rotation = 0f;
            scale = 0.20f;
            rotationSpeed = 40f;

            pos = Instance.VirtualBounds.Size.ToVector2() + new Vector2(-lineTexture.Width, -lineTexture.Height) * scale;

        }

        public void Initialize(SLOverlayer overlayer, params object?[] args)
        {
            pos = (Vector2)((args.Length > 0 ? args[0] : null) ?? pos);
        }

        public void Reset()
        {

        }

        public void Update(int elapsedMiliseconds)
        {
            rotation += rotationSpeed;
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(lineTexture, pos, null, Color.White, rotation / 360f, new Vector2(lineTexture.Width / 2f, lineTexture.Height / 2f), scale, SpriteEffects.None, 0);
            //spriteBatch.DrawCircle(pos, scale * lineTexture.Width * 0.8f, 15, Color.White, 2);
        }
    }
    public class TransitionOverlay : IOverlay
    {
        public string ID => "transition";

        public bool Ended { get; private set; }

        private Rectangle area;
        private SLOverlayer overlayer;
        private Tweener<float> tweener;
        private string text;

        private Rectangle drawArea;
        private float heightCoefficient;
        private Vector2 textPos;
        private Action action;

        private bool hasHitMax;

        public TransitionOverlay()
        {
            area = Rectangle.Empty;
            drawArea = Rectangle.Empty;
            overlayer = null!; // I know I know
            tweener = null!; // yupyup
            Ended = false;
            text = string.Empty;
            textPos = Vector2.Zero;
            action = () => { };
        }

        public void Update(int elapsedMiliseconds)
        {
            int desiredHeight = area.Height;
            if (!hasHitMax && heightCoefficient > 0.999f)
            {
                tweener = new Tweener<float>(1f, 0f, Duration / 1000f / 2, Ease.Sine.In);
                action();
                tweener.Start();
                hasHitMax = true;
            }
            Ended = hasHitMax && heightCoefficient < 0.001f;

            tweener.Update(elapsedMiliseconds / 1000f);

            heightCoefficient = tweener.Value;

            drawArea = new Rectangle(area.Location, new Point(area.Width, (int)(desiredHeight * heightCoefficient)));
            textPos = Centering.MiddleXY(SLEnvironment.Font.MeasureString(text).ToPoint(), drawArea, new Vector2(SLEnvironment.FontScale) * TextSizeMod);
            textPos = new Vector2(textPos.X, Math.Min(textPos.Y, drawArea.Height - Instance.Environment.FontDimensions.Y * TextSizeMod));

            Centering.OnPixel(ref textPos);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(Instance.Textures.Pixel, drawArea, Color.Black);
            spriteBatch.DrawRectangle(drawArea, Color.White);
            spriteBatch.DrawString(SLEnvironment.Font, text, textPos, Color.White, 0f, Vector2.Zero, SLEnvironment.FontScale * TextSizeMod, SpriteEffects.None, 0f);
        }

        public void Initialize(SLOverlayer overlayer, params object?[] args)
        {
            Ended = false;
            tweener = new Tweener<float>(0f, 1f, Duration / 1000f / 2, Ease.Sine.Out);
            area = (Rectangle)((args.Length > 0 ? args[0] : null) ?? Instance.VirtualBounds);
            text = (string)(args[2] ?? string.Empty);
            action = (Action)(args[1]! ?? action);

            tweener.Start();

            this.overlayer = overlayer;
        }

        public void Reset()
        {
            tweener.Stop();
            tweener.Reset();
            hasHitMax = false;
            heightCoefficient = 0f;
        }

        public static int Duration => 4000;
        public static float TextSizeMod => 3;
    }
}
