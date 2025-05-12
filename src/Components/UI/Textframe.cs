
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using static Stolon.StolonGame;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace Stolon
{
    public struct DialogueDrawArgs
    {
        public DialogueInfo Source { get; }
        public int[] TimeMap { get; }
        public int PostTime { get; }
        public DialogueDrawArgs(DialogueInfo source, int[] timeMap, int postTime)
        {
            Source = source;
            TimeMap = timeMap;
            PostTime = postTime;
        }
        public static DialogueDrawArgs FromInfo(DialogueInfo info)
        {
            return new DialogueDrawArgs(info, new int[info.Text.Length].Select((item, i) =>
            {
                char c = info.Text[i];
                return c switch
                {
                    '.' => Textframe.CharReadMiliseconds * 3,
                    '?' => Textframe.CharReadMiliseconds * 3,
                    _ => Textframe.CharReadMiliseconds,
                };
            }).ToArray(), info.PostMiliseconds);
        }
    }
    public class Textframe : GameComponent
    {
        private Queue<DialogueInfo> dialogueQueue;
        private Rectangle dialoguebounds;
        private DialogueInfo? currentDialogue;
        private DialogueDrawArgs? currentDialogueDrawArgs;
        private Point dialogueTextPos;
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
        public const int CharReadMiliseconds = 75; // per char
        public const int PostReadMiliseconds = CharReadMiliseconds * 10; // how long the dialogue stagnates after its finished.

        private int msSinceLastChar;
        private int charsRead;
        private int postTimeRead;

        private UserInterface userInterface;

        public Textframe(UserInterface userInterface)
        {
            dialogueQueue = new Queue<DialogueInfo>();
            dialogueTextPos = Point.Zero;
            currentDialogue = null;
            currentDialogueDrawArgs = null;
            dialogueShowCoefficient = 0f;
            msSinceLastChar = 0;
            charsRead = 0;
            toDrawDialogueText = string.Empty;
            this.userInterface = userInterface;
        }
        public void Queue(DialogueInfo[] dialogue)
        {
            for (int i = 0; i < dialogue.Length; i++)
                Queue(dialogue[i]);
        }
        public void Queue(DialogueInfo dialogue)
        {
            dialogueQueue.Enqueue(dialogue);
            Instance.DebugStream.Log("dialogue queued with text: " + dialogue.Text);
        }
        public void Next()
        {
            if (dialogueQueue.Count == 0) throw new Exception();

            Instance.DebugStream.Log(">attempting dequeuing of dialogue with text: " + dialogueQueue.Peek().Text);
            bool providerDiffers = currentDialogue.HasValue && currentDialogue.Value.Provider.Name != dialogueQueue.Peek().Provider.Name;
            if (providerDiffers) Instance.DebugStream.Log("dialogue has new provider of name: " + dialogueQueue.Peek().Provider.Name);

            currentDialogue = dialogueQueue.Dequeue();
            currentDialogueDrawArgs = DialogueDrawArgs.FromInfo(currentDialogue.Value);

            toDrawDialogueText = string.Empty;
            msSinceLastChar = 0;
            charsRead = 0;
            postTimeRead = 0;

            //initialDialogueMiliseconds = GetMilisecondsFromText(currentDialogue.Value.Text) + currentDialogue.Value.ExtraMS;
            //dialogueMilisecondsRemaining = initialDialogueMiliseconds;

            awaitingMouseDialogueHover = true;
            if (providerDiffers || providerTextSizeTweener == null) // initialize or refresh.
            {
                providerTextSizeTweener = new Tweener<float>(0.0001f, 1f, 1, Ease.Sine.Out); // 0 does not work with size calculations..
                providerTextSizeTweener.Start();
            }

            Instance.DebugStream.Success();
        }

        public void Queue(int count, Func<string, int, string>? selector = null)
        {
            Instance.DebugStream.Log(">mass queueing a stream of size: " + count);
            selector ??= new Func<string, int, string>((s, i) => s);
            for (int i = 0; i < count; i++) Queue(new DialogueInfo(StolonEnvironment.Instance, selector.Invoke(Instance.UserInterface.GetRandomSplashText(), i)));
            Instance.DebugStream.Success();
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

                if (toDrawDialogueText == currentDialogue.Value.Text) // if no text is left to add..
                {
                    postTimeRead += elapsedMiliseconds; // only add postread if text is full.
                    if (dialogueQueue.Count > 0 && postTimeRead > currentDialogueDrawArgs!.Value.PostTime) Next(); // ..and queue is full, go next.
                }
                else if (msSinceLastChar > currentDialogueDrawArgs!.Value.TimeMap[charsRead]) // else if its time for a new char..
                {
                    //Console.WriteLine(currentDialogueDrawArgs!.Value.TimeMap[charsRead]);
                    toDrawDialogueText += currentDialogue.Value.Text[charsRead]; // ..add said char.
                    charsRead++; 
                    msSinceLastChar = 0;
                }

                providerTextSizeTweener!.Update(elapsedMiliseconds / 1000f);
                providerTextScaleCoefficient = MathF.Min(providerTextSizeTweener.Value, dialogueShowCoefficient > 0.9f ? 1f : dialogueShowCoefficient);

                //toDrawDialogueText = dialogueMilisecondsRemaining < 0 ? currentDialogue.Value.Text :
                //    currentDialogue.Value.Text[0..(int)MathF.Ceiling(currentDialogue.Value.Text.Length * ((initialDialogueMiliseconds - dialogueMilisecondsRemaining) / (float)initialDialogueMiliseconds))];

                dialogueTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - Instance.Fonts["fonts\\smollerMono"].FastMeasureString(toDrawDialogueText).X / 2f),
                    (int)(dialoguebounds.Height / 2f - Instance.Fonts["fonts\\smollerMono"].Dimensions.Y / 2f));
                 
                providerTextPos = dialoguebounds.Location
                    + new Point((int)(dialoguebounds.Width / 2f - Instance.Fonts["fonts\\smollerMono"].FastMeasureString(currentDialogue.Value.Provider.Name).X * providerTextScaleCoefficient / 2f), 2);
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
                spriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], toDrawDialogueText, dialogueTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, Instance.Fonts["fonts\\smollerMono"].Scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], currentDialogue.Value.Provider.Name.ToUpper(), providerTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, providerTextScaleCoefficient * Instance.Fonts["fonts\\smollerMono"].Scale, SpriteEffects.None, 0f);
            }
            spriteBatch.DrawRectangle(dialoguebounds, Color.White, userInterface.LineWidth);

            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}