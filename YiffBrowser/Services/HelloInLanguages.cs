﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace YiffBrowser.Services {
	public static class HelloInLanguages {
		public static KeyValuePair<string, string> GetRandomWelcomePair() {
			return welcomes.ElementAt(new Random().Next(welcomes.Count));
		}

		public static readonly Dictionary<string, string> welcomes = new() {
			{ "Afrikaans", "hallo" },
			{ "Albanian", "Përshëndetje" },
			{ "Amharic", "ሰላም" },
			{ "Arabic", "مرحبا" },
			{ "Armenian", "Բարեւ" },
			{ "Azerbaijani", "Salam" },
			{ "Basque", "Kaixo" },
			{ "Belarusian", "добры дзень" },
			{ "Bengali", "হ্যালো" },
			{ "Bosnian", "zdravo" },
			{ "Bulgarian", "Здравейте" },
			{ "Catalan", "Hola" },
			{ "Cebuano", "Hello" },
			{ "Chichewa", "Moni" },
			{ "Chinese (Simplified)", "您好" },
			{ "Chinese (Traditional)", "您好" },
			{ "Corsican", "Bonghjornu" },
			{ "Croatian", "zdravo" },
			{ "Czech", "Ahoj" },
			{ "Danish", "Hej" },
			{ "Dutch", "Hallo" },
			{ "English", "Hello" },
			{ "Esperanto", "Saluton" },
			{ "Estonian", "Tere" },
			{ "Filipino", "Hello" },
			{ "Finnish", "Hei" },
			{ "French", "Bonjour" },
			{ "Frisian", "Hello" },
			{ "Galician", "Ola" },
			{ "Georgian", "გამარჯობა" },
			{ "German", "Hallo" },
			{ "Greek", "Γεια σας" },
			{ "Gujarati", "હેલો" },
			{ "Haitian Creole", "Bonjou" },
			{ "Hausa", "Sannu" },
			{ "Hawaiian", "Alohaʻoe" },
			{ "Hebrew", "שלום" },
			{ "Hindi", "नमस्ते" },
			{ "Hmong", "Nyob zoo" },
			{ "Hungarian", "Helló" },
			{ "Icelandic", "Halló" },
			{ "Igbo", "Ndewo" },
			{ "Indonesian", "Halo" },
			{ "Irish", "Dia duit" },
			{ "Italian", "Ciao" },
			{ "Japanese", "こんにちは" },
			{ "Javanese", "Hello" },
			{ "Kannada", "ಹಲೋ" },
			{ "Kazakh", "Сәлем" },
			{ "Khmer", "ជំរាបសួរ" },
			{ "Korean", "안녕하세요." },
			{ "Kurdish (Kurmanji)", "Hello" },
			{ "Kyrgyz", "салам" },
			{ "Lao", "ສະບາຍດີ" },
			{ "Latin", "salve" },
			{ "Latvian", "Labdien!" },
			{ "Lithuanian", "Sveiki" },
			{ "Luxembourgish", "Moien" },
			{ "Macedonian", "Здраво" },
			{ "Malagasy", "Hello" },
			{ "Malay", "Hello" },
			{ "Malayalam", "ഹലോ" },
			{ "Maltese", "Hello" },
			{ "Maori", "Hiha" },
			{ "Marathi", "हॅलो" },
			{ "Mongolian", "Сайн байна уу" },
			{ "Myanmar (Burmese)", "မင်္ဂလာပါ" },
			{ "Nepali", "नमस्ते" },
			{ "Norwegian", "Hallo" },
			{ "Pashto", "سلام" },
			{ "Persian", "سلام" },
			{ "Polish", "Cześć" },
			{ "Portuguese", "Olá" },
			{ "Punjabi", "ਹੈਲੋ" },
			{ "Romanian", "Alo" },
			{ "Russian", "привет" },
			{ "Samoan", "Talofa" },
			{ "Scots Gaelic", "Hello" },
			{ "Serbian", "Здраво" },
			{ "Sesotho", "Hello" },
			{ "Shona", "Hello" },
			{ "Sindhi", "هيلو" },
			{ "Sinhala", "හෙලෝ" },
			{ "Slovak", "ahoj" },
			{ "Slovenian", "Pozdravljeni" },
			{ "Somali", "Hello" },
			{ "Spanish", "Hola" },
			{ "Sundanese", "halo" },
			{ "Swahili", "Sawa" },
			{ "Swedish", "Hallå" },
			{ "Tajik", "Салом" },
			{ "Tamil", "ஹலோ" },
			{ "Telugu", "హలో" },
			{ "Thai", "สวัสดี" },
			{ "Turkish", "Merhaba" },
			{ "Ukranian", "Здрастуйте" },
			{ "Urdu", "ہیلو" },
			{ "Uzbek", "Salom" },
			{ "Vietnamese", "Xin chào" },
			{ "Welsh", "Helo" },
			{ "Xhosa", "Sawubona" },
			{ "Yiddish", "העלא" },
			{ "Yoruba", "Kaabo" },
			{ "Zulu", "Sawubona" },
		};
	}
}
