using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace E621Downloader.Models {
	public static class KeyListener {
		private static List<KeyListenerInstance> Instances { get; set; } = new List<KeyListenerInstance>();

		public static void SubmitInstance(KeyListenerInstance instance) {
			Instances.Add(instance);
		}

		public static void RegisterKeyDown(VirtualKey key) {
			foreach(KeyListenerInstance item in GetInstances()) {
				item.KeyDown?.Invoke(key);
			}
		}

		public static void RegisterKeyUp(VirtualKey key) {
			foreach(KeyListenerInstance item in GetInstances()) {
				item.KeyUp?.Invoke(key);
			}
		}

		public static IEnumerable<KeyListenerInstance> GetInstances() => Instances;
	}

	public class KeyListenerInstance {
		public Action<VirtualKey> KeyDown { get; private set; }
		public Action<VirtualKey> KeyUp { get; private set; }

		public KeyListenerInstance(Action<VirtualKey> KeyDown, Action<VirtualKey> KeyUp = null) {
			this.KeyDown = KeyDown;
			this.KeyUp = KeyUp;
		}

	}
}
