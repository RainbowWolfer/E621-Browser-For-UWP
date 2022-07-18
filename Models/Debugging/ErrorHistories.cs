using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models.Debugging {
	public static class ErrorHistories {
		public static List<HandledException> HandledExceptions { get; } = new();
		public static void Add(Exception ex) {
			HandledExceptions.Insert(0, new HandledException(ex.GetType().ToString(), ex.Message, ex.StackTrace));
		}

		public class HandledException {
			public string ExceptionType { get; set; }
			public string Message { get; set; }
			public string StackTrace { get; set; }

			public HandledException(string exceptionType, string message, string stackTrace) {
				ExceptionType = exceptionType;
				Message = message;
				StackTrace = stackTrace;
			}
		}
	}
}
