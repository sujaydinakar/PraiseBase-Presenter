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
using Pbp.Properties;
using Pbp.Data.Song;

namespace Pbp.IO
{
    class OpenLyricsSongFileReader : SongFileReader
    {
        public override Song Load(string filename)
        {
            Song sng = new Song();

            // Default font settings if values in xml invalid
            sng.TextFont = Settings.Default.ProjectionMasterFont;
            sng.TextColor = Settings.Default.ProjectionMasterFontColor;
            sng.TranslationFont = Settings.Default.ProjectionMasterFontTranslation;
            sng.TranslationColor = Settings.Default.ProjectionMasterTranslationColor;
            sng.TextLineSpacing = Settings.Default.ProjectionMasterLineSpacing;

            // Init xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);
            XmlElement xmlRoot = xmlDoc.DocumentElement;

            // Check for correct root node and version
            if (xmlRoot.Name != "song" || xmlRoot.GetAttribute("version") != "0.8")
            {
                throw new InvalidSongSourceFileException();
            }

            //
            // File attributes
            //

            // Modified date
            if (xmlRoot.Attributes["modifiedDate"] != null)
            {
                sng.ModifiedTimestamp = xmlRoot.Attributes["modifiedDate"].InnerText;
            }

            // Application the song was created in
            if (xmlRoot.Attributes["createdIn"] != null)
            {
                sng.CreatedIn = xmlRoot.Attributes["createdIn"].InnerText;
            }

            // Application the song was modified in
            if (xmlRoot.Attributes["modifiedIn"] != null)
            {
                sng.ModifiedIn = xmlRoot.Attributes["modifiedIn"].InnerText;
            }
            
            //
            // Properties
            //
            
            // Check if properties sections exists
            if (xmlRoot["properties"] == null 
                || xmlRoot["properties"]["titles"] == null 
                || xmlRoot["properties"]["titles"]["title"] == null)
            {
                throw new IncompleteSongSourceFileException();
            }

            // Title
            sng.Title = xmlRoot["properties"]["titles"]["title"].InnerText;

            // Application the song was modified in
            if (xmlRoot["properties"]["ccliNo"] != null)
            {
                sng.CcliID = xmlRoot["properties"]["ccliNo"].InnerText;
            }

            // Application the song was modified in
            if (xmlRoot["properties"]["copyright"] != null)
            {
                sng.Copyright = xmlRoot["properties"]["copyright"].InnerText;
            }

            // Application the song was modified in
            if (xmlRoot["properties"]["released"] != null)
            {
                sng.ReleaseYear = xmlRoot["properties"]["released"].InnerText;
            }
            
            //
            // Lyrics
            //

            // Check if lyrics sections exists
            if (xmlRoot["lyrics"] == null)
            {
                throw new IncompleteSongSourceFileException();
            }

            foreach (XmlNode verseNode in xmlRoot["lyrics"]) 
            {
                if (verseNode.Name == "verse")
                {
                    var part = new SongPart();
                    if (verseNode.Attributes["name"] != null)
                    {
                        part.Caption = verseNode.Attributes["name"].InnerText;
                    }
                    if (verseNode.Attributes["lang"] != null)
                    {
                        part.Language = verseNode.Attributes["lang"].InnerText;
                    }

                    foreach (XmlNode linesNode in verseNode.ChildNodes)
                    {
                        if (linesNode.Name == "lines")
                        {
                            var slide = new SongSlide(sng);
                            if (linesNode.Attributes["part"] != null)
                            {
                                slide.Part = linesNode.Attributes["part"].InnerText;
                            }
                            string lineText = string.Empty;
                            foreach (XmlNode line in linesNode)
                            {
                                if (line.NodeType == XmlNodeType.Text)
                                {
                                    lineText += line.InnerText;
                                }
                                else if (line.NodeType == XmlNodeType.Element && line.Name=="br")
                                {
                                    slide.Lines.Add(lineText);
                                    lineText = string.Empty;
                                }
                            }
                            slide.Lines.Add(lineText);
                            part.Slides.Add(slide);
                        }
                    }
                    if (part.Slides.Count > 0)
                    {
                        sng.Parts.Add(part);
                    }
                }
            }

            // Check if at leas one part exists
            if (sng.Parts.Count == 0)
            {
                throw new IncompleteSongSourceFileException();
            }

            sng.UpdateSearchText();
            return sng;
        }
    }
}