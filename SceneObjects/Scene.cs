using GridTactics.Main;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Particles;
using GridTactics.SceneObjects.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace GridTactics.SceneObjects
{
    public class Scene
    {
        public delegate void SceneEndCallback();

        protected List<Overlay> overlayList = new List<Overlay>();
        protected List<Controller>[] controllerList = new List<Controller>[Enum.GetNames(typeof(PriorityLevel)).Length];
        protected PriorityLevel priorityLevel;

        protected List<Entity> entityList = new List<Entity>();
        protected List<Particle> particleList = new List<Particle>();

        protected Shader spriteShader;
        public Shader SceneShader { get; set; }

        protected bool sceneStarted;
        protected bool sceneEnded;

        public Scene()
        {
            foreach (int controlLevel in Enum.GetValues(typeof(PriorityLevel)))
            {
                controllerList[controlLevel] = new List<Controller>();
            }
        }

        public virtual void BeginScene()
        {
            sceneStarted = true;

            TransitionController transitionController = new TransitionController(TransitionDirection.In, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            transitionController.FinishTransition += new Action<TransitionDirection>(t => colorFade.Terminate());
            AddController(transitionController);
            CrossPlatformGame.TransitionShader = colorFade;
        }

        public virtual void ResumeScene()
        {

        }

        public virtual void EndScene()
        {
            sceneEnded = true;
            OnTerminated?.Invoke();
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Suspended) return;

            int i = 0;

            for (i = 0; i < Enum.GetNames(typeof(PriorityLevel)).Length; i++)
            {
                if (controllerList[i].Count > 0)
                {
                    priorityLevel = (PriorityLevel)i;
                }
            }

            List<Controller> activeControllers = controllerList.LastOrDefault(x => x.All(y => y.PriorityLevel >= priorityLevel) && x.Count > 0);

            if (activeControllers != null)
            {
                i = 0;
                while (i < activeControllers.Count)
                {
                    activeControllers[i].PreUpdate(gameTime);
                    i++;
                }

                activeControllers.RemoveAll(x => x.Terminated);
            }

            i = 0;

            if (priorityLevel < PriorityLevel.TransitionLevel)
            {
                while (i < overlayList.Count)
                {
                    overlayList[i].Update(gameTime);
                    i++;
                }
                overlayList.RemoveAll(x => x.Terminated);
            }

            var entities = entityList.FindAll(x => x.PriorityLevel >= priorityLevel);
            foreach (Entity entity in entities)
            {
                entity.Update(gameTime);
            }
            entityList.RemoveAll(x => x.Terminated);

            i = 0;
            while (i < particleList.Count) { particleList[i].Update(gameTime); i++; }
            particleList.RemoveAll(x => x.Terminated);

            if (activeControllers != null)
            {
                i = 0;
                while (i < activeControllers.Count)
                {
                    activeControllers[i].PostUpdate(gameTime);
                    i++;
                }

                activeControllers.RemoveAll(x => x.Terminated);
            }

            if (spriteShader != null)
            {
                spriteShader.Update(gameTime, Camera);
                if (spriteShader.Terminated) spriteShader = null;
            }

            if (SceneShader != null)
            {
                SceneShader.Update(gameTime, Camera);
                if (SceneShader.Terminated) SceneShader = null;
            }
        }

        public virtual void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(pixelRender);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawBackground(spriteBatch);
            spriteBatch.End();

            Matrix matrix = (Camera == null) ? Matrix.Identity : Camera.Matrix;
            Effect shader = (spriteShader == null) ? null : spriteShader.Effect;
            foreach (Entity entity in entityList) entity.DrawShader(spriteBatch, Camera, matrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            DrawGame(spriteBatch, shader, matrix);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(compositeRender);

            if (!CrossPlatformGame.ClearedCompositeRender)
            {
                CrossPlatformGame.ClearedCompositeRender = true;
                graphicsDevice.Clear(Color.Transparent);
            }

            shader = (SceneShader == null) ? null : SceneShader.Effect;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.Identity);
            spriteBatch.Draw(pixelRender, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }

        public virtual void DrawBackground(SpriteBatch spriteBatch)
        {

        }

        public virtual void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            foreach (Entity entity in entityList) entity.Draw(spriteBatch, Camera);
            foreach (Particle particle in particleList) particle.Draw(spriteBatch, Camera);
        }

        public virtual void DrawOverlay(SpriteBatch spriteBatch)
        {
            foreach (Overlay overlay in overlayList) overlay.Draw(spriteBatch);
        }

        public virtual T AddParticle<T>(T newParticle) where T : Particle
        {
            particleList.Add(newParticle);
            return newParticle;
        }

        public T AddOverlay<T>(T newOverlay) where T : Overlay
        {
            overlayList.Add(newOverlay);
            return newOverlay;
        }

        public T AddController<T>(T newController, int index = -1) where T : Controller
        {
            if (index == -1) controllerList[(int)newController.PriorityLevel].Add(newController);
            else controllerList[(int)newController.PriorityLevel].Insert(index, newController);
            return newController;
        }

        public T AddEntity<T>(T newEntity) where T : Entity
        {
            entityList.Add(newEntity);
            return newEntity;
        }

        public T AddView<T>(T viewModel) where T : ViewModel
        {
            AddOverlay(viewModel);

            return viewModel;
        }

        public event TerminationFollowup OnTerminated;

        public virtual int Width { get => CrossPlatformGame.ScreenWidth; }
        public PriorityLevel PriorityLevel { get => priorityLevel; }
        public List<Controller>[] ControllerStack { get => controllerList; }
        public List<Overlay> OverlayList { get => overlayList; }
        public Camera Camera { get; protected set; }
        public List<Entity> EntityList { get => entityList; }
        public Vector2 GetMousePoint { get => new Vector2(Input.MousePosition.X + Camera.View.X, Input.MousePosition.Y + Camera.View.Y); }
        public bool SceneEnded { get => sceneEnded; }
        public bool Suspended { get; set; }
    }
}
