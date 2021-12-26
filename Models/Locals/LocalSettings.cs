using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Locals {
	public class LocalSettings {
		public static LocalSettings Current { get; set; }
		public static void Save() {
			Local.WriteLocalSettings();
		}

		public bool safeMode;
		public bool showNullImages;
		public bool showBlackListed;
		public bool cycleList;
		public bool spot_allowWebm;
		public bool spot_allowGif;
		public bool spot_allowBlackList;
		public bool spot_includeSafe;
		public bool spot_includeQuestoinable;
		public bool spot_includeExplicit;
		public int spot_amount;
	}
}
