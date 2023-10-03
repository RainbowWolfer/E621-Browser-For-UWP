using Prism.Mvvm;
using YiffBrowser.Helpers;
using YiffBrowser.Models.E621;

namespace YiffBrowser.Services.Downloads {
	public class DownloadPreparation(E621Post post, string folderName) : BindableBase {
		private bool doingTask = false;
		private bool hasRequestedCancel = false;

		public bool DoingTask {
			get => doingTask;
			set => SetProperty(ref doingTask, value);
		}

		public E621Post Post { get; } = post;
		public string FolderName { get; } = folderName;

		public FileType FileType => Post.GetFileType();
		public string FileTypeString => FileType.ToString();


		public bool HasRequestedCancel {
			get => hasRequestedCancel;
			private set => SetProperty(ref hasRequestedCancel, value);
		}

		public void Cancel() {
			HasRequestedCancel = true;
		}

	}
}
