using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace E621Downloader.Models {
	public static class Data {
		public const string USERAGENT = "RainbowWolferE621TestApp";

		public static E621Article[] GetPostsByTags(int page, params string[] tags) {
			if(page <= 0) {
				throw new Exception("Page not valid");
			}
			string url = string.Format("https://e621.net/posts?page={0}&tags=", page);
			tags.ToList().ForEach((t) => url += t + "+");

			var articles = new List<E621Article>();

			string result = ReadURL(url);
			Debug.WriteLine(result);
			if(result == null) {
				return articles.ToArray();
			}
			try {
				int start = result.IndexOf("<article") + 8;
				for(int i = start; i < result.Length; i++) {
					string endTest = result.Substring(i, 10);
					if(endTest == "</article>") {
						string allInfo = result.Substring(start, i - start);
						articles.Add(new E621Article(allInfo));
						if(result.IndexOf("<article", i) != -1) {
							start = i + 10;
							i = start;
						} else {
							break;
						}
					}
				}
			} catch(Exception e) {
				return articles.ToArray();
			}
			return articles.ToArray();
		}
		public static string ReadURL(string url) {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = USERAGENT;
			HttpWebResponse response;
			try {
				response = (HttpWebResponse)request.GetResponse();
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
				return null;
			}
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			string data = reader.ReadToEnd();
			return data;
		}
		[Obsolete("", true)]
		public static Paragraph[] GetCompleteParagraphs(string source) {
			Paragraph[] paragraphs = GetParallelParagraphs(source);
			CalculateChilrenParagrah(paragraphs);
			return paragraphs;
		}
		[Obsolete("", true)]
		public static Paragraph[] GetParallelParagraphs(string source) {
			List<Paragraph> paragraphs = new List<Paragraph>();
			for(int i = 0; i < source.Length; i++) {
				bool insideOpen = false;//<p></p>
				bool insideClose = false;//<p/>
				int index = i + 1;
				if(source[i] == '<') {
					while(index < source.Length - 1) {
						index++;
						if(source[index] == '>') {
							insideOpen = true;
							break;
						} else if(source[index] == '/' && source[index + 1] == '>') {
							insideClose = true;
							break;
						}
					}
					string startTag = GetSubString(source, i, index);//<tag>
					int spaceIndex = startTag.ToList().FindIndex((cha) => cha == ' ');
					string startIdentifier;//tag
					if(spaceIndex == -1) {
						startIdentifier = GetSubString(startTag, 1, startTag.Length - 2).Trim();
					} else {
						startIdentifier = GetSubString(startTag, 1, spaceIndex).Trim();
					}
					string content;
					if(insideOpen) {
						//1.没有检测到<span>
						//2.检测到<span>时，每次检测到，计数i+1，检测到</span>时，计数j+1，之后往后检测，
						//如果是<span>，则重复，每次当i==j时才跳出到3
						//3.如果是</span>则是返回这个正确的结尾
						int currentIndex = index;
						int a = 0;
						int b = 0;
						bool lastEnd = false;
						while(currentIndex++ < source.Length - startIdentifier.Length) {
							char c = source[currentIndex];
							if(c == '<') {
								if(source[currentIndex + 1] == '/') {//</span>
									string cIdentifier = GetSubString(source, currentIndex + 2, currentIndex + startIdentifier.Length + 1);
									if(cIdentifier == startIdentifier) {
										if(a == b) {
											lastEnd = true;
										}
										if(lastEnd) {
											break;
										}
										b++;
									}
								} else {//<span>
									string cIdentifier = GetSubString(source, currentIndex + 1, currentIndex + startIdentifier.Length);
									if(cIdentifier == startIdentifier) {
										a++;
										lastEnd = false;
									}
								}
							}
						}
						if(index + 1 > currentIndex - 1) {
							content = "";
						} else {
							content = GetSubString(source, index + 1, currentIndex - 1);
						}

						paragraphs.Add(new Paragraph(startIdentifier, GetProperties(startTag), content) { isClosed = false });
						i = currentIndex;
						continue;
					} else if(insideClose) {
						int currentIndex = index;
						while(currentIndex++ < source.Length - 1) {
							char current = source[currentIndex];
							if(current == '>') {
								break;
							}
						}
						if(currentIndex - 2 < index) {
							content = "";
						} else {
							content = GetSubString(source, index, currentIndex - 2);
						}
						paragraphs.Add(new Paragraph(startIdentifier, GetProperties(startTag), content) { isClosed = true });
						i = currentIndex;
						continue;
					}
				}
			}
			return paragraphs.ToArray();
		}
		[Obsolete("", true)]
		public static void CalculateChilrenParagrah(params Paragraph[] paragraphs) {
			foreach(Paragraph p in paragraphs) {
				Paragraph[] pc = GetParallelParagraphs(p.fullContent);
				if(pc.Length != 0) {
					p.children = pc;
					foreach(Paragraph pp in p.children) {
						pp.parent = p;
					}
					CalculateChilrenParagrah(p.children);
				} else {
					p.children = new Paragraph[0];
				}
			}
		}
		[Obsolete("", true)]
		public static Property[] GetProperties(string source) {
			string s1 = new string(new char[] { (char)34, (char)34 });
			string s2 = new string(new char[] { (char)34, (char)32, (char)34 });
			Console.WriteLine(s1);
			Console.WriteLine(s2);
			source = Regex.Replace(source, s1, s2);
			List<Property> properties = new List<Property>();
			int startIndex = 0;
			bool found = false;
			for(int i = 0; i < source.Length; i++) {
				if(source[i] == ' ') {
					startIndex = i + 1;
					found = true;
					break;
				}
			}
			if(!found) {
				return properties.ToArray();
			}

			for(int i = startIndex; i < source.Length; i++) {
				int nameStart = i;
				int index = nameStart;
				while(index++ < source.Length - 1) {
					if(source[index] == '=' && source[index + 1] == '"') {
						string name = GetSubString(source, nameStart, index - 1);
						string content = "";
						int contentStart = index + 2;
						index += 2;
						char c = source[index];
						while(index++ < source.Length - 1) {
							if(source[index] == '"') {
								content = GetSubString(source, contentStart, index - 1);
								properties.Add(new Property(name.Trim(), content));
								i = index;
								goto exitInnerLoop;
							}
						}
					}
				}
				exitInnerLoop:
				;
			}
			return properties.ToArray();
		}
		[Obsolete("", true)]
		public static string GetSubString(string source, int start, int end) {
			if(start > end || end > source.Length) {
				throw new Exception();
			}
			string result = "";
			for(int i = start; i <= end; i++) {
				result += source[i];
			}
			return result;
		}

		[Obsolete("", true)]
		public class Paragraph {
			public bool isClosed;
			public string tag;
			public Property[] properties;
			public string fullContent;
			public string fixedContent {
				get {
					if(GetParallelParagraphs(this.fullContent).Length == 0 && children != null) {
						return fullContent;
					} else {
						string c = fullContent;
						foreach(Paragraph p in children) {
							c = Regex.Replace(c, p.fullContent, "");
						}
						c = Regex.Replace(c, "<(.|\n)*?>", "");
						return c;
					}
				}
			}

			public Paragraph[] children { get; set; }
			public Paragraph parent { get; set; }

			public Paragraph(string tag, Property[] properties, string content) {
				this.tag = tag;
				this.properties = properties;
				this.fullContent = content;
			}

			public override string ToString() {
				return tag + "_" + properties.Length;
			}
		}
		[Obsolete("", true)]
		public class Property {
			public string name;
			public string content;

			public Property(string name, string content) {
				this.name = name;
				this.content = content;
			}
			public override string ToString() {
				return name + " _ " + content;
			}
		}
		[Obsolete("", true)]
		public class SourceSet {
			public class Set {
				public string address;
				public int quality;
				public Set(string address, int quality) {
					this.address = address;
					this.quality = quality;
				}
			}
			public string srcset { get; private set; }
			public Set[] sets { get; private set; }

			public SourceSet(string srcset) {
				this.srcset = srcset;

				string[] split = Regex.Split(srcset, ",");
				List<Set> ss = new List<Set>();
				foreach(string s in split) {
					int wIndex = s.LastIndexOf('w');
					int spaceIndex = s.LastIndexOf(' ');
					if(int.TryParse(GetSubString(s, spaceIndex, wIndex - 1), out int result)) {
						string address = GetSubString(s, 0, spaceIndex - 1);
						ss.Add(new Set(address, result));
					}
				}
				this.sets = ss.ToArray();
				for(int a = 0; a < sets.Length - 1; a++) {
					for(int b = 0; b < sets.Length - 1 - a; b++) {
						if(sets[b].quality > sets[b + 1].quality) {
							var temp = sets[b + 1];
							sets[b + 1] = sets[b];
							sets[b] = temp;
						}
					}
				}
			}
			public override string ToString() {
				return srcset;
			}
		}
	}

}
