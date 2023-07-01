using GridTactics.Main;
using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects;
using GridTactics.SceneObjects.Maps;
using GridTactics.SceneObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace GridTactics.Scenes.MapScene
{
    public class CaterpillarController : Controller
    {
        private class HeroMovement
        {
            public Hero hero;
            public Tile currentTile;
            public Tile destinationTile;

            public HeroMovement(Hero iHero, Tile iCurrentTile, Tile iDestinationTile)
            {
                hero = iHero;
                currentTile = iCurrentTile;
                destinationTile = iDestinationTile;
            }
        }

        private const float DEFAULT_WALK_LENGTH = 1.0f / 6;

        private MapScene mapScene;

        private List<HeroMovement> movementList;
        private float walkLength;
        private float walkTimeLeft;

        private IInteractive interactable;
        private InteractionPrompt interactionView;

        private bool firstFrame = true;

        public CaterpillarController(MapScene iMapScene, float iWalkLength = DEFAULT_WALK_LENGTH)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            walkLength = iWalkLength;

            interactionView = new InteractionPrompt(mapScene);
            mapScene.AddOverlay(interactionView);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            InputFrame playerInput = Input.CurrentInput;

            if (movementList == null)
            {
                if (mapScene.ProcessAutoEvents())
                {
                    return;
                }

                if (Input.CurrentInput.CommandPressed(Command.Cancel))
                {
                    /*
                    Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                    StatusScene.StatusScene statusScene = new StatusScene.StatusScene();
                    statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                    CrossPlatformGame.StackScene(statusScene);

                    return;
                    */
                }

                bool moveResult = true;
                if (playerInput.CommandDown(Command.Up)) moveResult = Move(Orientation.Up);
                else if (playerInput.CommandDown(Command.Right)) moveResult = Move(Orientation.Right);
                else if (playerInput.CommandDown(Command.Down)) moveResult = Move(Orientation.Down);
                else if (playerInput.CommandDown(Command.Left)) moveResult = Move(Orientation.Left);
                else
                {
                    foreach (Hero hero in mapScene.Party)
                    {
                        hero.DesiredVelocity = Vector2.Zero;
                        if (hero.AnimatedSprite.Frame % 2 != 0) hero.OrientedAnimation("Idle");
                    }
                }

                if (!moveResult) foreach (Hero hero in mapScene.Party) hero.OrientedAnimation("Idle");
                else interactionView.Target(null);
            }

            if (movementList != null)
            {
                foreach (HeroMovement movement in movementList)
                {
                    movement.hero.DesiredVelocity = Vector2.Zero;
                    movement.hero.Reorient(movement.destinationTile.Center - movement.currentTile.Center);
                    movement.hero.OrientedAnimation("Walk");
                }
            }
            else if (playerInput.CommandPressed(Command.Interact) && interactable != null)
            {
                if (interactable.Activate(mapScene.Party[0])) interactable = null;
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (movementList != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    float interval = 1.0f - walkTimeLeft / walkLength;
                    int heroOffset = (int)(interval * mapScene.Tilemap.TileWidth);

                    // mapScene.GameMap.Weather.ProceedTime(interval);

                    foreach (HeroMovement movement in movementList)
                    {
                        Vector2 directionVector = movement.destinationTile.Center - movement.currentTile.Center;
                        directionVector.Normalize();

                        Vector2 heroPosition = movement.currentTile.Center + directionVector * heroOffset;
                        movement.hero.CenterOn(new Vector2((int)heroPosition.X, (int)heroPosition.Y));
                        // movement.hero.UpdateLight(gameTime);
                    }
                }
                else
                {
                    foreach (HeroMovement movement in movementList)
                    {
                        movement.hero.CenterOn(movement.destinationTile.Center);
                        // movement.hero.UpdateLight(gameTime);
                    }

                    Hero leader = movementList.First().hero;
                    movementList = null;

                    Tile hostTile = mapScene.Tilemap.GetTile(leader.Center);

                    // FindTravelZone();
                }
            }
            else if (!firstFrame)
            {
                FindInteractables();
            }

            firstFrame = false;

            mapScene.CheckMusicZone();
        }

        /*
        private string FindTravelZone()
        {
            string exitName = mapScene.Tilemap.GetExit(mapScene.Party[0]);
            if (exitName.Length != 0)
            {
                GameState.SaveMapVisibility(mapScene.GameMap);

                FadeTransition transitionOut = new FadeTransition(Color.Black, Transition.TransitionState.Out, 600);
                CrossPlatformGame.LoadScene(typeof(MapScene), transitionOut, exitName, mapScene.GameMap.Name, mapScene.GameMap.Weather.WorldTime);
            }

            return null;
        }
        */

        private void FindInteractables()
        {
            List<IInteractive> interactableList = new List<IInteractive>();
            interactableList.AddRange(mapScene.NPCs.FindAll(x => x.Interactive));
            interactableList.AddRange(mapScene.EventTriggers.FindAll(x => x.Interactive));

            Hero player = mapScene.PartyLeader;
            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => player.Distance(x.Bounds));
            Rectangle interactZone = player.Bounds;
            int zoneWidth = mapScene.Tilemap.TileWidth;
            int zoneHeight = mapScene.Tilemap.TileHeight;

            var hostTile = mapScene.Tilemap.GetTile(player.Center);

            switch (player.Orientation)
            {
                case Orientation.Up:
                    interactZone = new Rectangle(hostTile.TileX * mapScene.Tilemap.TileWidth, (hostTile.TileY - 1) * mapScene.Tilemap.TileHeight, zoneWidth, zoneHeight);
                    break;
                case Orientation.Right:
                    interactZone = new Rectangle((hostTile.TileX + 1) * mapScene.Tilemap.TileWidth, hostTile.TileY * mapScene.Tilemap.TileHeight, zoneWidth, zoneHeight);
                    break;
                case Orientation.Down:
                    player.InteractionZone.Y += mapScene.Tilemap.TileHeight;
                    interactZone = new Rectangle(hostTile.TileX * mapScene.Tilemap.TileWidth, (hostTile.TileY + 1) * mapScene.Tilemap.TileHeight, zoneWidth, zoneHeight);
                    break;
                case Orientation.Left:
                    interactZone = new Rectangle((hostTile.TileX - 1) * mapScene.Tilemap.TileWidth, hostTile.TileY * mapScene.Tilemap.TileHeight, zoneWidth, zoneHeight);
                    break;
            }
            player.InteractionZone = interactZone;
            interactable = sortedInteractableList.FirstOrDefault(x => x.Bounds.Intersects(player.InteractionZone));
            interactionView.Target(interactable);
        }

        private bool Move(Orientation direction)
        {
            Hero leader = mapScene.Party[0];
            leader.Orientation = direction;

            Tile currentTile = mapScene.Tilemap.GetTile(leader.Center);
            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile leaderDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (leaderDestination == null) return false;
            if (mapScene.Tilemap.GetTile(tileX, tileY).Blocked) return false;
            //if (!mapScene.Tilemap.CanTraverse(leader, leaderDestination)) return false;
            //if (mapScene.NpcList.Exists(x => x.ControllerList.Exists(y => y is NpcController && ((NpcController)y).OccupiedTile == leaderDestination))) return false;

            movementList = new List<HeroMovement>();
            movementList.Add(new HeroMovement(leader, mapScene.Tilemap.GetTile(leader.Center), leaderDestination));

            for (int i = 1; i < mapScene.Party.Count; i++)
            {
                Tile current = mapScene.Tilemap.GetTile(mapScene.Party[i].Center);
                Tile destination = mapScene.Tilemap.GetTile(mapScene.Party[i - 1].Center);
                if (current != destination) movementList.Add(new HeroMovement(mapScene.Party[i], current, destination));
            }

            mapScene.Tilemap.ClearFieldOfView();
            foreach (HeroMovement heroMovement in movementList) mapScene.Tilemap.CalculateFieldOfView(heroMovement.destinationTile, MapScene.SIGHT_RANGE);

            walkTimeLeft = walkLength;
            interactable = null;

            return true;
        }

        public Tile OccupiedTile { get => (movementList == null) ? mapScene.Tilemap.GetTile(mapScene.Party[0].Center) : movementList[0].destinationTile; }
        public IInteractive Interactable { get => interactable; }
    }
}
