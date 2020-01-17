﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nova.Gui.GText {

	public class Font {

		public string Name { get; }

		public int LineHeight { get; }

		private readonly Texture2D[] texturePages;
		private readonly Dictionary<char, GlyphData> glyphs;
		private readonly Dictionary<string, int> kerningPairs;
		private readonly GlyphData MissingCharacterGlyph;

		public Font(string path) {

			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			Name = doc.SelectSingleNode("/font/info").Attributes.GetNamedItem("face").Value;

			texturePages = new Texture2D[doc.SelectSingleNode("/font/pages").ChildNodes.Count];
			Log($"Found {texturePages.Length} pages");

			foreach (XmlNode node in doc.SelectNodes("/font/pages/page")) {
				string tn = node.Attributes.GetNamedItem("file").Value;
				int id = GetInt(node, "id");
				string p = $"Fonts/{Name}/{Path.GetFileNameWithoutExtension(tn)}";
				texturePages[id] = Engine.Instance.Content.Load<Texture2D>(p);
			}

			int charCount = GetInt(doc.SelectSingleNode("/font/chars"), "count");
			glyphs = new Dictionary<char, GlyphData>(charCount);

			foreach (XmlNode node in doc.SelectNodes("/font/chars/char")) {
				char c = (char)GetInt(node, "id");
				Rectangle rect = new Rectangle(GetInt(node, "x"), GetInt(node, "y"), GetInt(node, "width"), GetInt(node, "height"));
				Texture2D page = texturePages[GetInt(node, "page")];
				Point offset = new Point(GetInt(node, "xoffset"), GetInt(node, "yoffset"));
				int xadvance = GetInt(node, "xadvance");

				glyphs.Add(c, new GlyphData(c, page, rect, offset, xadvance));
			}

			Log($"Loaded {glyphs.Count} characters");

			MissingCharacterGlyph = glyphs[(char)164];
			Log($"Assigned missing character glyph to [{MissingCharacterGlyph.Character}] ({(int)MissingCharacterGlyph.Character})");

			int kerningCount = GetInt(doc.SelectSingleNode("/font/kernings"), "count");
			kerningPairs = new Dictionary<string, int>(kerningCount);

			foreach (XmlNode node in doc.SelectNodes("/font/kernings/kerning")) {
				string pair = $"{(char)GetInt(node, "first")}{(char)GetInt(node, "second")}";
				int kerning = GetInt(node, "amount");
				kerningPairs.Add(pair, kerning);
			}

			Log($"Loaded {kerningPairs.Count} kerning pairs");

			Log("Load complete.");
		}

		private int GetInt(XmlNode node, string attributeName) {
			return int.Parse(node.Attributes.GetNamedItem(attributeName).Value);
		}

		private void Log(string s) {
			Console.WriteLine($"[{Name}] {s}");
		}

		/// <summary>
		/// Retrieve a Glyph for the given character. Returns missing character glyph if character is not found.
		/// </summary>
		public Glyph GetGlyph(char c) {
			if (glyphs.TryGetValue(c, out GlyphData data)) {
				return new Glyph(data);
			} else {
				Log($"Warning: Attempted to load missing character [{c}] ({(int)c})");
				return new Glyph(MissingCharacterGlyph);
			}
		}

		internal int GetKerning(char left, char right) {
			if (kerningPairs.TryGetValue($"{left}{right}", out int amount)) {
				return amount;
			} else {
				return 0;
			}
		}

		public override string ToString() {
			return $"Font ({Name})";
		}

	}

}