using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using YiffBrowser.Helpers;

namespace YiffBrowser.Models.E621 {
	public class E621Post {

		[JsonProperty("id")]
		public int ID { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }

		[JsonProperty("file")]
		public File File { get; set; }

		[JsonProperty("preview")]
		public Preview Preview { get; set; }

		[JsonProperty("sample")]
		public Sample Sample { get; set; }

		[JsonProperty("score")]
		public Score Score { get; set; }

		[JsonProperty("tags")]
		public Tags Tags { get; set; }

		[JsonProperty("locked_tags")]
		public List<string> LockedTags { get; set; }

		[JsonProperty("change_seq")]
		public int ChangeSeq { get; set; }

		[JsonProperty("flags")]
		public Flags Flags { get; set; }

		[JsonProperty("rating")]
		public E621Rating Rating { get; set; }

		[JsonProperty("fav_count")]
		public int FavCount { get; set; }

		[JsonProperty("sources")]
		public List<string> Sources { get; set; }

		[JsonProperty("pools")]
		public List<string> Pools { get; set; }

		[JsonProperty("relationships")]
		public Relationships Relationships { get; set; }

		[JsonProperty("approver_id")]
		public string ApproverId { get; set; }

		[JsonProperty("uploader_id")]
		public int UploaderId { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("comment_count")]
		public int CommentCount { get; set; }

		[JsonProperty("is_favorited")]
		public bool IsFavorited { get; set; }

		[JsonProperty("has_notes")]
		public bool HasNotes { get; set; }

		[JsonProperty("duration")]
		public string Duration { get; set; }

		#region Additional

		public bool HasVotedUp { get; set; }
		public bool HasVotedDown { get; set; }

		#endregion


		public bool HasNoValidURLs() {
			if (Preview == null || Sample == null || File == null) {
				return true;
			}
			return Preview.URL.IsBlank() || Sample.URL.IsBlank() || File.URL.IsBlank();
		}

		public override string ToString() {
			return $"E621Post ({ID}.{File.Ext})";
		}
	}

	public class E621PostsRoot {
		[JsonProperty("posts")]
		public List<E621Post> Posts { get; set; }

		[JsonProperty("post")]
		public E621Post Post { get; set; }
	}

	public class File {
		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("ext")]
		public string Ext { get; set; }

		[JsonProperty("size")]
		public long Size { get; set; }

		[JsonProperty("md5")]
		public string Md5 { get; set; }

		[JsonProperty("url")]
		public string URL { get; set; }

		[JsonIgnore]
		public string SizeInfo => $"{Width} × {Height} ({Size.FileSizeToKB()})";
	}

	public class Preview {
		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("url")]
		public string URL { get; set; }
	}

	public class Alternates {
	}

	public class Sample {
		[JsonProperty("has")]
		public bool Has { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("url")]
		public string URL { get; set; }

		[JsonProperty("alternates")]
		public Alternates Alternates { get; set; }
	}


	public class Score {
		[JsonProperty("up")]
		public int Up { get; set; }

		[JsonProperty("down")]
		public int Down { get; set; }

		[JsonProperty("total")]
		public int Total { get; set; }
	}

	public class Tags : ICloneable {
		[JsonProperty("general")]
		public List<string> General { get; set; }

		[JsonProperty("species")]
		public List<string> Species { get; set; }

		[JsonProperty("character")]
		public List<string> Character { get; set; }

		[JsonProperty("copyright")]
		public List<string> Copyright { get; set; }

		[JsonProperty("artist")]
		public List<string> Artist { get; set; }

		[JsonProperty("invalid")]
		public List<string> Invalid { get; set; }

		[JsonProperty("lore")]
		public List<string> Lore { get; set; }

		[JsonProperty("meta")]
		public List<string> Meta { get; set; }

		public List<string> GetAllTags() {
			List<string> result = new();
			General.ForEach(s => result.Add(s));
			Species.ForEach(s => result.Add(s));
			Character.ForEach(s => result.Add(s));
			Copyright.ForEach(s => result.Add(s));
			Artist.ForEach(s => result.Add(s));
			Invalid.ForEach(s => result.Add(s));
			Lore.ForEach(s => result.Add(s));
			Meta.ForEach(s => result.Add(s));
			return result;
		}

		public Tags CreateNewBySearch(string searchKey) {
			if (searchKey.IsBlank()) {
				return this;
			}

			Tags clone = new() {
				General = General.Where(x => x.SearchFor(searchKey)).ToList(),
				Species = Species.Where(x => x.SearchFor(searchKey)).ToList(),
				Character = Character.Where(x => x.SearchFor(searchKey)).ToList(),
				Copyright = Copyright.Where(x => x.SearchFor(searchKey)).ToList(),
				Artist = Artist.Where(x => x.SearchFor(searchKey)).ToList(),
				Invalid = Invalid.Where(x => x.SearchFor(searchKey)).ToList(),
				Lore = Lore.Where(x => x.SearchFor(searchKey)).ToList(),
				Meta = Meta.Where(x => x.SearchFor(searchKey)).ToList(),
			};

			return clone;
		}

		public object Clone() {
			return MemberwiseClone();
		}

	}

	public class Flags {
		public bool? pending;
		public bool? flagged;
		public bool? note_locked;
		public bool? status_locked;
		public bool? rating_locked;
		public bool? deleted;
	}

	public class Relationships {
		[JsonProperty("parent_id")]
		public string ParentId { get; set; }

		[JsonProperty("has_children")]
		public bool HasChildren { get; set; }

		[JsonProperty("has_active_children")]
		public bool HasActiveChildren { get; set; }

		[JsonProperty("children")]
		public List<string> Children { get; set; }
	}

	public enum E621Rating {
		[EnumMember(Value = "s")]
		Safe,
		[EnumMember(Value = "q")]
		Questionable,
		[EnumMember(Value = "e")]
		Explicit,
	}


	public enum FileType {
		Png, Jpg, Gif, Webm, Anim
	}
}
