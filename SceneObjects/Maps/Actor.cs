using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GridTactics.SceneObjects.Maps
{
    public enum Orientation
    {
        Up,
        Right,
        Down,
        Left,

        None = -1,
    }

    public abstract class Actor : Entity
    {
        public const int ORIENTATION_COUNT = 4;

        public static Vector2[] ORIENTATION_UNIT_VECTORS = new Vector2[4] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
        protected static float[] ORIENTATION_ROTATIONS = new float[5] { 0.0f, (float)Math.PI / 2.0f, (float)Math.PI, (float)Math.PI * 3.0f / 2.0f, (float)Math.PI };

        protected Tilemap tilemap;

        protected Rectangle boundingBox;
        protected Rectangle currentBounds;
        protected bool ignoreObstacles = false;
        protected bool hitTerrain;

        protected int flightHeight;
        protected AnimatedSprite shadowSprite;

        protected Orientation orientation;
        private bool[] movementArcs = new bool[4];

        private Vector2 displacement;
        protected Vector2 blockedDisplacement;
        protected Vector2 desiredVelocity;

        protected List<Controller> controllerList = new List<Controller>();

        public Actor(Scene iScene, Tilemap iTilemap, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList, Rectangle iBounds, Orientation iOrientation = Orientation.None)
            : base(iScene, iPosition, iSprite, iAnimationList)
        {
            tilemap = iTilemap;

            boundingBox = iBounds;
            currentBounds = UpdateBounds(position);

            orientation = iOrientation;
        }

        public Actor(Scene iScene, Tilemap iTilemap, Vector2 iPosition, Rectangle iBounds, Orientation iOrientation = Orientation.None)
            : base(iScene, iPosition)
        {
            tilemap = iTilemap;

            boundingBox = iBounds;
            currentBounds = UpdateBounds(position);

            orientation = iOrientation;
        }

        public override void Update(GameTime gameTime)
        {
            controllerList.RemoveAll(x => x.Terminated);

            Vector2 startingPosition = position;
            velocity = desiredVelocity;

            Move(gameTime);
            //UpdateElevation(gameTime);
            positionZ = flightHeight;

            currentBounds = UpdateBounds(position);
            displacement = position - startingPosition;

            animatedSprite?.Update(gameTime);
            shadowSprite?.Update(gameTime);
        }

        public override void Move(GameTime gameTime)
        {
            hitTerrain = false;
            blockedDisplacement = Vector2.Zero;
            Displace(gameTime, tilemap);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            float depth = (camera == null) ? 0 : camera.GetDepth(DepthPosition);
            shadowSprite?.Draw(spriteBatch, position, camera, depth);
            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode")) Debug.DrawBox(spriteBatch, currentBounds);
        }

        private void Displace(GameTime gameTime, Tilemap tileMap)
        {
            if (ignoreObstacles)
            {
                position.X += velocity.X * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                position.Y += velocity.Y * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }
            else
            {
                Vector2 desiredDisplacement = velocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                Vector2 expectedPosition = position + desiredDisplacement;
                Vector2 initialExpectedPosition = expectedPosition;
                position = ConstrainedPosition(tileMap, desiredDisplacement);

                float slideDistance = (expectedPosition - position).Length();
                if (slideDistance > 0.001f && Math.Abs(velocity.X) > 0.001f && Math.Abs(velocity.Y) > 0.001f)
                {
                    currentBounds = UpdateBounds(position);
                    if (Math.Abs(velocity.Y) > 0.001f)
                    {
                        desiredDisplacement = new Vector2(0.0f, slideDistance * velocity.Y / Math.Abs(velocity.Y));
                        expectedPosition = position + desiredDisplacement;
                        position = ConstrainedPosition(tileMap, desiredDisplacement);
                    }

                    if (slideDistance > 0.001f)
                    {
                        if (Math.Abs(velocity.X) > 0.001f)
                        {
                            desiredDisplacement = new Vector2(slideDistance * velocity.X / Math.Abs(velocity.X), 0.0f);
                            expectedPosition = position + desiredDisplacement;
                            position = ConstrainedPosition(tileMap, desiredDisplacement);
                            slideDistance = (expectedPosition - position).Length() / 2.0f;
                        }
                    }
                }

                blockedDisplacement = initialExpectedPosition - position;
            }
        }

        private Vector2 ConstrainedPosition(Tilemap tilemap, Vector2 displacement)
        {
            Vector2 endPosition = position + displacement;
            Rectangle endBounds = UpdateBounds(endPosition);
            Rectangle displacementBounds = Rectangle.Union(currentBounds, endBounds);

            int startTileX = displacementBounds.Left / tilemap.TileWidth;
            int startTileY = displacementBounds.Top / tilemap.TileHeight;
            int endTileX = displacementBounds.Right / tilemap.TileWidth;
            int endTileY = displacementBounds.Bottom / tilemap.TileHeight;
            List<Rectangle> colliderList = new List<Rectangle>();
            List<Rectangle> entityColliders = ActorColliders;

            if (entityColliders != null) colliderList.AddRange(entityColliders);
            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    Tile tile = tilemap.GetTile(x, y);
                    if (tile != null) colliderList.AddRange(tile.ColliderList);
                }
            }

            Vector2 result = position + displacement;
            if (colliderList.Count == 0) return result;
            else
            {
                bool movingRight = displacement.X > 0.0f;
                bool movingLeft = displacement.X < 0.0f;
                bool movingDown = displacement.Y > 0.0f;
                bool movingUp = displacement.Y < 0.0f;

                float right = position.X + boundingBox.Right;
                float left = position.X + boundingBox.Left;
                float bottom = position.Y + boundingBox.Bottom;
                float top = position.Y + boundingBox.Top;

                float leadingStartX = -999;
                int constraintX = -999;
                if (movingRight) { leadingStartX = position.X + boundingBox.Right; constraintX = int.MaxValue; right += displacement.X; }
                if (movingLeft) { leadingStartX = position.X + boundingBox.Left; constraintX = int.MinValue; left += displacement.X; }
                float leadingEndX = leadingStartX + displacement.X;

                float leadingStartY = -999;
                int constraintY = -999;
                if (movingUp) { leadingStartY = position.Y + boundingBox.Top; constraintY = int.MinValue; top += displacement.Y; }
                if (movingDown) { leadingStartY = position.Y + boundingBox.Bottom; constraintY = int.MaxValue; bottom += displacement.Y; }
                float leadingEndY = leadingStartY + displacement.Y;

                bool constrainedX = false;
                bool constrainedY = false;

                foreach (Rectangle collider in colliderList)
                {
                    if (left > collider.Right) continue;
                    if (right < collider.Left) continue;
                    if (top > collider.Bottom) continue;
                    if (bottom < collider.Top) continue;

                    int blockingX = -999;
                    if (movingRight) blockingX = collider.Left;
                    if (movingLeft) blockingX = collider.Right;

                    int blockingY = -999;
                    if (movingUp) blockingY = collider.Bottom;
                    if (movingDown) blockingY = collider.Top;

                    if (movingRight && leadingStartX < blockingX && leadingEndX > blockingX && blockingX < constraintX) { constraintX = blockingX; constrainedX = true; }
                    if (movingLeft && leadingStartX > blockingX && leadingEndX < blockingX && blockingX > constraintX) { constraintX = blockingX; constrainedX = true; }
                    if (movingUp && leadingStartY > blockingY && leadingEndY < blockingY && blockingY > constraintY) { constraintY = blockingY; constrainedY = true; }
                    if (movingDown && leadingStartY < blockingY && leadingEndY > blockingY && blockingY < constraintY) { constraintY = blockingY; constrainedY = true; }
                }

                if (constrainedX && constrainedY)
                {
                    hitTerrain = true;

                    if (movingRight)
                    {
                        result = new Vector2(constraintX - boundingBox.Right - 0.001f, result.Y);
                        if (movingUp) result.Y = constraintY - boundingBox.Top + 0.001f;
                        if (movingDown) result.Y = constraintY - boundingBox.Bottom - 0.001f;
                    }
                    if (movingLeft)
                    {
                        result = new Vector2(constraintX - boundingBox.Left + 0.001f, result.Y);
                        if (movingUp) result.Y = constraintY - boundingBox.Top + 0.001f;
                        if (movingDown) result.Y = constraintY - boundingBox.Bottom - 0.001f;
                    }
                }
                else if (constrainedX)
                {
                    hitTerrain = true;

                    if (movingRight) result = new Vector2(constraintX - boundingBox.Right - 0.001f, result.Y);
                    if (movingLeft) result = new Vector2(constraintX - boundingBox.Left + 0.001f, result.Y);

                    float movementInterval = (Math.Abs(displacement.X) - Math.Abs(result.X - endPosition.X)) / Math.Abs(displacement.X);
                    result.Y = position.Y + displacement.Y * movementInterval;
                }
                else if (constrainedY)
                {
                    hitTerrain = true;

                    if (movingUp) result = new Vector2(result.X, constraintY - boundingBox.Top + 0.001f);
                    if (movingDown) result = new Vector2(result.X, constraintY - boundingBox.Bottom - 0.001f);

                    float movementInterval = (Math.Abs(displacement.Y) - Math.Abs(result.Y - endPosition.Y)) / Math.Abs(displacement.Y);
                    result.X = position.X + displacement.X * movementInterval;
                }

                return result;
            }
        }

        protected Rectangle UpdateBounds(Vector2 boxPosition)
        {
            return new Rectangle((int)boxPosition.X + boundingBox.X, (int)boxPosition.Y + boundingBox.Y, boundingBox.Width, boundingBox.Height);
        }

        protected Rectangle UpdateBounds(Vector2 boxPosition, float rotation)
        {
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);

            Vector2 leftTop = new Vector2(boundingBox.Left, boundingBox.Top);
            Vector2 rightTop = new Vector2(boundingBox.Right, boundingBox.Top);
            Vector2 leftBottom = new Vector2(boundingBox.Left, boundingBox.Bottom);
            Vector2 rightBottom = new Vector2(boundingBox.Right, boundingBox.Bottom);

            Vector2.Transform(ref leftTop, ref rotationMatrix, out leftTop);
            Vector2.Transform(ref rightTop, ref rotationMatrix, out rightTop);
            Vector2.Transform(ref leftBottom, ref rotationMatrix, out leftBottom);
            Vector2.Transform(ref rightBottom, ref rotationMatrix, out rightBottom);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            Rectangle rotatedBoundingBox = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

            return new Rectangle((int)boxPosition.X + rotatedBoundingBox.X, (int)boxPosition.Y + rotatedBoundingBox.Y, rotatedBoundingBox.Width, rotatedBoundingBox.Height);
        }

        public void UpdateBounds()
        {
            currentBounds = UpdateBounds(position);
        }

        public virtual void Reorient(Vector2 movement)
        {
            if (movement.Length() < 0.0001f) return;

            float rotation = (float)Math.Atan2(movement.Y, movement.X) + (float)Math.PI / 2.0f;
            for (int i = 0; i < movementArcs.Length; i++)
            {
                float deltaRotation = Math.Min(Math.Abs(rotation - ORIENTATION_ROTATIONS[i]), (float)Math.PI * 2.0f - Math.Abs(rotation - ORIENTATION_ROTATIONS[i]));
                bool newMovementArc = deltaRotation < Math.PI / 2.0f * 0.9f;
                if (newMovementArc && !movementArcs[i]) orientation = (Orientation)i;

                movementArcs[i] = newMovementArc;
            }

            if (Math.Abs(movement.X) > Math.Abs(movement.Y) * 1.05f)
            {
                if (movement.X > 0.0f) orientation = Orientation.Right;
                else orientation = Orientation.Left;
            }
            else if (Math.Abs(movement.Y) > Math.Abs(movement.X) * 1.05f)
            {
                if (movement.Y > 0.0f) orientation = Orientation.Down;
                else orientation = Orientation.Up;
            }
        }

        public virtual void Reorient(Orientation newOrientation)
        {
            orientation = newOrientation;
            for (int i = 0; i < movementArcs.Length; i++) movementArcs[i] = false;
        }

        public float Distance(Rectangle rectangle)
        {
            if (currentBounds.Intersects(rectangle)) return 0.0f;

            float x = 0;
            if (currentBounds.Left - rectangle.Right > 0) x = currentBounds.Left - rectangle.Right;
            else if (rectangle.Left - currentBounds.Right > 0) x = rectangle.Left - currentBounds.Right;

            float y = 0;
            if (currentBounds.Top - rectangle.Bottom > 0) y = currentBounds.Top - rectangle.Bottom;
            else if (rectangle.Top - currentBounds.Bottom > 0) y = rectangle.Top - currentBounds.Bottom;

            return (float)Math.Sqrt(x * x + y * y);
        }

        public float Distance(Actor actor)
        {
            return Distance(actor.Bounds);
        }

        public virtual void Idle()
        {
            Running = false;

            desiredVelocity = Vector2.Zero;

            animatedSprite.PlayAnimation("Idle" + orientation.ToString());
        }

        public virtual void Walk(Vector2 movement, float walkSpeed)
        {
            Running = false;

            desiredVelocity = movement * walkSpeed;

            Reorient(movement);

            animatedSprite.PlayAnimation("Walk" + orientation.ToString());
        }

        public virtual void Run(Vector2 movement, float runSpeed)
        {
            Running = true;

            desiredVelocity = movement * runSpeed;

            Reorient(movement);

            animatedSprite.PlayAnimation("Run" + orientation.ToString());
        }

        public virtual void Teleport(Vector2 destination)
        {
            position = new Vector2(destination.X + boundingBox.Left + boundingBox.Width / 2, destination.Y + boundingBox.Bottom);
        }

        public virtual void CenterOn(Vector2 destination)
        {
            position = new Vector2(destination.X, destination.Y + 8);
            UpdateBounds();
        }

        public virtual void OrientedAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            PlayAnimation(animationName + orientation.ToString(), animationFollowup);
        }

        public void SetFlight(int newHeight, Texture2D shadow)
        {
            shadowSprite = new AnimatedSprite(shadow, new Dictionary<string, Animation>(animatedSprite.AnimationList));
            positionZ = flightHeight = newHeight;
        }

        public override Vector2 Position { set { position = value; } }
        public Rectangle BoundingBox { get => boundingBox; }
        public Rectangle Bounds { get => currentBounds; }
        public Vector2 Center { get => new Vector2((currentBounds.Left + currentBounds.Right) / 2, currentBounds.Center.Y); }
        public Vector2 Bottom { get => new Vector2((currentBounds.Left + currentBounds.Right) / 2, currentBounds.Bottom); }
        public override float DepthPosition { get => currentBounds.Bottom; }

        public virtual List<Rectangle> ActorColliders { get => null; }
        public List<Rectangle> NearbyColliders
        {
            get
            {
                int tileStartX = currentBounds.Left / tilemap.TileWidth - 1;
                int tileEndX = currentBounds.Right / tilemap.TileWidth + 1;
                int tileStartY = currentBounds.Top / tilemap.TileHeight - 1;
                int tileEndY = currentBounds.Bottom / tilemap.TileHeight + 1;

                List<Rectangle> colliderList = new List<Rectangle>();
                for (int x = tileStartX; x <= tileEndX; x++)
                {
                    for (int y = tileStartY; y <= tileEndY; y++)
                    {
                        Tile nearbyTile = tilemap.GetTile(x, y);
                        if (nearbyTile != null) colliderList.AddRange(tilemap.GetTile(x, y).ColliderList);
                    }
                }

                return colliderList;
            }
        }

        public bool Visible { get => parentScene.Camera.View.Intersects(currentBounds); }
        public bool IgnoreObstacles { get => ignoreObstacles; }
        public Orientation Orientation { get => orientation; set => orientation = value; }
        public Vector2 Displacement { get => displacement; }
        public Vector2 BlockedDisplacement { get => blockedDisplacement; }
        public Vector2 DesiredVelocity { get => desiredVelocity; set => desiredVelocity = value; }
        public List<Controller> ControllerList { get => controllerList; }

        public virtual Vector2 ShootPosition
        {
            get
            {
                switch (orientation)
                {
                    case Orientation.Up: return new Vector2(currentBounds.Center.X, currentBounds.Top);
                    case Orientation.Right: return new Vector2(currentBounds.Right, currentBounds.Center.Y);
                    case Orientation.Down: return new Vector2(currentBounds.Center.X, currentBounds.Bottom);
                    case Orientation.Left: return new Vector2(currentBounds.Left, currentBounds.Center.Y);
                    default: return position;
                }
            }
        }

        public bool Running { get; set; }
    }
}
