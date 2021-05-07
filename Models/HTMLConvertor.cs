using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E621Downloader.Models {
	public class HTMLConvertor {
		public readonly string source;

		public HTMLConvertor(string source) {
			this.source = source;
		}

		public void FindNextNode(string content) {
			int header_start = content.IndexOf('<');
			string header = "";
			for(int i = 0; i < int.MaxValue; i++) {
				if(content[i] == '>') {
					break;
				} else if(content[i] == ' ') {
					header = content.Substring(header_start + 1, i - 1);
					break;
				}
			}
			int header_end = content.IndexOf('>');
			string fullProperty = content.Substring(header_start + header.Length + 1, header_end - header_start - header.Length - 2);
			Debug.WriteLine(header);
			string[] sets = fullProperty.Split(' ');

			if(content[header_end - 1] == '/') {
				Debug.WriteLine("Single Line End");
				return;
			}

			int sameCount = 0;
			for(int i = header_end + 1; i < content.Length; i++) {
				string surroundString_test = GetSurroundString(content, i);
				char c = content[i];
				if(c == '>') {
					string ender_test = content.Substring(i - 3 - header.Length, 3 + header.Length);
					Debug.WriteLine("ENDER test : " + ender_test);
					if(ender_test == "</" + header + '>') {
						Debug.WriteLine("Endertest success!");
					}

				}
				if(c == '<') {
					string header_test = content.Substring(i + 1, header.Length);
					Debug.WriteLine("Test Header : " + header_test);
					if(header_test == header) {
						sameCount++;
					}
					i += header.Length + 1;
					continue;
				}
			}




		}
		private string GetSurroundString(string content, int centerIndex, int depth = 3) {
			string result = "";
			for(int i = -depth; i <= depth; i++) {
				int index = centerIndex + i;
				if(index >= 0 && index < content.Length) {
					result += content[index];
				}
			}
			return result;
		}


		public class Node {

		}

		public class Property {

		}

		//source set?
	}
}
