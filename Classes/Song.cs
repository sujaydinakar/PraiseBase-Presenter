﻿/*
 *   PraiseBase Presenter 
 *   The open source lyrics and image projection software for churches
 *   
 *   http://code.google.com/p/praisebasepresenter
 *
 *   This program is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU General Public License
 *   as published by the Free Software Foundation; either version 2
 *   of the License, or (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program; if not, write to the Free Software
 *   Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 *   Author:
 *      Nicolas Perrenoud <nicu_at_lavine.ch>
 *   Co-authors:
 *      ...
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using Pbp.Properties;
using System.Security.Cryptography;

namespace Pbp
{
    /// <summary>
    /// Keeps and manages all song related data loaded form an xml file
    /// 
    /// Author: Nicolas Perrenoud <nicolape@ee.ethz.ch>
    /// </summary>
    public partial class Song
	{
		#region Private Variables
		/// <summary>
		/// The whole songtext, which is used for a quick search by the song manager
		/// </summary>
		private string _text;
		/// <summary>
		/// Quality assurance indicator
		/// </summary>
		private int _QA;
		/// <summary>
		/// The file type of this song
		/// </summary>
		private FileFormat _fileType;
		#endregion

		#region Fields
		/// <summary>
		/// Indicates if this is a valid song file. Do not use this class if this
		/// variable is set to false after loading!
		/// </summary>
		public bool IsValid { get; private set; }
		/// <summary>
		/// Path of the song xml file
		/// </summary>
		public string FilePath  { get; private set; }
		/// <summary>
		/// Song number used by the song manager
		/// </summary>
		public int ID { get; set; }
        /// <summary>
        /// The song title. Usually the same as the file name
        /// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Main language of the song
        /// </summary>
		public string Language { get; set; }
         /// <summary>
        /// A list of tags (like categories) which describe the type of the song
        /// </summary>
		public TagList Tags {get;set;}
		/// <summary>
        /// User defined comment for quality assurance information or presentation issues
        /// </summary>
		public string Comment { get; set; }
        /// <summary>
        /// Allows searching in the whole songtext
        /// </summary>
		public string SearchText
		{
			get { return _text; }
			private set
			{
				_text = value;
				_text = _text.Trim().ToLower();
				_text = _text.Replace(",", String.Empty);
				_text = _text.Replace(".", String.Empty);
				_text = _text.Replace(";", String.Empty);
				_text = _text.Replace(Environment.NewLine, String.Empty);
				_text = _text.Replace("  ", " ");
			}
		}
        /// <summary>
        /// Text font
        /// </summary>
        public Font TextFont {get;set;}
        /// <summary>
        /// Text color
        /// </summary>
		public Color TextColor { get; set; }
        /// <summary>
        /// Font of tanslation text
        /// </summary>
		public Font TranslationFont { get; set; }
        /// <summary>
        /// Color of translation text
        /// </summary>
		public Color TranslationColor { get; set; }
        /// <summary>
        /// Additional height between lines
        /// </summary>
		public int TextLineSpacing { get; set; }
        /// <summary>
        /// Additional height between a line and its translation
        /// </summary>
		public int TranslationLineSpacing { get; set; }
        /// <summary>
        /// The list of all parts in the song
        /// </summary>
		public PartList Parts { get; set; }
        /// <summary>
        /// A list containing a sequence of part numbers indicating 
        /// the real sequence the song is song
        /// </summary>
		public List<int> PartSequence { get; set; }
        /// <summary>
        /// List of all slides. Used in the presenter song detail overview.
        /// </summary>
		public SlideList Slides { get; set; }
        /// <summary>
        /// Indicates the current slide
        /// </summary>
		public int CurrentSlide { get; set; }
        /// <summary>
        /// List of the paths to all images
        /// </summary>
		public List<string> RelativeImagePaths { get; set; }
		/// <summary>
		/// Default horizontal text aligning
		/// </summary>
		public SongTextHorizontalAlign DefaultHorizAlign { get; set; }
		/// <summary>
		/// Default vertical text aligning
		/// </summary>
		public SongTextVerticalAlign DefaultVertAlign { get; set; }
		#endregion

		/// <summary>
        /// The song constructor
        /// </summary>
        /// <param name="filePath">Full path to the song xml file</param>
		public Song() : this(null)
		{
		}

		/// <summary>
        /// The song constructor
        /// </summary>
        /// <param name="filePath">Full path to the song xml file</param>
        public Song(string filePath)
        {
            IsValid = false;
			bool err = false;
			
			FilePath = filePath;
			Tags = new TagList();
			DefaultHorizAlign = SongTextHorizontalAlign.Center;
			DefaultVertAlign = SongTextVerticalAlign.Center;
			Slides = new SlideList();
			Parts = new PartList();
			RelativeImagePaths = new List<string>();

			if (filePath != null)
			{
				try
				{
					// Init xml
					XmlDocument xmlDoc;
					XmlElement xmlRoot;
					xmlDoc = new XmlDocument();
					xmlDoc.Load(filePath);
					xmlRoot = xmlDoc.DocumentElement;

					// Detect type
					_fileType = detectFileType(xmlRoot);

					// PraiseBase Presenter Song Format
					if (_fileType == FileFormat.pbps)
					{
						throw new NotImplementedException();
					}
					// PowerPraise Song Format
					else if (_fileType == FileFormat.ppl)
					{
						loadPowerPraiseFile(xmlRoot);
					}
					else
					{
						throw new Exception("Ungültiges Dateiformat (" + xmlRoot.Name + ")!");
					}

				}
				catch (Exception e)
				{
					err = true;
					Console.WriteLine("ERROR loading song " + filePath + " : " + e.Message);
				}
			}
			else
			{

				Title = "Neues Lied";
				Language = "Deutsch";
				Comment = String.Empty;
				_QA = 0;
				Part tmpPart = new Part();
				tmpPart.Slides.Add(new Slide(this));
				Parts.Add(tmpPart);
				_fileType = FileFormat.ppl;

				// Default font settings if values in xml invalid
				TextFont = Settings.Instance.ProjectionMasterFont;
				TextColor = Settings.Instance.ProjectionMasterFontColor;
				TranslationFont = Settings.Instance.ProjectionMasterFontTranslation;
				TranslationColor = Settings.Instance.ProjectionMasterTranslationColor;
				TextLineSpacing = Settings.Instance.ProjectionMasterLineSpacing;
			}

			if (!err)
            {
                IsValid = true;
            }
        }



		private FileFormat detectFileType(XmlElement xmlRoot)
		{
			if (xmlRoot.Name == "ppl" && xmlRoot.GetAttribute("version") == "3.0")
			{
				return FileFormat.ppl;
			}
			else if (xmlRoot.Name == "pbps" && xmlRoot.GetAttribute("version") == "1.0")
			{
				return FileFormat.pbps;
			}
			return FileFormat.invalid;
		}

		private FileFormat detectFileType(string ext)
		{
			if (ext == "ppl")
				return FileFormat.ppl;
			if (ext == "pbps")
				return FileFormat.pbps;
			return FileFormat.invalid;
		}

		public static string getDefaultExtension()
		{
			return "ppl";
		}

		public static string getFileBoxFilter()
		{
			String fltr = String.Empty;
			//fltr += "PraiseBase-Presenter Song (*.pbps)|*.pbps|";
			fltr += "PowerPraise Lied Lied (*.ppl)|*.ppl|";
			fltr += "Alle Dateien (*.*)|*.*";
			return fltr;
		}

		public static string getFileBoxFilterSave()
		{
			String fltr = String.Empty;
			//fltr += "PraiseBase-Presenter Song (*.pbps)|*.pbps|";
			fltr += "PowerPraise Lied Lied (*.ppl)|*.ppl";
			return fltr;
		}

		public ImageList getThumbs()
		{
			ImageList thumbList = new ImageList();
			thumbList.ImageSize = Settings.Instance.ThumbSize;
			thumbList.ColorDepth = ColorDepth.Depth32Bit;

			thumbList.Images.Add(ImageManager.Instance.getEmptyThumb());
			foreach (String relPath in RelativeImagePaths)
			{
				Image img = ImageManager.Instance.getThumbFromRelPath(relPath);
				if (img!=null)
					thumbList.Images.Add(img);
			}
			return thumbList;
		}

        public Image getImage(int nr)
        {
			try
			{
				if (nr < 1)
				{
					return ImageManager.Instance.getEmptyImage();
				}
				if (RelativeImagePaths[nr - 1] == null)
				{
					throw new Exception("Das Bild mit der Nummer " + nr + " existiert nicht!");
				}
				Image img = ImageManager.Instance.getImageFromRelPath(RelativeImagePaths[nr - 1]);
				if (img!=null)
				{
					return img;
				}
				else
				{
					throw new Exception("Das Bild " + RelativeImagePaths[nr - 1] + " existiert nicht!");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return ImageManager.Instance.getEmptyImage();
			}
        }

		/// <summary>
		/// Loads the song contents from a Powerpraise file
		/// </summary>
		/// <param name="xmlRoot">Pointer to the XML-Root Element</param>
		private void loadPowerPraiseFile(XmlNode xmlRoot)
		{
			//
			// General stuff
			//
			Title = xmlRoot["general"]["title"].InnerText;

			if (xmlRoot["general"]["language"] != null)
				Language = xmlRoot["general"]["language"].InnerText;
			else
				Language = "Deutsch";

			if (xmlRoot["general"]["category"] != null)
			{
				Tags.Add(xmlRoot["general"]["category"].InnerText);
			}

			if (xmlRoot["general"]["comment"] != null)
				Comment = xmlRoot["general"]["comment"].InnerText;
			else
				Comment = "";

			if (xmlRoot["general"]["qa"] != null)
				Int32.TryParse(xmlRoot["general"]["qa"].InnerText, out _QA);
			else
				_QA = 0;

			if (xmlRoot["formatting"]["textorientation"] != null)
			{
				if (xmlRoot["formatting"]["textorientation"]["horizontal"] != null)
				{
					switch (xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText)
					{
						case "left":
							DefaultHorizAlign = SongTextHorizontalAlign.Left;
							break;
						case "center":
							DefaultHorizAlign = SongTextHorizontalAlign.Center;
							break;
						case "right":
							DefaultHorizAlign = SongTextHorizontalAlign.Right;
							break;
					}
				}
				if (xmlRoot["formatting"]["textorientation"]["vertical"] != null)
				{
					switch (xmlRoot["formatting"]["textorientation"]["vertical"].InnerText)
					{
						case "top":
							DefaultVertAlign = SongTextVerticalAlign.Top;
							break;
						case "center":
							DefaultVertAlign = SongTextVerticalAlign.Center;
							break;
						case "bottom":
							DefaultVertAlign = SongTextVerticalAlign.Bottom;
							break;
					}
				}
			}

			if (xmlRoot["formatting"]["font"]["maintext"] != null)
			{
				int trySize;
				XmlElement tmpElem = xmlRoot["formatting"]["font"];

				int.TryParse(tmpElem["maintext"]["size"].InnerText, out trySize);
				TextFont = new Font(
					tmpElem["maintext"]["name"].InnerText,
					trySize > 0 ? trySize : Settings.Instance.ProjectionMasterFont.Size,
					(FontStyle)((int)(tmpElem["maintext"]["bold"].InnerText == "true" ? FontStyle.Bold : FontStyle.Regular) + (int)(tmpElem["maintext"]["italic"].InnerText == "true" ? FontStyle.Italic : FontStyle.Regular)));

				int.TryParse(tmpElem["translationtext"]["size"].InnerText, out trySize);
				TranslationFont = new Font(
					tmpElem["translationtext"]["name"].InnerText,
					trySize > 0 ? trySize : Settings.Instance.ProjectionMasterFont.Size,
					(FontStyle)((int)(tmpElem["translationtext"]["bold"].InnerText == "true" ? FontStyle.Bold : FontStyle.Regular) + (int)(tmpElem["translationtext"]["italic"].InnerText == "true" ? FontStyle.Italic : FontStyle.Regular)));

				int.TryParse(tmpElem["maintext"]["color"].InnerText, out trySize);
				TextColor = Color.FromArgb(255, Color.FromArgb(trySize));

				int.TryParse(tmpElem["translationtext"]["color"].InnerText, out trySize);
				TranslationColor = Color.FromArgb(255, Color.FromArgb(trySize));


			}
			if (xmlRoot["formatting"]["linespacing"]["main"] != null)
			{
				int trySize;
				int.TryParse(xmlRoot["formatting"]["linespacing"]["main"].InnerText, out trySize);
				TextLineSpacing = trySize > 0 ? trySize : Settings.Instance.ProjectionMasterLineSpacing;
			}

			//
			// ... and the images
			//

			foreach (XmlElement elem in xmlRoot["formatting"]["background"])
			{
				if (elem.Name == "file")
				{
					if (ImageManager.Instance.imageExists(elem.InnerText))
						RelativeImagePaths.Add(elem.InnerText);
				}
			}

			//
			// Now the song text ... 
			//
			SearchText = xmlRoot["songtext"].InnerText;
			foreach (XmlElement elem in xmlRoot["songtext"])
			{
				if (elem.Name == "part")
				{
					string caption = elem.GetAttribute("caption");
					Part tmpPart = new Part(caption);
					foreach (XmlElement slideElem in elem)
					{
						if (slideElem.Name == "slide")
						{
							Slide tmpSlide = new Slide(this);
							tmpSlide.Lines = new List<string>();
							tmpSlide.HorizontalAlign = DefaultHorizAlign;
							tmpSlide.VerticalAlign = DefaultVertAlign;

							int bgNr = System.Convert.ToInt32(slideElem.GetAttribute("backgroundnr")) + 1;
							bgNr = bgNr < 0 ? 0 : bgNr;
							bgNr = bgNr > RelativeImagePaths.Count ? RelativeImagePaths.Count : bgNr;
							tmpSlide.ImageNumber = bgNr;
							foreach (XmlElement lineElem in slideElem)
							{
								if (lineElem.Name == "line")
								{
									tmpSlide.Lines.Add(lineElem.InnerText);
								}
								if (lineElem.Name == "translation")
								{
									tmpSlide.Translation.Add(lineElem.InnerText);
								}
							}
							Slides.Add(tmpSlide);
							tmpPart.Slides.Add(tmpSlide);
						}
					}
					Parts.Add(tmpPart);
				}
			}
		}

        /// <summary>
        /// Saves the song to an xml file
        /// </summary>
        /// <param name="fileName">The target filename. Use null to save it back to its original file</param>
        public void save(string fileName)
        {
			if (fileName == null)
			{
				fileName = FilePath;
			}
			else
			{
				string ext = Path.GetExtension(fileName);
				_fileType = detectFileType(ext);
			}


			
			XmlDocument xmlDoc = new XmlDocument();


			if (_fileType == FileFormat.ppl)
            {
				XmlNode xmlnode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
				xmlDoc.AppendChild(xmlnode);

                xmlDoc.AppendChild(xmlDoc.CreateElement("ppl"));
                XmlElement xmlRoot = xmlDoc.DocumentElement;
                xmlRoot.SetAttribute("version", "3.0");

                xmlRoot.AppendChild(xmlDoc.CreateElement("general"));
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("title"));
                xmlRoot["general"]["title"].InnerText = Title;
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("category"));
                xmlRoot["general"]["category"].InnerText = Tags.Count > 0 ? Tags[0] : "Keine Kategorie";
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("language"));
				if (Language != string.Empty)
				{
					xmlRoot["general"]["language"].InnerText = Language;
				}
				if (Comment != string.Empty)
				{
					xmlRoot["general"].AppendChild(xmlDoc.CreateElement("comment"));
					xmlRoot["general"]["comment"].InnerText = Comment;
				}

				if (_QA > 0)
				{
					xmlRoot["general"].AppendChild(xmlDoc.CreateElement("qa"));
					xmlRoot["general"]["qa"].InnerText = _QA.ToString();
				}

                xmlRoot.AppendChild(xmlDoc.CreateElement("songtext"));

				List<string> usedImages = new List<string>();
				foreach (Part prt in Parts)
				{
					XmlElement tn = xmlDoc.CreateElement("part");
					tn.SetAttribute("caption", prt.Caption);
					foreach (Slide sld in prt.Slides)
					{
						if (sld.ImageNumber > 0)
						{
							if (!usedImages.Contains(RelativeImagePaths[sld.ImageNumber - 1]))
							{
								usedImages.Add(RelativeImagePaths[sld.ImageNumber - 1]);
							}
							sld.ImageNumber = usedImages.IndexOf(RelativeImagePaths[sld.ImageNumber - 1]) + 1;
						}
					}
				}
				RelativeImagePaths = usedImages;

                foreach (Part prt in Parts)
                {
                    XmlElement tn = xmlDoc.CreateElement("part");
                    tn.SetAttribute("caption", prt.Caption);
                    foreach (Slide sld in prt.Slides)
                    {
                        XmlElement tn2 = xmlDoc.CreateElement("slide");
						tn2.SetAttribute("mainsize", TextFont.Size.ToString());
						tn2.SetAttribute("backgroundnr", (sld.ImageNumber-1).ToString());

                        foreach (string ln in sld.Lines)
                        {
                            XmlElement tn3 = xmlDoc.CreateElement("line");
                            tn3.InnerText = ln;
                            tn2.AppendChild(tn3);
                        }
						foreach (string ln in sld.Translation)
						{
							XmlElement tn3 = xmlDoc.CreateElement("translation");
							tn3.InnerText = ln;
							tn2.AppendChild(tn3);
						}
                        tn.AppendChild(tn2);
                    }
                    xmlRoot["songtext"].AppendChild(tn);
                }

                xmlRoot.AppendChild(xmlDoc.CreateElement("order"));
                foreach (Part prt in Parts)
                {
                    XmlElement tn = xmlDoc.CreateElement("item");
                    tn.InnerText = prt.Caption;
                    xmlRoot["order"].AppendChild(tn);
                }

                xmlRoot.AppendChild(xmlDoc.CreateElement("information"));
                xmlRoot["information"].AppendChild(xmlDoc.CreateElement("copyright"));
                xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("position"));
                xmlRoot["information"]["copyright"]["position"].InnerText = "lastslide";
                xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("text"));
                xmlRoot["information"].AppendChild(xmlDoc.CreateElement("source"));
                xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("position"));
                xmlRoot["information"]["source"]["position"].InnerText = "firstslide";
                xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("text"));

                xmlRoot.AppendChild(xmlDoc.CreateElement("formatting"));

                xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("font"));
				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("maintext"));
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("name"));
				xmlRoot["formatting"]["font"]["maintext"]["name"].InnerText = TextFont.Name.ToString();
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("size"));
				xmlRoot["formatting"]["font"]["maintext"]["size"].InnerText = TextFont.Size.ToString();
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("bold"));
				xmlRoot["formatting"]["font"]["maintext"]["bold"].InnerText = (TextFont.Bold).ToString().ToLower();
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("italic"));
				xmlRoot["formatting"]["font"]["maintext"]["italic"].InnerText = (TextFont.Italic).ToString().ToLower();
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["maintext"]["color"].InnerText = (16777216 + TextColor.ToArgb()).ToString();
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("outline"));
				xmlRoot["formatting"]["font"]["maintext"]["outline"].InnerText = "25";
				xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("shadow"));
				xmlRoot["formatting"]["font"]["maintext"]["shadow"].InnerText = "20";

				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("translationtext"));
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("name"));
				xmlRoot["formatting"]["font"]["translationtext"]["name"].InnerText = TranslationFont.Name.ToString();
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("size"));
				xmlRoot["formatting"]["font"]["translationtext"]["size"].InnerText = TranslationFont.Size.ToString();
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("bold"));
				xmlRoot["formatting"]["font"]["translationtext"]["bold"].InnerText = (TranslationFont.Bold).ToString().ToLower();
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("italic"));
				xmlRoot["formatting"]["font"]["translationtext"]["italic"].InnerText = (TranslationFont.Italic).ToString().ToLower();
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["translationtext"]["color"].InnerText = (16777216 +TranslationColor.ToArgb()).ToString();
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("outline"));
				xmlRoot["formatting"]["font"]["translationtext"]["outline"].InnerText = "25";
				xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("shadow"));
				xmlRoot["formatting"]["font"]["translationtext"]["shadow"].InnerText = "20";

				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("copyrighttext"));
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("name"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["name"].InnerText = TranslationFont.Name.ToString();
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("size"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["size"].InnerText = TranslationFont.Size.ToString();
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("bold"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["bold"].InnerText = (TranslationFont.Bold).ToString().ToLower();
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("italic"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["italic"].InnerText = (TranslationFont.Italic).ToString().ToLower();
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["color"].InnerText = (16777216 + TranslationColor.ToArgb()).ToString();
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("outline"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["outline"].InnerText = "25";
				xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("shadow"));
				xmlRoot["formatting"]["font"]["copyrighttext"]["shadow"].InnerText = "20";

				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("sourcetext"));
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("name"));
				xmlRoot["formatting"]["font"]["sourcetext"]["name"].InnerText = TranslationFont.Name.ToString();
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("size"));
				xmlRoot["formatting"]["font"]["sourcetext"]["size"].InnerText = TranslationFont.Size.ToString();
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("bold"));
				xmlRoot["formatting"]["font"]["sourcetext"]["bold"].InnerText = (TranslationFont.Bold).ToString().ToLower();
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("italic"));
				xmlRoot["formatting"]["font"]["sourcetext"]["italic"].InnerText = (TranslationFont.Italic).ToString().ToLower();
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["sourcetext"]["color"].InnerText = (16777216 + TranslationColor.ToArgb()).ToString();
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("outline"));
				xmlRoot["formatting"]["font"]["sourcetext"]["outline"].InnerText = "25";
				xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("shadow"));
				xmlRoot["formatting"]["font"]["sourcetext"]["shadow"].InnerText = "20";


				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("outline"));
				xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("enabled"));
				xmlRoot["formatting"]["font"]["outline"]["enabled"].InnerText = "true";
				xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["outline"]["color"].InnerText = "0";

				xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("shadow"));
				xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("enabled"));
				xmlRoot["formatting"]["font"]["shadow"]["enabled"].InnerText = "true";
				xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("color"));
				xmlRoot["formatting"]["font"]["shadow"]["color"].InnerText = "0";
				xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("direction"));
				xmlRoot["formatting"]["font"]["shadow"]["direction"].InnerText = "125";

                xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("background"));
				foreach (string imp in usedImages)
                {
						XmlElement tn = xmlDoc.CreateElement("file");
						tn.InnerText = imp;
						xmlRoot["formatting"]["background"].AppendChild(tn);
                }
                xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("linespacing"));
                
                xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("main"));
                xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("translation"));
                xmlRoot["formatting"]["linespacing"]["main"].InnerText = TextLineSpacing.ToString();
                xmlRoot["formatting"]["linespacing"]["translation"].InnerText = TextLineSpacing.ToString();

                xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("textorientation"));
                xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("horizontal"));
				if (Parts[0].Slides[0].HorizontalAlign == SongTextHorizontalAlign.Left)
					xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "left";
				else if (Parts[0].Slides[0].HorizontalAlign == SongTextHorizontalAlign.Right)
					xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "right";
				else
					xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "center";
				xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("vertical"));
				if (Parts[0].Slides[0].VerticalAlign == SongTextVerticalAlign.Top)
					xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "top";
				else if (Parts[0].Slides[0].VerticalAlign == SongTextVerticalAlign.Bottom)
					xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "bottom";
				else
					xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "center";
				xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("transpos"));
				xmlRoot["formatting"]["textorientation"]["transpos"].InnerText = "inline";

                xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("borders"));
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainleft"));
				xmlRoot["formatting"]["borders"]["mainleft"].InnerText = "40";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("maintop"));
				xmlRoot["formatting"]["borders"]["maintop"].InnerText = "70";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainright"));
				xmlRoot["formatting"]["borders"]["mainright"].InnerText = "40";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainbottom"));
				xmlRoot["formatting"]["borders"]["mainbottom"].InnerText = "80";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("copyrightbottom"));
				xmlRoot["formatting"]["borders"]["copyrightbottom"].InnerText = "30";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourcetop"));
				xmlRoot["formatting"]["borders"]["sourcetop"].InnerText = "20";
				xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourceright"));
				xmlRoot["formatting"]["borders"]["sourceright"].InnerText = "40";



				XmlWriter wrt = new XmlTextWriter(fileName, Encoding.UTF8);
				xmlDoc.WriteTo(wrt);
				wrt.Flush();
				wrt.Close();

				FilePath = fileName;
			
			}
        }

		/// <summary>
		/// Sets a specific quality assurance indicator
		/// </summary>
		/// <param name="quai">The indicator to be added</param>
		public void setQA(QualityAssuranceIndicators quai)
		{
			_QA = _QA | (int)quai;
		}

		/// <summary>
		/// Removes a specific quality assurance indicator
		/// </summary>
		/// <param name="quai">The indicator to be removed</param>
		public void remQA(QualityAssuranceIndicators quai)
		{
			_QA = _QA & (~(int)quai);
		}

		/// <summary>
		/// Returns if a specific quality assurance indicator is set
		/// </summary>
		/// <param name="quai">The desired indicator</param>
		public bool getQA(QualityAssuranceIndicators quai)
		{
			return (_QA & (int)quai) > 0 ? true : false; 
		}

		/// <summary>
		/// Returns a hashcode of the song, used for example in the
		/// editor to check if the file was changed
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int code = Title.GetHashCode() 
				^ _QA.GetHashCode()
				^ TextFont.GetHashCode()
				^ TextColor.GetHashCode()
				^ TranslationColor.GetHashCode()
				^ TranslationFont.GetHashCode()
				^ Language.GetHashCode()
				^ Comment.GetHashCode()
				^ TextLineSpacing.GetHashCode() 
				^ Tags.GetHashCode()
				^ Parts.GetHashCode();
			
			return code;
		}

    }
}
