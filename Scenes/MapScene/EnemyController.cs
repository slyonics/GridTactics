using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class EnemyController : ScriptController
    {
        private const float DEFAULT_WALK_LENGTH = 1.0f / 3;

        private MapScene mapScene;
        private Enemy enemy;

        private Tile currentTile;
        private Tile destinationTile;
        private float currentWalkLength;
        private float walkTimeLeft;

        public EnemyController(MapScene iScene, Enemy iEnemy)
            : base(iScene, iEnemy.IdleScript, PriorityLevel.GameLevel)
        {
            mapScene = iScene;
            enemy = iEnemy;

            currentTile = mapScene.Tilemap.GetTile(enemy.Center);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (!scriptParser.Finished) base.PreUpdate(gameTime);

            if (destinationTile == null)
            {
                enemy.DesiredVelocity = Vector2.Zero;
                enemy.OrientedAnimation("Idle");
            }
            else
            {
                enemy.DesiredVelocity = Vector2.Zero;
                enemy.Reorient(destinationTile.Center - currentTile.Center);
                enemy.OrientedAnimation("Walk");
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
                    enemy.CenterOn(new Vector2((int)npcPosition.X, (int)npcPosition.Y));
                }
                else
                {
                    enemy.CenterOn(destinationTile.Center);
                    currentTile = destinationTile;
                    destinationTile = null;
                }
            }
        }

        public bool Move(Orientation direction, float walkLength = DEFAULT_WALK_LENGTH)
        {
            enemy.Orientation = direction;

            int tileX = currentTile.TileX;
            int tileY = currentTile.TileY;
            switch (direction)
            {
                case Orientation.Up: tileY--; break;
                case Orientation.Right: tileX++; break;
                case Orientation.Down: tileY++; break;
                case Orientation.Left: tileX--; break;
            }

            Tile enemyDestination = mapScene.Tilemap.GetTile(tileX, tileY);
            if (enemyDestination == null) return false;
            if (!mapScene.Tilemap.CanTraverse(enemy, enemyDestination)) return false;

            destinationTile = enemyDestination;
            currentWalkLength = walkTimeLeft = walkLength;

            return true;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Wander": Move((Orientation)Rng.RandomInt(0, 3), int.Parse(tokens[1]) / 1000.0f); break;
                default: return false;
            }

            return true;
        }
    }
}
