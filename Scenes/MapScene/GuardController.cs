using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Maps;
using GridTactics.SceneObjects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class GuardController : ScriptController
    {
        private enum Awareness
        {
            Patrolling,
            Suspicious,
            Chasing,
            Searching
        }

        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Guard npc;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

        private Controller activeController;
        private EmoteParticle emoteParticle;

        private List<string> patrolRoute;
        private string currentWaypoint;
        private string destinationWaypoint;

        private Awareness awareness;
        private int giveUpTimer = 3000;
        private int detectionTimer = 2000;
        private int hearingRange = 60;
        private int sightRange = 120;


        public GuardController(MapScene iScene, Guard iNpc)
            : base(iScene, iNpc.Behavior, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            npc = iNpc;

            currentTile = mapScene.Tilemap.GetTile(npc.Center);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            UpdateAwareness(gameTime);

            if (activeController != null)
            {
                if (activeController.Terminated)
                {
                    activeController = null;

                    if (patrolRoute != null && awareness == Awareness.Patrolling)
                    {
                        currentWaypoint = destinationWaypoint;
                        int index = patrolRoute.IndexOf(currentWaypoint);
                        int nextIndex = index + 1;
                        if (nextIndex >= patrolRoute.Count) nextIndex = 0;
                        destinationWaypoint = patrolRoute[nextIndex];

                        PathingController pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, npc, mapScene.Waypoints[destinationWaypoint], 30);
                        activeController = mapScene.AddController(pathingController);
                    }
                    else if (awareness == Awareness.Chasing)
                    {
                        PathingController pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, npc, mapScene.PartyLeader.Center, 30);
                        activeController = mapScene.AddController(pathingController);
                    }
                }
                else
                {

                }

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

        private void UpdateAwareness(GameTime gameTime)
        {
            switch (awareness)
            {
                case Awareness.Patrolling:
                    if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < hearingRange && mapScene.PartyLeader.Running)
                    {
                        ChangeAwareness(Awareness.Suspicious);
                    }
                    else if (CanSeePlayer())
                    {
                        if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < sightRange)
                        {
                            ChangeAwareness(Awareness.Suspicious);
                        }
                    }
                    break;

                case Awareness.Suspicious:

                    if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < 40)
                    {
                        ChangeAwareness(Awareness.Chasing);
                    }
                    else if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < hearingRange && mapScene.PartyLeader.Running)
                    {
                        detectionTimer -= gameTime.ElapsedGameTime.Milliseconds;
                        if (detectionTimer <= 0)
                        {
                            ChangeAwareness(Awareness.Chasing);
                        }
                    }
                    else if (CanSeePlayer() && Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < sightRange)
                    {
                        detectionTimer -= gameTime.ElapsedGameTime.Milliseconds; if (detectionTimer <= 0)
                        {
                            ChangeAwareness(Awareness.Chasing);
                        }
                    }
                    else
                    {
                        giveUpTimer -= gameTime.ElapsedGameTime.Milliseconds;
                        if (giveUpTimer <= 0)
                        {
                            ChangeAwareness(Awareness.Patrolling);
                        }
                    }
                    break;

                case Awareness.Chasing:
                    if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < 40)
                    {
                        giveUpTimer = 3000;
                    }
                    else if (Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < hearingRange && mapScene.PartyLeader.Running)
                    {
                        giveUpTimer = 3000;
                    }
                    else if (CanSeePlayer() && Vector2.Distance(npc.Center, mapScene.PartyLeader.Center) < sightRange)
                    {
                        giveUpTimer = 3000;
                    }
                    else
                    {
                        giveUpTimer -= gameTime.ElapsedGameTime.Milliseconds;
                        if (giveUpTimer <= 0)
                        {
                            ChangeAwareness(Awareness.Patrolling);
                        }
                    }
                    break;
            }

            
        }

        private void ChangeAwareness(Awareness newAwareness)
        {
            Awareness oldAwareness = awareness;

            awareness = newAwareness;
            switch (awareness)
            {

                case Awareness.Patrolling:
                    activeController?.Terminate();
                    activeController = null;

                    ResumePatrol();

                    break;

                case Awareness.Suspicious:
                    giveUpTimer = 3000;
                    detectionTimer = 2000;
                    activeController.Terminate();
                    activeController = null;

                    float deltaX = Math.Abs(mapScene.PartyLeader.Position.X - npc.Position.X);
                    float deltaY = Math.Abs(mapScene.PartyLeader.Position.Y - npc.Position.Y);
                    if (deltaX <= deltaY)
                    {
                        if (mapScene.PartyLeader.Position.Y > npc.Position.Y) npc.Orientation = Orientation.Down;
                        else npc.Orientation = Orientation.Up;
                    }
                    else
                    {
                        if (mapScene.PartyLeader.Position.X > npc.Position.X) npc.Orientation = Orientation.Right;
                        else npc.Orientation = Orientation.Left;
                    }

                    emoteParticle?.Terminate();
                    emoteParticle = mapScene.AddParticle(new EmoteParticle(mapScene, npc, EmoteType.Question));

                    break;

                case Awareness.Chasing:
                    giveUpTimer = 3000;
                    FollowerController pathingController = new FollowerController(mapScene, npc, mapScene.PartyLeader);
                    activeController = mapScene.AddController(pathingController);

                    emoteParticle?.Terminate();
                    emoteParticle = mapScene.AddParticle(new EmoteParticle(mapScene, npc, EmoteType.Exclamation));

                    break;
            }
        }

        private bool CanSeePlayer()
        {
            Tile hostTile = mapScene.Tilemap.GetTile(npc.Center);
            if (hostTile.Obscured) return false;

            switch (npc.Orientation)
            {
                case Orientation.Up: return npc.Position.Y > mapScene.PartyLeader.Position.Y && Math.Abs(mapScene.PartyLeader.Position.Y - npc.Position.Y) > Math.Abs(mapScene.PartyLeader.Position.X - npc.Position.X);
                case Orientation.Right: return npc.Position.X < mapScene.PartyLeader.Position.X && Math.Abs(mapScene.PartyLeader.Position.Y - npc.Position.Y) < Math.Abs(mapScene.PartyLeader.Position.X - npc.Position.X);
                case Orientation.Down: return npc.Position.Y < mapScene.PartyLeader.Position.Y && Math.Abs(mapScene.PartyLeader.Position.Y - npc.Position.Y) > Math.Abs(mapScene.PartyLeader.Position.X - npc.Position.X);
                case Orientation.Left: return npc.Position.X > mapScene.PartyLeader.Position.X && Math.Abs(mapScene.PartyLeader.Position.Y - npc.Position.Y) < Math.Abs(mapScene.PartyLeader.Position.X - npc.Position.X);
                default: return false;
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
            patrolRoute = waypoints.ToList();
            
            StartPatrol();
        }

        private void StartPatrol()
        {
            currentWaypoint = mapScene.Waypoints.OrderBy(x => Vector2.Distance(x.Value, npc.Center)).First().Key;

            int index = patrolRoute.IndexOf(currentWaypoint);
            int nextIndex = index++;
            if (nextIndex >= patrolRoute.Count) nextIndex = 0;
            destinationWaypoint = patrolRoute[nextIndex];

            PathingController pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, npc, mapScene.Waypoints[destinationWaypoint], 30);
            activeController = mapScene.AddController(pathingController);
        }

        private void ResumePatrol()
        {
            PathingController pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, npc, mapScene.Waypoints[destinationWaypoint], 30);
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
