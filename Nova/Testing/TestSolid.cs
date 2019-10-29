﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nova.Input;
using Nova.PhysicsEngine;
using Nova.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nova {

	public class TestSolid : Entity {

		SolidRigidbody solid;
		BoxCollider boxCollider;

		Vector2 initPos;

		SimpleButton ToggleMove = new SimpleButton(Microsoft.Xna.Framework.Input.Keys.T);
		bool move;

		public TestSolid(Scene scene, Texture2D texture, Vector2 pos, Vector2 dimensions) :
			base(scene, pos) {

			Scale = dimensions;
			Position = pos;

			initPos = Position;

			new SpriteRenderer(this, texture, MDraw.DepthStartScene + 10);

			boxCollider = new BoxCollider(this, Vector2.Zero, dimensions);
			solid = new SolidRigidbody(this, boxCollider);
		}

		public override void Update() {

			//if (ToggleMove.JustPressed) {
			//	move = !move;
			//}

			move = true;

			if (move && InputManager.Any.Unleash.Pressed) {

				Vector2 inputs = new Vector2(InputManager.Any.Horizontal.Value, InputManager.Any.Vertical.Value);
				Calc.ClampMagnitude(ref inputs, 1f);

				solid.Velocity += 0.01f * inputs;

				//float scalar = MathHelper.TwoPi / 2f;
				//float y = 2 / (10 * scalar) * (float)Math.Cos(Time.TotalTime * scalar);

			}

			base.Update();
		}

	}

}