using System;
using System.Reflection;

namespace YiffBrowser.Helpers {
	public static class AttributeHelper {
		public static bool TryGetAttribute<T>(this MemberInfo[] membersInfo, out T attribute) where T : Attribute {
			if (membersInfo.IsNotEmpty()) {
				foreach (MemberInfo member in membersInfo) {
					if (member.TryGetAttribute(out T a)) {
						attribute = a;
						return true;
					}
				}
			}
			attribute = null;
			return false;
		}

		public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T attribute) where T : Attribute {
			if (memberInfo is not null) {
				object[] attributes = memberInfo.GetCustomAttributes(typeof(T), false);
				if (attributes.IsNotEmpty()) {
					foreach (object a in attributes) {
						if (a is T t) {
							attribute = t;
							return true;
						}
					}
				}
			}
			attribute = null;
			return false;
		}
	}
}
