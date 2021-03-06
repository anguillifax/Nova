﻿using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace Nova {

	/// <summary>
	/// Serialize and Deserialize files to the disk
	/// </summary>
	public static class SaveLoad {

		public static void Save<T>(string path, T obj) {

			Directory.CreateDirectory(Path.GetDirectoryName(path));

			using (var sw = new StreamWriter(path)) {
				sw.Write(JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented));
			}

		}

		public static T Load<T>(string path) {

			if (!File.Exists(path)) throw new FileNotFoundException();

			using (var sr = new StreamReader(path)) {
				return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
			}

		}

		public static XmlDocument ReadXml(string path) {

			if (File.Exists(path)) {

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(File.ReadAllText(path));

				return doc;

			} else {
				throw new FileNotFoundException();
			}

		}

	}

}