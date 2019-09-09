﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Nova.Input {

	public static class DebugGUI_Rebind {

		public static bool IsOpen { get; set; }

		public static int SelectionIndex { get; private set; }
		public static bool IsEditingKeyboard { get; private set; }

		private static int MaxButtons;

		public static void Update() {

			if (InputManager.RebindingPanel.JustPressed) {
				IsOpen = !IsOpen;
				IsEditingKeyboard = false;
				enterReleased = true;

				if (!IsOpen) {
					InputManager.SaveBindings();
				}
			}

			if (IsOpen) {
				MaxButtons = InputManager.SourceKeyboard.AllButtons.Count;
				if (Keyboard.GetState().IsKeyUp(Keys.Enter)) enterReleased = true;

				if (IsOpen) {
					if (!IsEditingKeyboard && InputManager.Any.Enter.JustPressed) {
						Console.WriteLine("edit");
						IsEditingKeyboard = true;
						enterReleased = false;
						RebindingManagerKeyboard.RegisterAlreadyPressedKeys(Keyboard.GetState().GetPressedKeys());
					} else if (IsEditingKeyboard && GetHardcodedEnter()) {
						Console.WriteLine("stop");
						IsEditingKeyboard = false;
					}
				}

				if (!IsEditingKeyboard) {
					int delta = 0;
					//if (InputManager.Vertical.RepeaterPos) {
					//	delta = -1;
					//}
					if (InputManager.Cycle.JustPressed) {
						delta = +1;
					}
					if (delta != 0) {
						SelectionIndex = Calc.Loop(SelectionIndex + delta, 0, MaxButtons);
					}
				}

				RebindingManagerKeyboard.Target = InputManager.SourceKeyboard.AllButtons[SelectionIndex];

				if (IsEditingKeyboard) {

					RebindingManagerKeyboard.Update();
					if (InputManager.Any.Clear.JustPressed) {
						RebindingManagerKeyboard.Unbind();
					}

				}

				//	if (IsOpen) {

				//		

				//		if (!IsEditingKeyboard) {
				//			
				//		}

				//		RebindingManager.Target = InputManager.AllButtons[SelectionIndex];
				//		if (IsEditingKeyboard) {
				//			RebindingManager.SearchKeyboard();
				//			if (InputManager.Clear.SourceKeyboardDefault.JustPressed) {
				//				RebindingManager.Target.SourceKeyboard.Unbind();
				//			}
				//		} else {
				//			if (InputManager.Clear.SourceKeyboard.JustPressed || InputManager.Clear.SourceKeyboardDefault.JustPressed) {
				//				RebindingManager.Target.SourceKeyboard.Unbind();
				//			}
				//		}

				//		RebindingManager.SearchGamepad();
				//		if (InputManager.Clear.SourceGamepad.JustPressed && RebindingManager.Target.GamepadRebindable) {
				//			RebindingManager.Target.SourceGamepad.UnbindAll();
				//		}

				//	}

				//}
			}
		}

		static bool enterReleased;
		static bool GetHardcodedEnter() {
			if (enterReleased) {
				var s = Keyboard.GetState();
				return s.IsKeyDown((Keys)InputManager.SourceKeyboard.Enter.HardcodedKey);
			}
			return false;
		}

		static SpriteBatch b;

		public static void Draw() {

			if (!IsOpen) return;

			b = DrawManager.SpriteBatch;

			b.Begin();

			var pos = new Vector2(100, 20f);
			float firstSpace = 300;
			float inputSpace = 200;

			Write("Rebind Panel", pos, Color.White);
			pos.Y += 50f;
			Write("KB User", new Vector2(pos.X + firstSpace, pos.Y), Color.White);
			Write("KB Def", new Vector2(pos.X + firstSpace + inputSpace, pos.Y), Color.White);
			Write("Gamepad1", new Vector2(pos.X + firstSpace + 2 * inputSpace, pos.Y), Color.White);
			Write("Gamepad2", new Vector2(pos.X + firstSpace + 3 * inputSpace, pos.Y), Color.White);

			pos.Y += 60f;

			for (int i = 0; i < MaxButtons; i++) {

				bool active = SelectionIndex == i;
				Color color = active ? (IsEditingKeyboard ? Color.Lime : Color.Yellow) : (IsEditingKeyboard ? Color.Gray : Color.White);

				Vector2 rowPos = pos.Copy();

				if (active) {
					rowPos.X += 5f;
				}

				Write(InputManager.SourceKeyboard.AllButtons[i].Name, rowPos, color);

				rowPos.X += firstSpace;
				var kb = InputManager.SourceKeyboard.AllButtons[i];
				if (kb.UserKey != null) Write(kb.UserKey.ToString(), rowPos, color);

				rowPos.X += inputSpace;
				if (kb.HardcodedKey != null) Write(kb.HardcodedKey.ToString(), rowPos, color);

				rowPos.X += inputSpace;
				var gp1 = InputManager.SourceGamepad1.AllButtons[i];
				if (gp1.ButtonList.Count > 0) Write(PrintFormatter.ListToString(gp1.ButtonList), rowPos, color);

				rowPos.X += inputSpace;
				var gp2 = InputManager.SourceGamepad2.AllButtons[i];
				if (gp2.ButtonList.Count > 0) Write(PrintFormatter.ListToString(gp2.ButtonList), rowPos, color);

				pos.Y += 50f;

			}

			b.End();
		}

		private static void Write(string text, Vector2 pos, Color color) {
			b.DrawString(Engine.DefaultFont, text, pos, color);
		}

	}

}