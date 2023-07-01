using GridTactics.Main;
using GridTactics.SceneObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.ConversationScene
{
    public class ConversationScene : Scene
    {
        public static List<ConversationRecord> CONVERSATIONS { get; set; }

        private ConversationRecord conversationData;
        private Texture2D backgroundSprite;
        private ConversationViewModel conversationViewModel;
        private ConversationController conversationController;

        public ConversationScene(string conversationName)
            : base()
        {
            conversationData = CONVERSATIONS.FirstOrDefault(x => x.Name == conversationName);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(ConversationRecord iConversationData)
            : base()
        {
            conversationData = iConversationData;

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(string conversationName, Rectangle dialogBounds, bool autoProceed = false)
        {
            conversationData = CONVERSATIONS.FirstOrDefault(x => x.Name == conversationName);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData, dialogBounds, autoProceed);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(string conversationName, Rectangle dialogBounds)
        {
            conversationData = CONVERSATIONS.FirstOrDefault(x => x.Name == conversationName);

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData, dialogBounds);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(ConversationRecord iConversationData, Rectangle dialogBounds)
        {
            conversationData = iConversationData;

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData, dialogBounds);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(ConversationRecord iConversationData, Rectangle dialogBounds, bool autoProceed)
        {
            conversationData = iConversationData;

            string[] conversationScript = conversationData.DialogueRecords[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueRecords[0].Script);

            conversationViewModel = new ConversationViewModel(this, conversationData, dialogBounds, autoProceed);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(ConversationRecord iConversationData, Rectangle dialogBounds, int autoProceedLength)
            : this(iConversationData, dialogBounds, true)
        {
            ConversationViewModel.AutoProceedLength = autoProceedLength;
        }

        public static void Initialize()
        {
            if (CONVERSATIONS == null) CONVERSATIONS = AssetCache.LoadRecords<ConversationRecord>("ConversationData");
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            if (!string.IsNullOrEmpty(conversationData.Background))
            {
                base.BeginScene();

                Audio.PlayMusic(GameMusic.Intro);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            portraits.RemoveAll(x => x.Terminated);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            if (backgroundSprite != null)
                spriteBatch.Draw(backgroundSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
        }

        public void FinishDialogue()
        {
            foreach (Portrait portrait in portraits)
            {
                portrait.FinishTransition();
            }
        }

        public void AddPortrait(Portrait portrait)
        {
            portraits.Add(portrait);
            AddEntity(portrait);
        }

        public void RunScript(string[] script)
        {
            conversationController = AddController(new ConversationController(this, script));
        }

        public bool IsScriptRunning()
        {
            return conversationController != null && !conversationController.Terminated;
        }

        private List<Portrait> portraits = new List<Portrait>();
        public List<Portrait> Portraits { get => portraits; }
        public ConversationViewModel ConversationViewModel { get => conversationViewModel; }

        public bool EndGame { get => conversationController.EndGame; }
    }
}
