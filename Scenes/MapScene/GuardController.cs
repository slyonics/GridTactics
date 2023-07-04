using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class GuardController : ScriptController
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Guard npc;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

        private Controller activeController;

        private Dictionary<string, Vector2> patrolRoute;
        private string destinationWaypoint;


        public GuardController(MapScene iScene, Guard iNpc)
            : base(iScene, iNpc.Behavior, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            npc = iNpc;

            currentTile = mapScene.Tilemap.GetTile(npc.Center);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (activeController != null)
            {
                if (activeController.Terminated) activeController = null;
                return;
            }

            if (!scriptParser.Finished && destinationTile == null) base.PreUpdate(gameTime);

            if (destinationTile == null)
            {
                npc.DesiredVelocity = Vector2.Zero;
                npc.OrientedAnimation("Idle");
            }
            else
            {
                npc.DesiredVelocity = Vector2.Zero;
                npc.Reorient(destinationTile.Center - currentTile.Center);
                npc.OrientedAnimation("Walk");
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            if (activeController != null)
            {
                if (activeController.Terminated) activeController = null;
                return;
            }

            if (destinationTile != null)
            {
                walkTimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (walkTimeLeft > 0.0f)
                {
                    Vector2 npcPosition = Vector2.Lerp(destinationTile.Center, currentTile.Center, walkTimeLeft / currentWalkLength);
                    npc.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    npc.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;
                }
            }
        }

        public bool Move(Orientation direction, float walkLength = DEFAULT_WALK_LENGTH)
        {
            npc.Orientation = direction;

            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile npcDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (npcDestination == null) return false;
            if (!mapScene.Tilemap.CanTraverse(npc, npcDestination)) return false;
            //if (((CaterpillarController)mapScene.HeroList[0].ControllerList.Find(x => x is CaterpillarController)).OccupiedTile == npcDestination) return false;

            destinationTile = npcDestination;
            currentWalkLength = walkTimeLeft = walkLength;

            return true;
        }

        public void Turn(bool clockwise)
        {
            Orientation newOrientation = npc.Orientation;

            if (clockwise)
            {
                if (newOrientation == Orientation.Left) newOrientation = Orientation.Up;
                else newOrientation = npc.Orientation + 1;
                npc.Reorient(newOrientation);
                npc.Idle();
            }
            else
            {
                if (newOrientation == Orientation.Up) newOrientation = Orientation.Left;
                else newOrientation = npc.Orientation - 1;
                npc.Reorient(newOrientation);
                npc.Idle();
            }
        }

        public void Patrol(IEnumerable<string> waypoints)
        {
            patrolRoute = new List<string>(waypoints);
            ResumePatrol();
        }

        private void ResumePatrol()
        {
            Vector2 closestWaypoint = mapScene

            PathingController pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, npc, new Vector2(403, 371), 30);
            activeController = mapScene.AddController(pathingController);
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Wander": Move((Orientation)Rng.RandomInt(0, 3), int.Parse(tokens[1]) / 1000.0f); break;
                case "Turn": Turn(tokens[1] == "CW"); break;
                case "Patrol": Patrol(tokens.Skip(1)); break;
                default: return false;
            }

            return true;
        }
    }
}
