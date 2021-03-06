﻿using System;
using System.Collections.Generic;
using ASCIIEngine.Core;
using ASCIIEngine.Core.BasicClasses;
using ASCIIEngine.Core.Components;
using FluentAssertions;
using Xunit;

namespace ASCIIEngine.UnitTests
{
    public class CollisionTests
    {
        [Fact]
        public void Collision_ShouldNotRegister()
        {
            var collided = false;

            var baseCore = new Base();

            baseCore.AddObject(new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 1)
            });

            baseCore.AddObject(new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 1),
                FireOnCollision = () => collided = true
            });


            baseCore.AddObject(new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 2),
                FireOnCollision = () => collided = true
            });

            baseCore.DoStep();

            collided.Should().BeFalse();
        }

        [Fact]
        public void IsCollisionDetected_ShouldReturnTrue()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 1)
            };

            var secondObject = new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 1)
            };

            var collided = CollisionHandler.IsCollisionDetected(firstObject, secondObject);

            collided.Should().BeTrue();
        }

        [Fact]
        public void IsCollisionDetected_ShouldReturnFalse()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            var secondObject = new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(1, 1)
            };

            var collided = CollisionHandler.IsCollisionDetected(firstObject, secondObject);

            collided.Should().BeFalse();
        }

        [Fact]
        public void ResolveCollisionsWithTwoRigidBodies_ShouldMoveObjectsToCorrectPositions()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, -1)
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Right;

            var secondObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 1)
            };

            secondObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Left;

            firstObject.Step();
            secondObject.Step();

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject});

            firstObject.Position.Should().BeEquivalentTo(new Vector2D(0, -1) + Vector2D.Right);
            secondObject.Position.Should().BeEquivalentTo(new Vector2D(0, 1) + Vector2D.Left);
        }

        [Fact]
        public void ResolveCollisionsWithTwoRigidBodies_ShouldDoNothing()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Left;

            var secondObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 1)
            };

            secondObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Right;

            firstObject.Step();
            secondObject.Step();

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject});

            firstObject.Position.Should().BeEquivalentTo(Vector2D.Left);
            secondObject.Position.Should().BeEquivalentTo(new Vector2D(0, 1) + Vector2D.Right);
        }

        [Fact]
        public void ResolveCollisionsWithRigidBodyAndStaticObject_ShouldMoveRigidBodyToCorrectPosition()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Right;

            var secondObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 1)
            };

            firstObject.Step();
            secondObject.Step();

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject});

            firstObject.Position.Should().BeEquivalentTo(Vector2D.Right);
            secondObject.Position.Should().BeEquivalentTo(new Vector2D(0, 1));
        }

        [Fact]
        public void ResolveCollisionsWithRigidBodyAndStaticObject_ShouldDoNothing()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Down;

            var secondObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 1)
            };

            firstObject.Step();
            secondObject.Step();

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject});

            firstObject.Position.Should().BeEquivalentTo(Vector2D.Down);
            secondObject.Position.Should().BeEquivalentTo(Vector2D.Up);
        }

        [Fact]
        public void ResolveConflictedCollidersWithoutDirection_ShouldMoveCollidersToDifferentPositions()
        {
            var firstObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Zero;

            var secondObject = new GameObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0)
            };

            secondObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Zero;

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject});

            firstObject.Position.Should().BeEquivalentTo(Vector2D.Left);
            secondObject.Position.Should().BeEquivalentTo(Vector2D.Down);
        }

        [Fact]
        public void ResolveThreeColliders_ShouldTriggerOnCollisionTwoTimeForEachObject()
        {
            int firstCounter = 0, secondCounter = 0, thirdCounter = 0;
            
            var firstObject = new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0),
                FireOnCollision = () => firstCounter++
            };

            firstObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Zero;

            var secondObject = new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0),
                FireOnCollision = () => secondCounter++
            };

            secondObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Zero;
            
            var thirdObject = new TestObject
            {
                Layer = 1,
                HasCollider = true,
                Position = new Vector2D(0, 0),
                FireOnCollision = () => thirdCounter++
            };
            
            thirdObject.AddComponent<RigidBody2D>().Velocity = Vector2D.Zero;

            CollisionHandler.ResolveCollisions(new[] {firstObject, secondObject, thirdObject});

            firstCounter.Should().Be(2);
            secondCounter.Should().Be(2);
            thirdCounter.Should().Be(2);
        }

        private class TestObject : GameObject
        {
            public Action FireOnCollision;

            public override void OnCollision(IEnumerable<GameObject> collidedWith) => FireOnCollision();
        }
    }
}