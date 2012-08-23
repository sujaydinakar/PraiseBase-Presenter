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
using System.Drawing;
using System.Linq;

namespace Pbp
{
    /// <summary>
    /// A single slide with songtext and/or a background image
    /// </summary>
    public class SongSlide : ICloneable
    {
        /// <summary>
        /// Pointer to the song object who owns this slide
        /// </summary>
        private readonly Song _ownerSong;

        /// <summary>
        /// The slide constructor
        /// </summary>
        public SongSlide(Song ownerSong)
        {
            Lines = new List<string>();
            Translation = new List<string>();
            HorizontalAlign = Song.SongTextHorizontalAlign.Center;
            VerticalAlign = Song.SongTextVerticalAlign.Center;
            SongTextAlign = Song.TextAlign.MittleLeft;
            ImageNumber = 0;
            _ownerSong = ownerSong;
        }

        /// <summary>
        /// All text lines of this slide
        /// </summary>
        public List<string> Lines { get; set; }

        /// <summary>
        /// All translation lines of this slide
        /// </summary>
        public List<string> Translation { get; set; }

        /// <summary>
        /// Number of the slide image. If set to -1, no image is used
        /// </summary>
        public int ImageNumber { get; set; }

        /// <summary>
        /// Indicates wether this slide has a translation
        /// </summary>
        public bool Translated
        {
            get { return Translation.Count > 0 ? true : false; }
        }

        /// <summary>
        /// Horizonztal text alignment
        /// </summary>
        public Song.SongTextHorizontalAlign HorizontalAlign { get; set; }

        /// <summary>
        /// Vertical text alignment
        /// </summary>
        public Song.SongTextVerticalAlign VerticalAlign { get; set; }

        public Song.TextAlign SongTextAlign { get; set; }

        /// <summary>
        /// The font object of this slide
        /// </summary>
        public Font TextFont
        {
            get { return _ownerSong.TextFont; }
        }

        /// <summary>
        /// The font object of the translation
        /// </summary>
        public Font TranslationFont
        {
            get { return _ownerSong.TranslationFont; }
        }

        /// <summary>
        /// The font color of this slide
        /// </summary>
        public Color TextColor
        {
            get { return _ownerSong.TextColor; }
        }

        /// <summary>
        /// The translation font color
        /// </summary>
        public Color TranslationColor
        {
            get { return _ownerSong.TranslationColor; }
        }

        /// <summary>
        /// The line spacing of the text
        /// </summary>
        public int TextLineSpacing
        {
            get { return _ownerSong.TextLineSpacing; }
        }

        #region ICloneable Members

        /// <summary>
        /// Clones this slide
        /// </summary>
        /// <returns>A duplicate of this slide</returns>
        public object Clone()
        {
            var res = new SongSlide(_ownerSong) { HorizontalAlign = HorizontalAlign, ImageNumber = ImageNumber };
            foreach (string obj in Lines)
                res.Lines.Add(obj);
            foreach (string obj in Translation)
                res.Translation.Add(obj);
            res.VerticalAlign = VerticalAlign;
            return res;
        }

        #endregion

        /// <summary>
        /// Sets the text of this slide
        /// </summary>
        /// <param name="text"></param>
        public void SetSlideText(string text)
        {
            Lines = new List<string>();
            string[] ln = text.Trim().Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (string sl in ln)
            {
                Lines.Add(sl.Trim());
            }
        }

        /// <summary>
        /// Sets the translation of this slide
        /// </summary>
        /// <param name="text"></param>
        public void SetSlideTextTranslation(string text)
        {
            Translation = new List<string>();
            string[] tr = text.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (string sl in tr)
            {
                Translation.Add(sl.Trim());
            }
        }

        /// <summary>
        /// Returns a string of the wrapped text
        /// </summary>
        /// <returns>Wrapped text</returns>
        public string LineBreakText()
        {
            string txt = "";
            int i = 1;
            foreach (string str in Lines)
            {
                txt += str;
                if (i < Lines.Count)
                    txt += Environment.NewLine;
                i++;
            }
            return txt;
        }

        /// <summary>
        /// Returns the wrapped translation text
        /// </summary>
        /// <returns>Wrapped translation</returns>
        public string LineBreakTranslation()
        {
            string txt = "";
            int i = 1;
            foreach (string str in Translation)
            {
                txt += str;
                if (i < Translation.Count)
                    txt += Environment.NewLine;
                i++;
            }
            return txt;
        }

        /// <summary>
        /// Returns the text on one line. This is mainly used 
        /// in the song detail overview in the presenter.
        /// </summary>
        /// <returns>Text on one line</returns>
        public string OneLineText()
        {
            return Lines.Aggregate("", (current, str) => current + (str + " "));
        }

        public string OneLineTranslation()
        {
            return Translation.Aggregate("", (current, str) => current + (str + " "));
        }

        public override int GetHashCode()
        {
            int res = ImageNumber.GetHashCode() ^ HorizontalAlign.GetHashCode() ^ VerticalAlign.GetHashCode();
            for (int i = 0; i < Lines.Count; i++)
            {
                res = res ^ Lines[i].GetHashCode();
            }
            for (int i = 0; i < Translation.Count; i++)
            {
                res = res ^ Translation[i].GetHashCode();
            }
            return res;
        }
    } ;
}