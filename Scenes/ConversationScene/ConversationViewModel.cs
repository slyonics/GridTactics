using GridTactics.Main;
using GridTactics.Models;
using GridTactics.SceneObjects;
using GridTactics.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GridTactics.Scenes.ConversationScene
{
    public class ConversationViewModel : ViewModel
    {
        private ConversationScene conversationScene;
        private ConversationRecord conversationRecord;
        private DialogueRecord currentDialogue;
        private int dialogueIndex;

        private CrawlText crawlText;

        public bool AutoProceed { get; set; }
        public int AutoProceedLength { get; set; } = -1;

        public bool disableEnd = false;

        bool endscriptrunning;

        public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;

            if (!string.IsNullOrEmpty(conversationRecord.Bounds))
            {
                string[] tokens = conversationRecord.Bounds.Split(',');
                Window.Value = new Rectangle(ParseInt(tokens[0]), ParseInt(tokens[1]), ParseInt(tokens[2]), ParseInt(tokens[3]));
            }

            LoadView(GameView.ConversationScene_ConversationView);

            crawlText = GetWidget<CrawlText>("ConversationText");
        }

        public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord, Rectangle conversationBounds, bool autoProceed)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;
            Window.Value = conversationBounds;

            if (autoProceed) LoadView(GameView.ConversationScene_ConversationView3);
            else LoadView(GameView.ConversationScene_ConversationView2);

            crawlText = GetWidget<CrawlText>("ConversationText");

            AutoProceed = autoProceed;
        }

        public ConversationViewModel(ConversationScene iScene, ConversationRecord iConversationRecord, Rectangle conversationBounds)
            : base(iScene, PriorityLevel.GameLevel)
        {
            conversationScene = (parentScene as ConversationScene);
            conversationRecord = iConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;
            Window.Value = conversationBounds;

            LoadView(GameView.ConversationScene_ConversationView);

            crawlText = GetWidget<CrawlText>("ConversationText");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (conversationScene.PriorityLevel > PriorityLevel.GameLevel) return;

            if (AutoProceedLength > 0)
            {
                AutoProceedLength -= gameTime.ElapsedGameTime.Milliseconds;
                if (AutoProceedLength <= 0) Proceed();
            }

            if (crawlText.ReadyToProceed && !ReadyToProceed.Value)
            {
                if (!conversationScene.IsScriptRunning())
                {
                    ReadyToProceed.Value = true;
                }

                OnDialogueScrolled?.Invoke();
                OnDialogueScrolled = null;
            }

            if (!Closed && !ChildList.Any(x => x.Transitioning))
            {
                if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    Proceed();
                }
            }

            if (terminated)
            {
                parentScene.EndScene();
            }
        }

        public void Proceed()
        {
            if (!crawlText.ReadyToProceed)
            {
                crawlText.FinishText();
                conversationScene.FinishDialogue();
            }
            else if (!AutoProceed) NextDialogue();
        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "ConversationText":
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        conversationScene.FinishDialogue();
                    }
                    else if (!AutoProceed) NextDialogue();
                    break;
            }
        }

        public void NextDialogue()
        {
            dialogueIndex++;

            if (dialogueIndex >= conversationRecord.DialogueRecords.Length)
            {
                if (conversationRecord.EndScript != null)
                {
                    if (endscriptrunning) return;
                    endscriptrunning = true;
                    ConversationController conversationController = conversationScene.AddController(new ConversationController(conversationScene, conversationRecord.EndScript));
                    conversationController.OnTerminated += new TerminationFollowup(() =>
                    {
                        if (endscriptrunning) EndConversation();
                    });
                }
                else if (!disableEnd) EndConversation();

                return;
            }

            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Dialogue.Value = currentDialogue.Text;
            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;

            ReadyToProceed.Value = false;

            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);
        }

        public void EndConversation()
        {
            //if (string.IsNullOrEmpty(conversationRecord.Background))
            //{
                Close();
            //}
            //else
            //{
            //    if (conversationScene.EndGame) CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
            //    else CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
            //}
        }

        public void ChangeConversation(ConversationRecord newConversationRecord)
        {
            dialogueIndex = 0;

            conversationRecord = newConversationRecord;
            currentDialogue = conversationRecord.DialogueRecords[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;

            disableEnd = false;
            endscriptrunning = false;
        }

        public event Action OnDialogueScrolled;

        public ModelProperty<Rectangle> Window { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 10, 240, 60));
        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<GameFont> ConversationFont { get; set; } = new ModelProperty<GameFont>(GameFont.Dialogue);
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
    }
}
