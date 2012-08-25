﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pbp.Data.Bible;
using System.Xml;
using System.IO;

namespace Pbp.IO
{
    public class XMLBibleReblader
    {
        private bool getNodeText(XmlTextReader textReader, string name, ref Bible target, string fieldName)
        {
            if (textReader.NodeType == XmlNodeType.Element && textReader.Name.ToString().ToLower() == name)
            {
                textReader.Read();
                if (textReader.NodeType == XmlNodeType.Text)
                {
                    Type myType = typeof(Bible);
                    System.Reflection.PropertyInfo myFields = myType.GetProperty(fieldName);
                    myFields.SetValue(target, textReader.Value,null);
                }
            }
            return false;
        }

        public Bible loadMeta(string fileName)
        {
            Bible b = new Bible();

            
            // Read a document
            XmlTextReader textReader = new XmlTextReader(fileName);
            
            // Read until end of file
            while (textReader.Read())
            {
                if (textReader.NodeType == XmlNodeType.Element && textReader.Name.ToString().ToLower() == "xmlbible")
                {
                    while (textReader.Read())
                    {
                        if (textReader.NodeType == XmlNodeType.Element)
                        {
                            if (getNodeText(textReader, "title", ref b, "Title")) continue;
                            if (getNodeText(textReader, "description", ref b, "Description")) continue;
                            if (getNodeText(textReader, "contributors", ref b, "Contributors")) continue;
                            if (getNodeText(textReader, "language", ref b, "Language")) continue;
                            if (getNodeText(textReader, "publisher", ref b, "Publisher")) continue;
                            if (getNodeText(textReader, "date", ref b, "Source")) continue;
                            if (getNodeText(textReader, "source", ref b, "Date")) continue;
                            if (getNodeText(textReader, "identifier", ref b, "Identifier")) continue;
                        }
                    }
                    break;
                }
            }
            return b;
        }

        public Bible loadContent(string fileName, Bible b)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            if (xmlRoot.Name == "XMLBIBLE")
            {
                b.Books = new List<Book>();
                foreach (XmlNode bookNode in xmlRoot.ChildNodes)
                {
                    if (bookNode.Name.ToLower() == "biblebook")
                    {
                        Book bo = new Book();
                        bo.Bible = b;
                        bo.Number = int.Parse(bookNode.Attributes["bnumber"].InnerText);
                        bo.Name = bookNode.Attributes["bname"] != null ? bookNode.Attributes["bname"].InnerText : Bible.bookMap[bo.Number - 1];
                        bo.ShortName = bookNode.Attributes["bsname"] != null ? bookNode.Attributes["bsname"].InnerText : bo.Name;
                        bo.Chapters = new List<Chapter>();
                        foreach (XmlNode chapNode in bookNode.ChildNodes)
                        {
                            if (chapNode.Name.ToLower() == "chapter")
                            {
                                Chapter ch = new Chapter();
                                ch.Book = bo;
                                ch.Number = int.Parse(chapNode.Attributes["cnumber"].InnerText);
                                ch.Verses = new List<Verse>();
                                foreach (XmlNode verseNode in chapNode.ChildNodes)
                                {
                                    if (verseNode.Name.ToLower() == "vers")
                                    {
                                        Verse v = new Verse();
                                        v.Chapter = ch;
                                        v.Number = int.Parse(verseNode.Attributes["vnumber"].InnerText);
                                        v.Text = verseNode.InnerText;
                                        ch.Verses.Add(v);
                                    }
                                }
                                bo.Chapters.Add(ch);
                            }
                        }
                        b.Books.Add(bo);
                    }
                }
            }
            return b;
        }
    }
}