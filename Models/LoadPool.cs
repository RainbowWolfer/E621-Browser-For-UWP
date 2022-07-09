using E621Downloader.Models.Posts;
using E621Downloader.Pages;
using System;
using System.Collections.Generic;
using Windows.Media.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace E621Downloader.Models {
	public static class LoadPool {
		public static Dictionary<string, LoadPoolItem> Pool { get; private set; } = new();

		public static LoadPoolItem SetNew(Post post) {
			if(Pool.TryGetValue(post.id, out LoadPoolItem value)) {
				return value;
			} else {
				LoadPoolItem item = new(post);
				Pool.TryAdd(post.id, item);
				return item;
			}
		}


		public static LoadPoolItem GetLoader(string postID) {
			if(Pool.TryGetValue(postID, out LoadPoolItem value)) {
				return value;
			} else {
				return null;
			}
		}


		//public static void SetItemPreview(Post post, BitmapImage preview) {
		//	if(Pool.TryGetValue(post.id, out LoadPoolItem value)) {
		//		value.Preview = preview;
		//	} else {
		//		LoadPoolItem item = new(post);
		//		if(Pool.TryAdd(post.id, item)) {
		//			item.Preview = preview;
		//		}
		//	}
		//}

		//public static void SetItemSample(Post post, BitmapImage sample) {
		//	if(Pool.TryGetValue(post.id, out LoadPoolItem value)) {
		//		value.Sample = sample;
		//	} else {
		//		LoadPoolItem item = new(post);
		//		if(Pool.TryAdd(post.id, item)) {
		//			item.Sample = sample;
		//		}
		//	}
		//}

		//public static void SetItemFile(Post post, BitmapImage image) {
		//	FileType type = PicturePage.GetFileType(post);
		//	if(type == FileType.Webm || type == FileType.Anim) {
		//		return;
		//	}
		//	if(Pool.TryGetValue(post.id, out LoadPoolItem value)) {
		//		value.ImageFile = image;
		//	} else {
		//		LoadPoolItem item = new(post);
		//		if(Pool.TryAdd(post.id, item)) {
		//			item.ImageFile = image;
		//		}
		//	}
		//}

		//public static void SetItemFile(Post post, MediaSource video) {
		//	FileType type = PicturePage.GetFileType(post);
		//	if(type != FileType.Webm) {
		//		return;
		//	}
		//	if(Pool.TryGetValue(post.id, out LoadPoolItem value)) {
		//		value.VideoFile = video;
		//	} else {
		//		LoadPoolItem item = new(post);
		//		if(Pool.TryAdd(post.id, item)) {
		//			item.VideoFile = video;
		//		}
		//	}
		//}
	}

	public class LoadPoolItem {
		private BitmapImage preview = null;
		private BitmapImage sample = null;
		private BitmapImage imageFile = null;
		private MediaSource videoFile = null;

		public event Action<BitmapImage> OnPreviewLoad;
		public event Action<BitmapImage> OnSampleLoad;
		public event Action<BitmapImage> OnImageLoad;
		public event Action<MediaSource> OnVideoLoad;

		public string PostID { get; private set; }
		public bool IsVideo { get; private set; } = false;
		public BitmapImage Preview {
			get => preview;
			set {
				preview = value;
				OnPreviewLoad?.Invoke(value);
			}
		}
		public BitmapImage Sample {
			get => sample;
			set {
				sample = value;
				OnSampleLoad?.Invoke(value);
			}
		}
		public BitmapImage ImageFile {
			get => imageFile;
			set {
				imageFile = value;
				OnImageLoad?.Invoke(value);
			}
		}
		public MediaSource VideoFile {
			get => videoFile;
			set {
				videoFile = value;
				OnVideoLoad?.Invoke(value);
			}
		}

		public LoadPoolItem(Post post) {
			PostID = post.id;
			FileType type = PicturePage.GetFileType(post);
			IsVideo = type == FileType.Webm;
		}

		public LoadPoolItem(string postID, bool isVideo) {
			PostID = postID;
			IsVideo = isVideo;
		}

		public BitmapImage GetBestPreviewImage() {
			if(Sample != null) {
				return Sample;
			} else if(Preview != null) {
				return Preview;
			} else {
				return null;
			}
		}
	}

	public class LoadPoolItemActions {
		public Action OnUrlsEmpty { get; set; } = null;
		public Action OnSampleUrlEmpty { get; set; } = null;

		public Action OnPreviewStart { get; set; } = null;
		public Action OnPreviewFailed { get; set; } = null;
		public Action<BitmapImage> OnPreviewOpened { get; set; } = null;
		public Action<int> OnPreviewProgress { get; set; } = null;
		public Action OnPreviewExists { get; set; } = null;

		public Action<bool> OnSampleStart { get; set; } = null;
		public Action OnSampleFailed { get; set; } = null;
		public Action<BitmapImage> OnSampleOpened { get; set; } = null;
		public Action<int?> OnSampleProgress { get; set; } = null;
		public Action OnSampleExists { get; set; } = null;
	}
}
