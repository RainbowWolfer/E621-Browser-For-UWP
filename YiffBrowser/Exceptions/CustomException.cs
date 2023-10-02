using System;

namespace YiffBrowser.Exceptions {
	internal class CustomException(string message = null) : Exception(message) { }
}
