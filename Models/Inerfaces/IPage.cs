﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Inerfaces {
	public interface IPage {
		void UpdateNavigationItem();
		void FocusMode(bool enabled);//need to be checked in every OnNavigatedTo();
	}
}