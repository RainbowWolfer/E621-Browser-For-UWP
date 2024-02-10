﻿using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Database {
	public static class E621DownloadDataAccess {

		public const string DatabaseFileName = "PostsInfo.db";

		public static async ValueTask<StorageFile> CheckDatabase(CancellationToken token = default) {
			StorageFolder folder = Local.DownloadFolder;
			if (folder == null) {
				return null;
			}
			try {
				StorageFile file = await folder.GetFileAsync(DatabaseFileName);
				file ??= await CreateDatabase(folder, token);
				//await CheckColumns(file);
				return file;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				StorageFile file = await CreateDatabase(folder, token);
				//await CheckColumns(file);
				return file;
			}
		}

		public async static ValueTask<StorageFile> CreateDatabase(IStorageFolder folder, CancellationToken token = default) {
			StorageFile file = await folder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists);

			using SqliteConnection connection = await OpenConnection(file.Path, token);

			string tableCommand =
				"CREATE TABLE IF NOT EXISTS " +
				"PostsInfo(" +
					"PostID INTEGER PRIMARY KEY, " +
					"PostJson TEXT NULL," + //full info
					"Tags TEXT NULL," + //partial info for quick search
					"Rating INT NULL" +
					"Score INT NULL" +
				")";

			SqliteCommand createTable = new(tableCommand, connection);

			await createTable.ExecuteReaderAsync(token);

			return file;
		}

		public static async Task CheckColumns(StorageFile file) {
			using SqliteConnection connection = await OpenConnection(file.Path);

			string[] sqls = [
				"ALTER TABLE PostsInfo ADD PostJson TEXT;",
				"ALTER TABLE PostsInfo ADD Tags TEXT;",
				"ALTER TABLE PostsInfo ADD Rating INT;",
				"ALTER TABLE PostsInfo ADD Score INT;",
			];

			foreach (string item in sqls) {
				try {
					using SqliteCommand command = new(item, connection);
					await command.ExecuteNonQueryAsync();
				} catch { }
			}


		}

		public static async Task AddOrUpdatePost(E621Post post, CancellationToken token = default) {
			StorageFile file = await CheckDatabase(token);
			if (file == null) {
				return;
			}
			if (post == null) {
				return;
			}

			string json = JsonConvert.SerializeObject(post);
			string tags = string.Join(",", post.Tags.GetAllTags());
			int rating = (int)post.Rating;
			int score = post.Score.Total;

			using SqliteConnection connection = await OpenConnection(file.Path, token);

			SqliteCommand insertCommand = new() {
				Connection = connection,
				CommandText = "INSERT OR REPLACE INTO PostsInfo VALUES (@ID, @JSON, @TAGS, @RATING, @SCORE);"
			};

			insertCommand.Parameters.AddWithValue("@ID", post.ID);
			insertCommand.Parameters.AddWithValue("@JSON", json);
			insertCommand.Parameters.AddWithValue("@TAGS", tags);
			insertCommand.Parameters.AddWithValue("@RATING", rating);
			insertCommand.Parameters.AddWithValue("@SCORE", score);

			await insertCommand.ExecuteReaderAsync(token);
		}

		public static async ValueTask<E621Post> GetPostInfo(int postID, CancellationToken token = default) {
			StorageFile file = await CheckDatabase(token);
			if (file == null) {
				return null;
			}

			using SqliteConnection connection = await OpenConnection(file.Path, token);

			SqliteCommand selectCommand = new($"SELECT PostID, PostJson FROM PostsInfo WHERE PostID = {postID};", connection);

			SqliteDataReader query = await selectCommand.ExecuteReaderAsync(CommandBehavior.SingleResult, token);

			if (await query.ReadAsync(token)) {
				int id = query.GetInt32(0);
				string json = query.GetString(1);
				E621Post post = JsonConvert.DeserializeObject<E621Post>(json);
				return post;
			}

			return null;
		}


		public static async ValueTask<SqliteConnection> OpenConnection(string filePath, CancellationToken token = default) {
			SqliteConnection db = new($"Filename={filePath}");
			await db.OpenAsync(token);
			return db;
		}

	}
}
