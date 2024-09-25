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
using static Stolon.UIElement;
using MonoGame.Extended.BitmapFonts;
using Betwixt;
using System.Text;

#nullable enable

namespace Stolon
{
    public class SLTextframe : AxComponent
    {
        private Queue<DialogueInfo> dialogueQueue;
        private Rectangle dialoguebounds;
        private DialogueInfo? currentDialogue;
        private Point dialogueTextPos;
        private int initialDialogueMiliseconds;
        private int dialogueMilisecondsRemaining;
        private string toDrawDialogueText;

        private Point providerTextPos;
        private float providerTextScaleCoefficient;
        private Tweener<float>? providerTextSizeTweener;

        private float dialogueShowCoefficient;
        private bool awaitingMouseDialogueHover;
        private bool dialogueIsHidden;
        public bool DialogueIsHidden
        {
            get => dialogueIsHidden;
            set => dialogueIsHidden = value;
        }

        public Rectangle DialogueBounds => dialoguebounds;
        private const int charReadMiliseconds = 75; // per char
        private const int postReadMiliseconds = charReadMiliseconds * 10; // how long the dialogue stagnates after its finished.

        private SLUserInterface userInterface;

        public SLTextframe(SLUserInterface userInterface)
        {
            dialogueQueue = new Queue<DialogueInfo>();
            dialogueTextPos = Point.Zero;
            dialogueMilisecondsRemaining = 0;
            currentDialogue = null;
            dialogueShowCoefficient = 0f;
            toDrawDialogueText = string.Empty;
            this.userInterface = userInterface;
        }
        public void Queue(DialogueInfo[] dialogue)
        {
            for (int i = 0; i < dialogue.Length; i++)
            {
                Queue(dialogue[i]);
            }
        }
        public void Queue(DialogueInfo dialogue)
        {
            Instance.DebugStream.WriteLine("Dialogue queued with text: " + dialogue.Text);
            dialogueQueue.Enqueue(dialogue);
        }

        public void Next(bool awaitingMouseDialogueHover = true)
        {
            Instance.DebugStream.WriteLine("Next dialogue has been requested.");
            if (dialogueQueue.Count == 0) throw new Exception();

            Instance.DebugStream.WriteLine("\tAttempting dequeuing of dialogue with text: " + dialogueQueue.Peek().Text);
            bool providerDiffers = currentDialogue.HasValue && currentDialogue.Value.Provider.Name != dialogueQueue.Peek().Provider.Name;
            if (providerDiffers) Instance.DebugStream.WriteLine("\tDialogue has new provider of name: " + dialogueQueue.Peek().Provider.Name);

            currentDialogue = dialogueQueue.Dequeue();

            initialDialogueMiliseconds = GetMilisecondsFromText(currentDialogue.Value.Text);
            dialogueMilisecondsRemaining = initialDialogueMiliseconds;

            if (providerDiffers || providerTextSizeTweener == null) // initialize or refresh.
            {
                providerTextSizeTweener = new Tweener<float>(0.0001f, 1f, 1, Ease.Sine.Out); // 0 does not work with size calculations..
                providerTextSizeTweener.Start();
            }

            this.awaitingMouseDialogueHover = awaitingMouseDialogueHover;

            Instance.DebugStream.Succes(1);
        }

        public void Queue(int count, Func<string, int, string>? selector = null)
        {
            Instance.DebugStream.WriteLine("Mass queueing a stream of size: " + count);
            selector = selector ?? new Func<string, int, string>((s, i) => s);
            for (int i = 0; i < count; i++) Queue(new DialogueInfo(SLEnvironment.Instance, selector.Invoke(Instance.UserInterface.GetRandomSplashText(), i)));
            Instance.DebugStream.Succes(1);
        }

        public override void Update(int elapsedMiliseconds)
        {
            Point dialogueBoxDimensions = new Point(256, 64);
            int dialogueYoffset = (int)(-10f * (dialogueIsHidden ? dialogueShowCoefficient : 1f));
            bool textFrameGoUp = false;
            dialoguebounds = new Rectangle(
                (int)(Instance.VirtualBounds.Width * 0.5f - dialogueBoxDimensions.X * 0.5f),
                (int)(Instance.VirtualBounds.Height - (dialogueBoxDimensions.Y * dialogueShowCoefficient) + dialogueYoffset),
                dialogueBoxDimensions.X,
                dialogueBoxDimensions.Y);

            dialogueMilisecondsRemaining -= elapsedMiliseconds;

            if (dialogueMilisecondsRemaining < -postReadMiliseconds) // this runs 
            {
                if (dialogueQueue.Count > 0)
                {
                    Next();
                }
                else
                {
                    Instance.DebugStream.WriteLine("No further dialogue found, awaiting further dialogue.");
                    dialogueMilisecondsRemaining = 0; // this is done to prevent this variable going to far into the negatives (not like I need to worry about that but still).
                }
            }

            if (currentDialogue.HasValue) // dialogue is here!
            {
                if (currentDialogue.Value.Text.Length == 0) throw new Exception("Text size zero.");

                providerTextSizeTweener!.Update(elapsedMiliseconds / 1000f);
                providerTextScaleCoefficient = MathF.Min(providerTextSizeTweener.Value, dialogueShowCoefficient > 0.9f ? 1f : dialogueShowCoefficient);

                toDrawDialogueText = dialogueMilisecondsRemaining < 0 ? currentDialogue.Value.Text :
                    currentDialogue.Value.Text[0..(int)MathF.Ceiling(currentDialogue.Value.Text.Length * ((initialDialogueMiliseconds - dialogueMilisecondsRemaining) / (float)initialDialogueMiliseconds))];

                dialogueTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - SLEnvironment.Font.MeasureString(toDrawDialogueText).X * SLEnvironment.FontScale / 2f),
                    (int)(dialoguebounds.Height / 2f - Instance.Environment.FontDimensions.Y / 2f));

                providerTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - SLEnvironment.Font.MeasureString(currentDialogue.Value.Provider.Name).X * providerTextScaleCoefficient * SLEnvironment.FontScale / 2f), 2);
            }

            if (awaitingMouseDialogueHover)
            {
                textFrameGoUp = true;
            }

            if (SLMouse.Domain == SLMouse.MouseDomain.Dialogue)
            {
                awaitingMouseDialogueHover = false;

                textFrameGoUp = true;
            }
            else
            {
                if (!awaitingMouseDialogueHover)
                {
                    textFrameGoUp = false;
                }
            }


            // dialogueShowCoefficient += MathF.Pow(dialogueShowCoefficient, 2) * 0.1f * (float)(textFrameGoUp ? 1f : -1f);

            DynamicTweening.PushSubunitary(ref dialogueShowCoefficient, textFrameGoUp, elapsedMiliseconds, smoothness: 2);
            dialogueShowCoefficient = Math.Clamp(dialogueShowCoefficient, 0.1f, 1f);
        }
        public int GetMilisecondsFromText(string text)
        {
            return text.Length * charReadMiliseconds;
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(Instance.Textures.Pixel, dialoguebounds, Color.Black);
            if (currentDialogue.HasValue)
            {
                spriteBatch.DrawString(SLEnvironment.Font, toDrawDialogueText, dialogueTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, SLEnvironment.FontScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(SLEnvironment.Font, currentDialogue.Value.Provider.Name.ToUpper(), providerTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, providerTextScaleCoefficient * SLEnvironment.FontScale, SpriteEffects.None, 0f);
            }
            spriteBatch.DrawRectangle(dialoguebounds, Color.White, userInterface.LineWidth);

            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}