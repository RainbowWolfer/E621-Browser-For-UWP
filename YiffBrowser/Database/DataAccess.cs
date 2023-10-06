using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;

namespace YiffBrowser.Database {
	public static class DataAccess {
		public async static void InitializeDatabase() {
			await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
			string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
			using SqliteConnection db = new($"Filename={dbPath}");
			db.Open();

			string tableCommand = "CREATE TABLE IF NOT EXISTS MyTable (Primary_Key INTEGER PRIMARY KEY, Text_Entry NVARCHAR(2048) NULL)";

			SqliteCommand createTable = new(tableCommand, db);

			createTable.ExecuteReader();
		}

		public static void AddData(string inputText) {
			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
			using SqliteConnection db = new($"Filename={dbpath}");
			db.Open();

			SqliteCommand insertCommand = new() {
				Connection = db,
				// Use parameterized query to prevent SQL injection attacks
				CommandText = "INSERT INTO MyTable VALUES (NULL, @Entry);"
			};

			insertCommand.Parameters.AddWithValue("@Entry", inputText);

			insertCommand.ExecuteReader();

		}

		public static List<String> GetData() {
			List<string> entries = new();

			string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
			using (SqliteConnection db = new($"Filename={dbpath}")) {
				db.Open();

				SqliteCommand selectCommand = new("SELECT Text_Entry from MyTable", db);

				SqliteDataReader query = selectCommand.ExecuteReader();

				while (query.Read()) {
					entries.Add(query.GetString(0));
				}
			}

			return entries;
		}
	}
}
