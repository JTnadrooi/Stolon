using AsitLib.XNA;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using static Stolon.StolonGame;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace Stolon
{
    public class Textframe : AxComponent
    {
        private Queue<DialogueInfo> dialogueQueue;
        private Rectangle dialoguebounds;
        private DialogueInfo? currentDialogue;
        private Point dialogueTextPos;
        private string toDrawDialogueText;

        private Point providerTextPos;
        private float providerTextScaleCoefficient;
        private Tweener<float>? providerTextSizeTweener;

        private float dialogueShowCoefficient;
        private bool awaitingMouseDialogueHover;
        private bool dialogueIsHidden;
        private bool dialogueHasFinished;
        public bool DialogueIsHidden
        {
            get => dialogueIsHidden;
            set => dialogueIsHidden = value;
        }

        public Rectangle DialogueBounds => dialoguebounds;
        public const int CharReadMiliseconds = 75; // per char
        private const int postReadMiliseconds = CharReadMiliseconds * 10; // how long the dialogue stagnates after its finished.

        private int msSinceLastChar;
        private int charsRead;

        private UserInterface userInterface;

        public Textframe(UserInterface userInterface)
        {
            dialogueQueue = new Queue<DialogueInfo>();
            dialogueTextPos = Point.Zero;
            currentDialogue = null;
            dialogueShowCoefficient = 0f;
            msSinceLastChar = 0;
            charsRead = 0;
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

        public void Next()
        {
            Instance.DebugStream.WriteLine("Next dialogue has been requested.");
            if (dialogueQueue.Count == 0) throw new Exception();

            Instance.DebugStream.WriteLine("\tAttempting dequeuing of dialogue with text: " + dialogueQueue.Peek().Text);
            bool providerDiffers = currentDialogue.HasValue && currentDialogue.Value.Provider.Name != dialogueQueue.Peek().Provider.Name;
            if (providerDiffers) Instance.DebugStream.WriteLine("\tDialogue has new provider of name: " + dialogueQueue.Peek().Provider.Name);

            currentDialogue = dialogueQueue.Dequeue();

            toDrawDialogueText = string.Empty;
            msSinceLastChar = 0;
            charsRead = 0;

            //initialDialogueMiliseconds = GetMilisecondsFromText(currentDialogue.Value.Text) + currentDialogue.Value.ExtraMS;
            //dialogueMilisecondsRemaining = initialDialogueMiliseconds;

            awaitingMouseDialogueHover = true;
            if (providerDiffers || providerTextSizeTweener == null) // initialize or refresh.
            {
                providerTextSizeTweener = new Tweener<float>(0.0001f, 1f, 1, Ease.Sine.Out); // 0 does not work with size calculations..
                providerTextSizeTweener.Start();
            }

            Instance.DebugStream.Succes(1);
        }

        public void Queue(int count, Func<string, int, string>? selector = null)
        {
            Instance.DebugStream.WriteLine("Mass queueing a stream of size: " + count);
            selector ??= new Func<string, int, string>((s, i) => s);
            for (int i = 0; i < count; i++) Queue(new DialogueInfo(StolonEnvironment.Instance, selector.Invoke(Instance.UserInterface.GetRandomSplashText(), i)));
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

            msSinceLastChar += elapsedMiliseconds;

            if (dialogueQueue.Count > 0 && !currentDialogue.HasValue) Next();

            if (currentDialogue.HasValue) // dialogue is here!
            {
                if (currentDialogue.Value.Text.Length == 0) throw new Exception("Text size zero.");

                if (toDrawDialogueText == currentDialogue.Value.Text)
                {
                    if (dialogueQueue.Count > 0) Next();
                }
                else if (msSinceLastChar > CharReadMiliseconds)
                {
                    toDrawDialogueText += currentDialogue.Value.Text[charsRead];
                    charsRead++;
                    msSinceLastChar = 0;
                }

                providerTextSizeTweener!.Update(elapsedMiliseconds / 1000f);
                providerTextScaleCoefficient = MathF.Min(providerTextSizeTweener.Value, dialogueShowCoefficient > 0.9f ? 1f : dialogueShowCoefficient);

                //toDrawDialogueText = dialogueMilisecondsRemaining < 0 ? currentDialogue.Value.Text :
                //    currentDialogue.Value.Text[0..(int)MathF.Ceiling(currentDialogue.Value.Text.Length * ((initialDialogueMiliseconds - dialogueMilisecondsRemaining) / (float)initialDialogueMiliseconds))];

                dialogueTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - StolonEnvironment.Font.MeasureString(toDrawDialogueText).X * StolonEnvironment.FontScale / 2f),
                    (int)(dialoguebounds.Height / 2f - Instance.Environment.FontDimensions.Y / 2f));

                providerTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - StolonEnvironment.Font.MeasureString(currentDialogue.Value.Provider.Name).X * providerTextScaleCoefficient * StolonEnvironment.FontScale / 2f), 2);
            }
            if (awaitingMouseDialogueHover) textFrameGoUp = true;
            if (SLMouse.Domain == SLMouse.MouseDomain.Dialogue)
            {
                awaitingMouseDialogueHover = false;
                textFrameGoUp = true;
            }
            else if (!awaitingMouseDialogueHover)
            {
                textFrameGoUp = false;
            }

            DynamicTweening.PushSubunitary(ref dialogueShowCoefficient, textFrameGoUp, elapsedMiliseconds, smoothness: 2);
            dialogueShowCoefficient = Math.Clamp(dialogueShowCoefficient, 0.1f, 1f);
        }
        public int GetMilisecondsFromText(string text)
        {
            return text.Length * CharReadMiliseconds;
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(Instance.Textures.Pixel, dialoguebounds, Color.Black);
            if (currentDialogue.HasValue)
            {
                spriteBatch.DrawString(StolonEnvironment.Font, toDrawDialogueText, dialogueTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, StolonEnvironment.FontScale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(StolonEnvironment.Font, currentDialogue.Value.Provider.Name.ToUpper(), providerTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, providerTextScaleCoefficient * StolonEnvironment.FontScale, SpriteEffects.None, 0f);
            }
            spriteBatch.DrawRectangle(dialoguebounds, Color.White, userInterface.LineWidth);

            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}