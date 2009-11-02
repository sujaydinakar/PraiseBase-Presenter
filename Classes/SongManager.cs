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
using Pbp.Properties;
using System.IO;
using System.Windows.Forms;

using Pbp.Forms;

namespace Pbp
{
	/// <summary>
	/// Holds a list of all songs and provides
	/// searching in the songlist
	/// </summary>
    class SongManager
    {
		/// <summary>
		/// Singleton variable
		/// </summary>
        static private SongManager _instance;

        /// <summary>
        /// List of all availabe songs
        /// </summary>
        public Dictionary<System.Guid, Song> SongList {get;private set;}


		/// <summary>
		/// Gets or sets the current song object
		/// </summary>
        public Song CurrentSong {get;set;}

		/// <summary>
		/// Gets the singleton of this class
		/// </summary>
		static public SongManager Instance
		{
			get
			{
				return getInstance();
			}
		}


		/// <summary>
		/// The constructor
		/// </summary>
		private SongManager(params object[] param)
        {
			reload(param);
        }

		/// <summary>
		/// Gets the singleton of this class
		/// </summary>
		/// <returns>Returns an unique instance of the song manager</returns>
		static public SongManager getInstance(params object[] param)
		{
			if (_instance == null)
			{
				_instance = new SongManager(param);
			}
			return _instance;
		}

		/// <summary>
		/// Reloads all songs from the song direcory
		/// specified in the application settings
		/// </summary>
        public void reload(params object[] param)
        {
			LoadingScreen ldg = null;
			if (param.Count() == 1 && param[0].GetType() == typeof(LoadingScreen))
			{
				ldg = (LoadingScreen)param[0];
			}				

            if (Settings.Instance.DataDirectory == "")
            {
				// Todo: check and create
                Settings.Instance.DataDirectory =  Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString() + Path.DirectorySeparatorChar + Settings.Instance.DataDirDefaultName;
                Settings.Instance.Save();
            }

            string searchDir = Settings.Instance.DataDirectory +  Path.DirectorySeparatorChar + Settings.Instance.SongDir;

			List<string> songPaths = new List<string>();
            if (Directory.Exists(searchDir))
            {
                foreach (string ext in Enum.GetNames(typeof(Song.FileFormat)))
                {
                    string[] songFilePaths = Directory.GetFiles(searchDir, "*."+ext, SearchOption.AllDirectories);
                    foreach (string file in songFilePaths)
                    {
						songPaths.Add(file);
                    }
                }
            }



			int cnt = songPaths.Count;
			if (ldg != null)
				ldg.setProgBarMax(cnt);

            SongList = new Dictionary<System.Guid, Song>();

			for (int i = 0; i <cnt;i++ )
			{
				if (ldg != null && i%10 == 0)
				{
					ldg.setProgBarValue(i);
					ldg.setLabel("Lade Lieder " + i.ToString() + "/" + cnt.ToString());
				}
				Song tmpSong = new Song(songPaths[i]);
				if (tmpSong.IsValid)
				{
                    SongList[tmpSong.GUID] = tmpSong;
				}
			}

            


        }

		/// <summary>
		/// Returns the list id of the song with specified title
		/// </summary>
		/// <param name="title">The title of the song</param>
		/// <returns>Returns the position in the songlist</returns>
		public Guid getGUID(string title)
		{
			foreach (KeyValuePair<Guid,Song> kvp in SongList)
			{
				if (kvp.Value.Title == title)
				{
					return kvp.Key;
				}
			}
            MessageBox.Show("Titel " + title + " nicht vorhanden");
            return Guid.Empty;
		}

		/// <summary>
		/// Reloads the song at the specified position
		/// </summary>
		/// <param name="i">The song position</param>
		public void reloadSong(Guid g)
		{
			SongList[g] = new Song(SongList[g].FilePath,g);
		}

		/// <summary>
		/// Reloads the song with the specified path
		/// </summary>
		/// <param name="path">Path to the song file</param>
		public void reloadSongByPath(string path)
		{
			if (path != String.Empty && path != null)
			{
                foreach (KeyValuePair<Guid, Song> kvp in SongList)
                {
                    if (kvp.Value.FilePath == path)
                    {
                        SongList[kvp.Key] = new Song(path,kvp.Key);
                        return;
                    }
                }
			}
		}




		/// <summary>
		/// Search the songlist for a given pattern and returns the matching songs
		/// </summary>
		/// <param name="needle">The search pattern</param>
		/// <param name="mode">If set to 1, the sogtext is also searched for the pattern. If set to 0, only the song title will be used</param>
		/// <returns>Returns a list of matches songs</returns>
		public List<Song> getSearchResults(string needle, int mode)
        {
			needle = needle.Trim().ToLower();
			needle = needle.Replace(",", "");
			needle = needle.Replace(".", "");
			needle = needle.Replace(";", "");
			needle = needle.Replace(Environment.NewLine, "");
			needle = needle.Replace("  ", " ");

            List<Song> tmpList = new List<Song>();
            foreach (KeyValuePair<Guid,Song> kvp in SongList)
            {
                if (SongList[kvp.Key].Title.ToLower().Contains(needle) ||
					(mode == 1 && SongList[kvp.Key].SearchText.Contains(needle)))
                {
                    tmpList.Add(SongList[kvp.Key]);
                }
            }
            return tmpList;
        }
    }
}
