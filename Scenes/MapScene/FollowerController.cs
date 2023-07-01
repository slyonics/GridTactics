using GridTactics.SceneObjects.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class FollowerController : Controller
    {
        private enum Behavior
        {
            Idling,
            Regrouping,
            Stuck
        }

        private const int START_REGROUP_DISTANCE = 34;
        private const int END_REGROUP_DISTANCE = 18;
        private const float STUCK_THRESHOLD = 700.0f;
        private const int MOVEMENT_HISTORY_LENGTH = 30;

        private MapScene mapScene;
        private Actor follower;
        private Actor leader;

        private Behavior behavior = Behavior.Idling;
        private float stuckDetection;

        private Vector2 lastPosition;
        private PathingController pathingController;

        private List<Vector3> movementHistory = new List<Vector3>();

        public FollowerController(MapScene iMapScene, Actor iFollower, Actor iLeader)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            follower = iFollower;
            leader = iLeader;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            switch (behavior)
            {
                case Behavior.Idling: IdlingAI(leader); break;
                case Behavior.Regrouping: RegroupingAI(gameTime, leader); break;
                case Behavior.Stuck: StuckAI(gameTime, leader); break;
            }
        }

        private void IdlingAI(Actor humanPlayer)
        {
            movementHistory.Clear();

            if (Vector2.Distance(humanPlayer.Position, follower.Position) > START_REGROUP_DISTANCE)
            {
                behavior = Behavior.Regrouping;
                stuckDetection = 0;
                lastPosition = follower.Position;
                return;
            }

            follower.Idle();
        }

        private void RegroupingAI(GameTime gameTime, Actor humanPlayer)
        {
            if (follower.BlockedDisplacement.Length() > 1) stuckDetection += follower.BlockedDisplacement.Length() * gameTime.ElapsedGameTime.Milliseconds;
            else stuckDetection = 0;
            if (stuckDetection > STUCK_THRESHOLD)
            {
                behavior = Behavior.Stuck;

                pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, follower, humanPlayer, PlayerController.WALKING_SPEED);
                mapScene.AddController(pathingController);

                follower.Idle();
                return;
            }

            if (Vector2.Distance(humanPlayer.Position, follower.Position) < END_REGROUP_DISTANCE)
            {
                behavior = Behavior.Idling;

                follower.Idle();
            }
            else
            {
                movementHistory.Add(new Vector3(humanPlayer.Center - follower.Center, gameTime.ElapsedGameTime.Milliseconds / 1000.0f));
                if (movementHistory.Count > MOVEMENT_HISTORY_LENGTH) movementHistory.RemoveAt(0);

                Vector2 movement = Vector2.Zero;
                foreach (Vector3 vector in movementHistory) movement += new Vector2(vector.X, vector.Y) * vector.Z;

                Vector3 leaderMovement = movementHistory.Last();
                if (leaderMovement.Length() > 0.001f)
                {
                    if (follower.Bounds.Top <= leader.Bounds.Top && follower.Bounds.Bottom >= leader.Bounds.Bottom && Math.Abs(leaderMovement.Y) < 0.0001f)
                    {
                        movement.Y = 0.0f;
                    }
                    else if (follower.Bounds.Right <= leader.Bounds.Right && follower.Bounds.Left >= leader.Bounds.Left && Math.Abs(leaderMovement.X) < 0.0001f)
                    {
                        movement.X = 0.0f;
                    }
                }

                if (movement.Length() < 0.001f) movement = Vector2.Zero;
                else movement.Normalize();

                if (Input.CurrentInput.CommandDown(Command.Run))
                    follower.Walk(movement, PlayerController.RUN_SPEED);
                else
                    follower.Walk(movement, PlayerController.WALKING_SPEED);
                lastPosition = follower.Position;
            }
        }

        public void StuckAI(GameTime gameTime, Actor humanPlayer)
        {
            if (Vector2.Distance(follower.Position, humanPlayer.Position) < END_REGROUP_DISTANCE)
            {
                behavior = Behavior.Idling;
                if (pathingController != null) pathingController.Terminate();
                pathingController = null;
                follower.Idle();
                return;
            }

            if (pathingController.PathingError)
            {
                var targetNode = mapScene.Tilemap.GetNavNode(follower, humanPlayer);
                if (targetNode != null) follower.Teleport(targetNode.Center);
                else follower.Teleport(humanPlayer.Position);
            }

            if (pathingController.Terminated)
            {
                behavior = Behavior.Regrouping;
                stuckDetection = 0;
                pathingController = null;
                lastPosition = follower.Position;
            }
        }
    }
}
