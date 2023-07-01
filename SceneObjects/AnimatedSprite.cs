using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public delegate void AnimationFollowup();
    public delegate void AnimationFollowupX(AnimatedSprite animatedSprite);

    public class Animation
    {
        public struct Frame
        {
            public float length;
            public Rectangle source;

            public Frame(int initialLength, Rectangle initialSource)
            {
                length = initialLength / 1000.0f;
                source = initialSource;
            }
        }

        public Frame[] frames;

        public Animation(int cellX, int cellY, int cellWidth, int cellHeight, int frameCount, int frameLength, int maxWidth = -1)
        {
            int[] frameLengths = GenerateFrameLengths(frameLength, frameCount);
            Rectangle[] frameSources = GenerateFrameSouces(cellX, cellY, cellWidth, cellHeight, frameCount, maxWidth);

            frames = GenerateFrames(frameSources, frameLengths);
        }

        public Animation(int cellX, int cellY, int cellWidth, int cellHeight, int frameCount, int[] frameLengths, int maxWidth = -1)
        {
            Rectangle[] frameSources = GenerateFrameSouces(cellX, cellY, cellWidth, cellHeight, frameCount, maxWidth);

            frames = GenerateFrames(frameSources, frameLengths);
        }

        public Animation(Rectangle[] frameSources, int frameLength, int maxWidth = -1)
        {
            int[] frameLengths = GenerateFrameLengths(frameLength, frameSources.Length);

            frames = GenerateFrames(frameSources, frameLengths);
        }

        public Animation(Rectangle[] frameSources, int[] frameLengths, int maxWidth = -1)
        {
            frames = GenerateFrames(frameSources, frameLengths);
        }

        private Frame[] GenerateFrames(Rectangle[] frameSources, int[] frameLengths)
        {
            if (frameSources.Length != frameLengths.Length) throw new Exception();

            Frame[] animationFrames = new Frame[frameSources.Length];
            for (int i = 0; i < frameSources.Length; i++) animationFrames[i] = new Frame(frameLengths[i], frameSources[i]);
            return animationFrames;
        }

        private int[] GenerateFrameLengths(int frameLength, int frameCount)
        {
            int[] frameLengths = new int[frameCount];
            for (int i = 0; i < frameLengths.Length; i++) frameLengths[i] = frameLength;
            return frameLengths;
        }

        private Rectangle[] GenerateFrameSouces(int cellX, int cellY, int cellWidth, int cellHeight, int frameCount, int textureWidth = -1)
        {
            Rectangle[] frameSources = new Rectangle[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                if (textureWidth > 0 && cellWidth * (cellX + i) >= textureWidth)
                {
                    cellX -= textureWidth / cellWidth;
                    cellY++;
                }

                frameSources[i] = new Rectangle(cellWidth * (cellX + i), cellHeight * cellY, cellWidth, cellHeight);
            }
            return frameSources;
        }
    }

    public class AnimatedSprite
    {
        private Texture2D sprite;
        private Color spriteColor;
        private SpriteEffects spriteEffects;
        private float rotation;
        private Vector2 scaleVector = new Vector2(1.0f, 1.0f);

        private Dictionary<string, Animation> animationList;
        private Animation animation;
        private string animationIndex = null;

        private Animation.Frame frame;
        private int frameIndex;

        private float timeToNextFrame;
        private float animationSpeed;
        private AnimationFollowup animationFollowup;
        private AnimationFollowupX animationFollowupX;

        public AnimatedSprite(Texture2D iSprite, Dictionary<string, Animation> initialAnimationList)
        {
            sprite = iSprite;
            spriteColor = Color.White;
            animationList = initialAnimationList;

            if (animationList == null)
            {
                animationList = new Dictionary<string, Animation>();
                animationList.Add("Idle", new Animation(0, 0, iSprite.Width, iSprite.Height, 1, 1000));
            }

            PlayAnimation(animationList.First().Key);
        }

        public AnimatedSprite(AnimatedSprite clone)
        {
            sprite = clone.sprite;
            spriteColor = clone.spriteColor;
            spriteEffects = clone.spriteEffects;
            rotation = clone.rotation;

            animationList = clone.animationList;
            animation = clone.animation;
            animationIndex = clone.animationIndex;

            frame = clone.frame;
            frameIndex = clone.frameIndex;

            timeToNextFrame = clone.timeToNextFrame;
            animationSpeed = clone.animationSpeed;
            animationFollowup = clone.animationFollowup;
            animationFollowupX = clone.animationFollowupX;
        }

        public void Update(GameTime gameTime)
        {
            if (timeToNextFrame < 0.0f) return;

            timeToNextFrame += gameTime.ElapsedGameTime.Milliseconds / 1000.0f * animationSpeed;
            while (timeToNextFrame >= frame.length)
            {
                timeToNextFrame -= frame.length;
                frameIndex++;
                if (frameIndex < animation.frames.Length) frame = animation.frames[frameIndex];
                else
                {
                    if (animationFollowup == null && animationFollowupX == null)
                    {
                        frameIndex = 0;
                        frame = animation.frames[frameIndex];
                    }
                    else
                    {
                        frameIndex--;
                        timeToNextFrame = -1;

                        animationFollowup?.Invoke();
                        animationFollowup = null;

                        animationFollowupX?.Invoke(this);
                        animationFollowupX = null;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Camera camera, float depth)
        {
            Rectangle destination = SpriteBounds(position);
            if (camera != null && !camera.View.Intersects(destination)) return;

            Vector2 origin = new Vector2(frame.source.Width / 2, frame.source.Height);
            if (rotation > 0) spriteBatch.Draw(sprite, new Vector2((int)position.X, (int)position.Y) - new Vector2(frame.source.Width / 2, frame.source.Height / 2), frame.source, spriteColor, rotation, new Vector2(frame.source.Width / 2, frame.source.Height / 2), scaleVector, spriteEffects, depth);
            else spriteBatch.Draw(sprite, new Vector2((int)position.X, (int)position.Y), frame.source, spriteColor, rotation, origin, scaleVector, spriteEffects, depth);
        }

        public void PlayAnimation(string index)
        {
            if (index == animationIndex && animationFollowup == null) return;

            animationFollowup = null;
            animationFollowupX = null;

            animationIndex = index;
            animation = animationList[animationIndex];

            frameIndex = 0;
            frame = animation.frames[frameIndex];

            timeToNextFrame = 0;
            animationSpeed = 1.0f;
        }

        public void PlayAnimation(string index, AnimationFollowup followup)
        {
            animationIndex = "";

            PlayAnimation(index);

            animationFollowup = followup;
        }

        public void PlayAnimation(string index, AnimationFollowupX followup)
        {
            animationIndex = "";

            PlayAnimation(index);

            animationFollowupX = followup;
        }

        public void PlayAnimation(Animation newAnimation)
        {
            animationFollowup = null;
            animationFollowupX = null;

            animationIndex = "";
            animation = newAnimation;

            frameIndex = 0;
            frame = animation.frames[frameIndex];

            timeToNextFrame = 0;
            animationSpeed = 1.0f;
        }

        public void PlayAnimation(Animation newAnimation, AnimationFollowup followup)
        {
            animationIndex = "";

            PlayAnimation(newAnimation);

            animationFollowup = followup;
        }

        public void PlayAnimation(Animation newAnimation, AnimationFollowupX followup)
        {
            animationIndex = "";

            PlayAnimation(newAnimation);

            animationFollowupX = followup;
        }

        public Rectangle SpriteBounds()
        {
            return new Rectangle(0, 0, frame.source.Width, frame.source.Height);
        }

        public Rectangle SpriteBounds(Vector2 position)
        {
            return new Rectangle((int)(position.X - frame.source.Width / 2), (int)(position.Y - frame.source.Height), frame.source.Width, frame.source.Height);
        }

        public Color SpriteColor { get => spriteColor; set => spriteColor = value; }
        public SpriteEffects SpriteEffects { set => spriteEffects = value; }
        public float Rotation { set => rotation = value; }
        public Vector2 Scale { get => scaleVector; set => scaleVector = value; }
        public float AnimationSpeed { set => animationSpeed = value; }

        public string AnimationName { get => animationIndex; }
        public int Frame { get => frameIndex; set => frameIndex = value; }

        public Dictionary<string, Animation> AnimationList
        {
            get => animationList; set
            {
                animationList = value;
                animation = null;
                PlayAnimation(animationList.First().Key);
            }
        }
    }
}
