using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using YiffBrowser.Models.E621;
using YiffBrowser.Services.Locals;

namespace YiffBrowser.Database {

	public static class E621DownloadDataAccess {

		public const string DatabaseFileName = "PostsInfo.db";

		public static SqliteConnection Connection { get; private set; }

		public static async ValueTask<SqliteConnection> OpenConnection(string filePath) {
			SqliteConnection db = new($"Filename={filePath}");
			await db.OpenAsync();
			return db;
		}

		public static async Task UpdateConnection() {
			if (Connection != null) {
				Connection.Close();
				Connection.Dispose();
			}

			Connection = await OpenConnection(Local.DatabaseFile.Path);
			await ValidateTables();
		}

		public static async Task ValidateTables() {
			if (Connection == null) {
				return;
			}
			await CreatePostsInfoTable(HostType.E621);
			await CreatePostsInfoTable(HostType.E926);
			await CreatePostsInfoTable(HostType.E6AI);
		}

		private static async Task CreatePostsInfoTable(HostType hostType) {
			string tableName = GetTableNameByType(hostType);
			string tableCommand =
				"CREATE TABLE IF NOT EXISTS " +
				$"{tableName}(" +
					"PostID INTEGER PRIMARY KEY, " +
					"PostJson TEXT NULL, " + //full info
					"Tags TEXT NULL, " + //partial info for quick search
					"Rating INT NULL, " +
					"Score INT NULL" +
				")";

			SqliteCommand createTable = new(tableCommand, Connection);
			await createTable.ExecuteReaderAsync();
		}

		private static string GetTableNameByType(HostType hostType) {
			return hostType switch {
				HostType.E926 => "E926",
				HostType.E621 => "E621",
				HostType.E6AI => "E6AI",
				_ => throw new NotImplementedException(),
			};
		}

		public static async Task AddOrUpdatePost(E621Post post) {
			HostType hostType = LocalSettings.StartHostType;
			string json = JsonConvert.SerializeObject(post);
			string tags = string.Join(",", post.Tags.GetAllTags());
			int rating = (int)post.Rating;
			int score = post.Score.Total;

			string tableName = GetTableNameByType(hostType);

			SqliteCommand insertCommand = new() {
				Connection = Connection,
				CommandText = $"INSERT OR REPLACE INTO {tableName} VALUES (@ID, @JSON, @TAGS, @RATING, @SCORE);"
			};

			insertCommand.Parameters.AddWithValue("@ID", post.ID);
			insertCommand.Parameters.AddWithValue("@JSON", json);
			insertCommand.Parameters.AddWithValue("@TAGS", tags);
			insertCommand.Parameters.AddWithValue("@RATING", rating);
			insertCommand.Parameters.AddWithValue("@SCORE", score);

			await insertCommand.ExecuteReaderAsync();

			Debug.WriteLine(post);
		}

		public static async ValueTask<E621Post> GetPostInfo(int postID) {
			HostType hostType = LocalSettings.StartHostType;
			string tableName = GetTableNameByType(hostType);
			SqliteCommand selectCommand = new($"SELECT PostID, PostJson FROM {tableName} WHERE PostID = {postID};", Connection);

			SqliteDataReader query = await selectCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);

			if (await query.ReadAsync()) {
				//int id = query.GetInt32(0);
				string json = query.GetString(1);
				E621Post post = JsonConvert.DeserializeObject<E621Post>(json);
				return post;
			}

			return null;
		}


	}
}
