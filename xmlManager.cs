using System;
using System.IO;
using System.Xml;

namespace Managers{

    public static class XmlManager{

        /// <summary>
        /// Creates a new XmlDocument object, loading the given file
        /// </summary>
        /// <param name="name">The name of the file to load</param>
        /// <returns>An XmlDocument object containing the files data</returns>
        public static XmlDocument LoadDocument(string name){

            XmlDocument loadedDocument = new XmlDocument();

            try{

                loadedDocument.Load($"xmldocs/{name}");
                
            }
            catch (FileNotFoundException){

                throw new FileNotFoundException($"The {name} document is either missing or corrupt.");

            }

            return loadedDocument;

        }

        public static (bool, XmlNode) GetMatchInNodeList(string textToMatch, XmlNodeList group, string attributeToRead = ""){

            (bool, XmlNode) result = (false, null);

            foreach (XmlNode node in group){

                if (textToMatch == node.InnerXml && attributeToRead == ""){

                    result = (true, node);

                }
                else if (attributeToRead != ""){

                    if (node.Attributes[attributeToRead] != null){

                        if (node.Attributes[attributeToRead].Value == textToMatch){

                            result = (true, node);

                        }

                    }

                }

            }

            return result;



        }

        public static (bool, XmlNode) GetFirstLevelChild(string textToMatch, ref XmlDocument document, string attributeToRead = "") => GetMatchInNodeList(textToMatch, document.FirstChild.ChildNodes, attributeToRead);

        /// <summary>
        /// Determines whether an XmlDocument contains a given element two levels lower than the roow
        /// </summary>
        /// <param name="textToMatch">The word to check for</param>
        /// <param name="document">The document to lookup</param>
        /// <returns>A tuple containing success, the first level node and the matched node</returns>
        public static (bool, XmlNode, XmlNode) GetSecondLevelChild(string textToMatch, ref XmlDocument document, string attributeToRead = ""){

            (bool, XmlNode, XmlNode) result = (false, null, null);
            
            foreach (XmlNode group in document.FirstChild.ChildNodes){

                (bool success, XmlNode node) match = GetMatchInNodeList(textToMatch, group.ChildNodes, attributeToRead);

                if (match.success){

                    return (true, group, match.node);

                }

            }

            return result;

        }

    }

}