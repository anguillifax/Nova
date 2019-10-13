﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nova.PhysicsEngine {

	/// <summary>
	/// Defines the mathematics behind all possible collisions between different types of colliders.
	/// Adapted from Christer Ericson's Real-Time Collision Book.
	/// <para>Function Notation: OperationName_WhatAgainst_What()</para>
	/// </summary>
	public static class PhysicsMath {

		public const float Epsilon = 1e-15f;

		#region Overlap XX against XX

		/// <summary>
		/// Tests if A is overlapping B. If A and B are exactly aligned on edges, this function also returns true.
		/// </summary>
		public static bool IsOverlapping_Box_Box(BoxCollider a, BoxCollider b) {
			if (Math.Abs(a.Position.X - b.Position.X) > a.Extents.X + b.Extents.X) return false;
			if (Math.Abs(a.Position.Y - b.Position.Y) > a.Extents.Y + b.Extents.Y) return false;
			return true;
		}

		/// <summary>
		/// Tests if A is inside of B. If A and B are exactly aligned on edges, this function returns false.
		/// </summary>
		public static bool IsInside_Box_Box(BoxCollider a, BoxCollider b) {
			if (Math.Abs(a.Position.X - b.Position.X) >= a.Extents.X + b.Extents.X) return false;
			if (Math.Abs(a.Position.Y - b.Position.Y) >= a.Extents.Y + b.Extents.Y) return false;
			return true;
		}

		public static bool IsOverlapping_Circle_Circle(CircleCollider a, CircleCollider b) {
			Vector2 d = a.Position - b.Position;
			float distSquared = d.LengthSquared();
			float radiusSum = a.Radius + b.Radius;
			return distSquared <= radiusSum * radiusSum;
		}

		public static bool IsInside_Circle_Circle(CircleCollider a, CircleCollider b) {
			Vector2 d = a.Position - b.Position;
			float distSquared = d.LengthSquared();
			float radiusSum = a.Radius + b.Radius;
			return distSquared < radiusSum * radiusSum;
		}

		public static bool IsOverlapping_Box_Circle(BoxCollider box, CircleCollider circle) {
			return SquareDistanceBetweenPointAndBox(circle.Position, box) <= circle.Radius * circle.Radius;
		}


		public static bool IsInside_Box_Circle(BoxCollider box, CircleCollider circle) {
			return SquareDistanceBetweenPointAndBox(circle.Position, box) < circle.Radius * circle.Radius;
		}

		#endregion

		#region Closest Point and Distance Mathematics

		/// <summary>
		/// Returns the closest point on the boundary of the box or inside the box.
		/// </summary>
		public static Vector2 ClosestPointOnBox(Vector2 point, BoxCollider box) {
			if (point.X < box.Min.X) point.X = box.Min.X;
			if (point.X > box.Max.X) point.X = box.Max.X;

			if (point.Y < box.Min.Y) point.Y = box.Min.Y;
			if (point.Y > box.Max.Y) point.Y = box.Max.Y;

			return point;
		}

		public static float SquareDistanceBetweenPointAndBox(Vector2 point, BoxCollider box) {
			float sqDist = 0f;

			if (point.X < box.Min.X) sqDist += Squared(box.Min.X - point.X);
			if (point.X > box.Max.X) sqDist = Squared(point.X - box.Max.X);

			if (point.Y < box.Min.Y) sqDist += Squared(box.Min.Y - point.Y);
			if (point.Y > box.Max.Y) sqDist = Squared(point.Y - box.Max.Y);

			return sqDist;
		}

		/// <summary>
		/// Returns a point on a plane defined by normal and passing through the origin.
		/// </summary>
		public static Vector2 GetClosestPointOnOriginPlane(Vector2 point, Vector2 normal) {
			normal.Normalize();
			return point - Vector2.Dot(normal, point) * normal;
		}

		#endregion

		#region Normals

		/// <summary>
		/// Get the normal of where 'of' touches 'against.' Do not use if 'of' is inside 'against'.
		/// </summary>
		public static Vector2 GetNormal_Box_Box(BoxCollider of, BoxCollider against) {

			Vector2 normal = Vector2.Zero;

			if (against.Max.X <= of.Min.X) normal.X = 1f;
			if (against.Min.X >= of.Max.X) normal.X = -1f;

			if (against.Max.Y <= of.Min.Y) normal.Y = 1f;
			if (against.Min.Y >= of.Max.Y) normal.Y = -1f;

			normal.Normalize();

			return normal;
		}

		public static Vector2 GetNormal_Box(Vector2 point, BoxCollider box) {

			Vector2 normal = Vector2.Zero;

			if (point.X < box.Position.X) normal.X = -1;
			if (point.X > box.Position.X) normal.X = 1;

			if (point.Y < box.Position.Y) normal.Y = -1;
			if (point.Y > box.Position.Y) normal.Y = 1;

			return normal.LengthSquared() == 0 ? Vector2.UnitX : normal.GetNormalized();
		}

		public static Vector2 GetNormal_Circle(Vector2 point, CircleCollider circle) {
			var normal = point - circle.Position;
			return normal.LengthSquared() == 0 ? Vector2.UnitX : normal.GetNormalized();
		}

		#endregion

		#region Moving Objects

		/// <summary>
		/// Returns time when two moving boxes overlap. If A overlaps B, then it is considered an intersection.
		/// </summary>
		public static bool IntersectMoving_Box_Box(BoxCollider a, BoxCollider b, Vector2 velA, Vector2 velB, out float firstTimeOfContact) {
			if (IsOverlapping_Box_Box(a, b)) {
				firstTimeOfContact = 0f;
				return true;
			} else {
				return IntersectMoving_Box_Box_NoOverlap(a, b, velA, velB, out firstTimeOfContact);
			}
		}

		/// <summary>
		/// Returns time when two moving boxes overlap. If A overlaps B, then it is NOT considered an intersection.
		/// </summary>
		public static bool IntersectMoving_Box_Box_NoOverlap(BoxCollider a, BoxCollider b, Vector2 velA, Vector2 velB, out float firstTimeOfContact) {

			// If A is stationary, v is relative velocity of B
			Vector2 v = velB - velA;

			firstTimeOfContact = 1f;
			bool hit = false;

			if (v.X != 0) {

				if (b.Max.X <= a.Min.X && v.X < 0f) {
					// B is moving away from A to the left. no intersection
					return false;

				} else if (b.Min.X >= a.Max.X && v.X > 0f) {
					// B is moving away from A to the right. no intersection
					return false;

				} else {
					// B is approaching A on x axis. possible intersection

					float t;
					if (v.X > 0f) {
						t = (a.Min.X - b.Max.X) / v.X;
					} else {
						t = (a.Max.X - b.Min.X) / v.X;
					}

					if (0f <= t && t <= 1f) {

						float projectionY = v.Y * t;

						if (b.Min.Y + projectionY < a.Max.Y && b.Max.Y + projectionY > a.Min.Y) {
							// y component of projection will overlap if B moves to time t
							firstTimeOfContact = Math.Min(t, firstTimeOfContact);
							hit = true;
						}

					}

				}

			}

			if (v.Y != 0) {

				if (b.Max.Y <= a.Min.Y && v.Y < 0f) {
					// B is moving away from A downwards. no intersection
					return false;

				} else if (b.Min.Y >= a.Max.Y && v.Y > 0f) {
					// B is moving away from A upwards. no intersection
					return false;

				} else {
					// B is approaching A on y axis. possible intersection

					float t;

					if (v.Y > 0f) {
						t = (a.Min.Y - b.Max.Y) / v.Y;
					} else {
						t = (a.Max.Y - b.Min.Y) / v.Y;
					}

					if (0f <= t && t <= 1f) {

						float projectionX = v.X * t;

						if (a.Min.X < b.Max.X + projectionX && a.Max.X > b.Min.X + projectionX) {
							// x component of projection will overlap if B moves to time t
							firstTimeOfContact = Math.Min(t, firstTimeOfContact);
							hit = true;
						}

					}

				}

			}

			return hit;
		}

		// Circle against circle p225

		// Circle against box p229

		#endregion

		#region Overlap Resolution (Depenetration)

		/// <summary>
		/// Calculate the closest point where A is outside of B. Do not use if A is overlapping/outside B.
		/// </summary>
		public static Vector2 Depenetrate_Box_Box(BoxCollider a, BoxCollider b) {

			if (a.Position == b.Position) {
				// Objects are exactly on top of each other. Push A above B.
				return b.Position + new Vector2(0, b.Extents.Y + a.Extents.Y);
			}

			float dx = 0f;
			float dy = 0f;

			if (a.Position.X > b.Position.X) dx = b.Max.X - a.Min.X;
			if (a.Position.X < b.Position.X) dx = b.Min.X - a.Max.X;
			if (a.Position.X == b.Position.X) dx = float.MaxValue;

			if (a.Position.Y > b.Position.Y) dy = b.Max.Y - a.Min.Y;
			if (a.Position.Y < b.Position.Y) dy = b.Min.Y - a.Max.Y;
			if (a.Position.Y == b.Position.Y) dy = float.MaxValue;

			if (Math.Abs(dx) < Math.Abs(dy)) {
				return a.Position + new Vector2(dx, 0);
			} else {
				return a.Position + new Vector2(0, dy);
			}
		}

		#endregion

		#region Raycasts (TODO)

		// Sphere p217

		// Box p219

		#endregion

		#region Specialized

		/// <summary>
		/// If vel moves into plane defined by normal, gives point on plane instead. Otherwise returns vel unchanged.
		/// </summary>
		public static Vector2 GetAllowedVelocity(Vector2 vel, Vector2 normal) {

			normal.Normalize();
			var dot = Vector2.Dot(vel, normal);

			if (dot < 0) {
				// point is below plane. snap to plane
				return vel - dot * normal;
			} else {
				return vel;
			}
		}

		#endregion

		/// <summary>
		/// Returns true if given colliders are null or inactive.
		/// </summary>
		private static bool CannotCollide(Collider a, Collider b) {
			return a == null || b == null || !a.Active || !b.Active;
		}

		/// <summary>
		/// Returns the squared value of v. Created exclusively to help improve mathematical
		/// clarity within this class.
		/// </summary>
		private static float Squared(float v) {
			return v * v;
		}

	}

}