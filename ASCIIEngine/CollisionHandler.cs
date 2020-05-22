﻿using System.Collections.Generic;
using System.Linq;
using ASCIIEngine.Core.BasicClasses;
using ASCIIEngine.Core.Components;

namespace ASCIIEngine.Core
{
    public static class CollisionHandler
    {
        public static bool IsCollisionDetected(GameObject obj, GameObject other)
        {
            var max = obj.Size + obj.Position;
            var otherMax = other.Size + other.Position;

            // Exit without intersection because a dividing axis is found
            if (max.X < other.Position.X || obj.Position.X > otherMax.X)
                return false;

            if (max.Y < other.Position.Y || obj.Position.Y > otherMax.Y)
                return false;

            // No separation axis found, so at least one intersecting axis exists
            return true;
        }

        public static void ResolveCollisions(IEnumerable<GameObject> objects)
        {
            var checkedObjs = new List<GameObject>();
            var colliders = objects.ToList();
            foreach (var obj in colliders)
            {
                if (checkedObjs.Contains(obj))
                    continue;

                foreach (var other in colliders)
                {
                    if (obj.Equals(other))
                        continue;

                    if (IsCollisionDetected(obj, other))
                    {
                        // If both are static - ignore collision
                        if (obj.ContainsComponent<RigidBody2D>() || other.ContainsComponent<RigidBody2D>())
                        {
                            obj.OnCollision(new[] {other});
                            other.OnCollision(new[] {obj});

                            var colliderPositions = colliders.Select(c => c.Position).ToList();
                            ProcessRigidBody(obj, GetUnoccupiedCell(obj.Position, colliderPositions));
                            colliderPositions.Add(obj.Position);
                            ProcessRigidBody(other, GetUnoccupiedCell(other.Position, colliderPositions));
                            ResolveCollisions(colliders);
                        }
                    }

                    checkedObjs.Add(obj);
                    checkedObjs.Add(other);
                }
            }
        }

        private static void ProcessRigidBody(GameObject obj, Vector2D direction)
        {
            var rBody = obj.GetComponent<RigidBody2D>();
            if (rBody == null) 
                return;
            
            if (rBody.Velocity == Vector2D.Zero)
            {
                rBody.OnCollision(direction);
            }
            else
            {
                rBody.OnCollision();
            }
        }

        private static Vector2D GetUnoccupiedCell(Vector2D currentCell, IEnumerable<Vector2D> collidedCells)
        {
            var neighborCells = currentCell.GetNeighbors();
            var unoccupiedCells = neighborCells.Except(collidedCells).ToList();
            return unoccupiedCells.Count == 0 ? Vector2D.Zero : unoccupiedCells.First();
        }
    }
}