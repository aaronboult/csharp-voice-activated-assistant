using System;
using System.IO;
using System.Xml;

namespace Managers{

    public static class XmlManager{

        public static XmlDocument LoadDocument(string name){

            XmlDocument loadedDocument = new XmlDocument();

            try{

                loadedDocument.Load(name);
                
            }
            catch (FileNotFoundException){

                throw new Exception($"The {name} document is either missing or corrupt.");

            }

            return loadedDocument;

        }

        /// <summary>
        /// Determines whether an XmlDocument contains a given element two levels lower than the roow
        /// </summary>
        /// <param name="tagName">The word to check for</param>
        /// <param name="document">The document to lookup</param>
        /// <returns>A tuple containing success, the first level node and the matched node</returns>
        public static (bool, XmlNode, XmlNode) HasSecondLevelChild(string tagName, ref XmlDocument document, string attributeToRead = ""){
            
            foreach (XmlNode group in document.FirstChild.ChildNodes){

                foreach (XmlNode keyword in group.ChildNodes){

                    if (tagName == keyword.InnerXml){

                        return (true, group, keyword);

                    }
                    else if (attributeToRead != ""){

                        if (keyword.Attributes[attributeToRead] != null){

                            if (keyword.Attributes[attributeToRead].Value == tagName){

                                return (true, group, keyword);

                            }

                        }

                    }

                }

            }

            return (false, null, null);

        }

    }

}