using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Maps
{
    public class PathingController : Controller
    {
        private const int DEFAULT_SEARCH_LIMIT = 500;

        private Tilemap tilemap;
        private Actor actor;
        private Vector2 destinationPoint;
        private NavNode destinationNode;

        private List<NavNode> nodeList;
        private Vector2 segmentStart;
        private Vector2 segmentEnd;
        private float segmentDistance;
        private float segmentLength;

        private Task pathingTask;

        private float walkSpeed;

        private bool pathingError = false;

        public Vector2 DestinationPoint { get => destinationPoint; }

        public PathingController(PriorityLevel iPriorityLevel, Tilemap iTilemap, Actor iActor, Vector2 iDestination, float iWalkSpeed, int searchLimit = DEFAULT_SEARCH_LIMIT)
            : base(iPriorityLevel)
        {
            tilemap = iTilemap;
            actor = iActor;
            destinationPoint = iDestination;
            walkSpeed = iWalkSpeed;

            actor.ControllerList.Add(this);

            pathingTask = new Task(() => GeneratePath(PriorityLevel, searchLimit));
            pathingTask.Start();
        }

        public PathingController(PriorityLevel iPriorityLevel, Tilemap iMapScene, Actor iActor, Actor iDestination, float iWalkSpeed, int searchLimit = DEFAULT_SEARCH_LIMIT)
            : base(iPriorityLevel)
        {
            tilemap = iMapScene;
            actor = iActor;
            destinationNode = tilemap.GetNavNode(actor, iDestination);
            if (destinationNode == null) destinationPoint = iDestination.Center;
            else destinationPoint = destinationNode.Center;
            walkSpeed = iWalkSpeed;

            actor.ControllerList.Add(this);

            pathingTask = new Task(() => GeneratePath(PriorityLevel, searchLimit));
            pathingTask.Start();
        }

        private void GeneratePath(PriorityLevel priorityLevel, int searchLimit)
        {
            if (actor.IgnoreObstacles)
            {
                nodeList = new List<NavNode>();

                segmentStart = actor.Position;
                segmentEnd = destinationPoint;
                segmentLength = segmentDistance = Vector2.Distance(segmentStart, segmentEnd);

                return;
            }

            NavNode startNode = tilemap.GetNavNode(actor);
            NavNode endNode = (destinationNode == null) ? tilemap.GetNavNode(destinationPoint) : destinationNode;

            if (startNode != null) nodeList = tilemap.GetPath(startNode, endNode, actor, searchLimit);
            if (nodeList == null)
            {
                pathingError = true;
                Terminate();
            }
            else
            {
                if (nodeList.Count > 1 && nodeList[1].AccessibleFromActor(actor)) nodeList.RemoveAt(0);

                segmentStart = new Vector2(actor.Position.X + actor.BoundingBox.Left + actor.BoundingBox.Width / 2, actor.Position.Y + actor.BoundingBox.Bottom);
                segmentEnd = nodeList[0].Center;
                segmentLength = segmentDistance = Vector2.Distance(segmentStart, segmentEnd);
            }
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (!pathingTask.IsCompleted) return;

            if (nodeList == null || (nodeList.Count == 0 && segmentDistance <= 0.0f))
            {
                actor.Idle();
                Terminate();
                return;
            }

            segmentDistance -= gameTime.ElapsedGameTime.Milliseconds * walkSpeed / 1000.0f;

            RemoveOldNodes();
            AlignActorToPath();
        }

        public void AlignActorToPath()
        {
            if (!pathingTask.IsCompleted || Terminated) return;

            actor.Reorient(segmentEnd - segmentStart);

            float segmentProgress = segmentDistance / segmentLength;
            Vector2 pathPosition = Vector2.Lerp(segmentEnd, segmentStart, segmentProgress) - new Vector2(actor.BoundingBox.Left + actor.BoundingBox.Width / 2, actor.BoundingBox.Bottom);
            actor.Position = new Vector2(pathPosition.X, pathPosition.Y);
            actor.OrientedAnimation("Walk");
            actor.UpdateBounds();
        }

        public void RemoveOldNodes()
        {
            if (!pathingTask.IsCompleted) return;

            while (nodeList.Count > 0 && segmentDistance <= 0.0f)
            {
                segmentStart = nodeList[0].Center;
                nodeList.RemoveAt(0);

                if (nodeList.Count == 0)
                {
                    segmentEnd = (destinationNode == null) ? destinationPoint : destinationNode.Center;
                    segmentDistance += Vector2.Distance(segmentEnd, segmentStart);
                    segmentLength = Vector2.Distance(segmentEnd, segmentStart);
                    if (segmentLength < 0.001f) segmentLength = 1.0f;

                    return;
                }

                segmentEnd = nodeList[0].Center;
                segmentDistance += Vector2.Distance(segmentEnd, segmentStart);
                segmentLength = Vector2.Distance(segmentEnd, segmentStart);
            }
        }

        public bool PathingError { get => pathingError; }
    }
}
