using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Networks {
	public class DataResult<T> {
		public HttpResultType ResultType { get; set; }

		public T Data { get; set; }

		public DataResult(HttpResultType resultType, T data) {
			ResultType = resultType;
			Data = data;

		}
	}
}
