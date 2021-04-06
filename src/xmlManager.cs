using System;
using System.Collections.Generic;
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

        public static Dictionary<string, string> GetMapFromDocument(XmlNodeList nodes, string key, string value){

            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (XmlNode node in nodes){

                if (node.Attributes[key] != null & node.Attributes[value] != null){

                    map.Add(node.Attributes[key].Value, node.Attributes[value].Value);

                }

            }

            return map;

        }

        public static (bool, XmlNode) GetMatchInNodeList(string textToMatch, XmlNodeList group, string attributeToRead = "", bool matchInnerXml = false){

            (bool, XmlNode) result = (false, null);

            foreach (XmlNode node in group){
                
                if (textToMatch == node.Name && attributeToRead == ""){
                    
                    result = (true, node);

                    break;

                }
                else if (textToMatch == node.InnerXml && matchInnerXml){

                    result = (true, node);

                    break;

                }
                else if (attributeToRead != ""){

                    if (node.Attributes[attributeToRead] != null){

                        if (node.Attributes[attributeToRead].Value == textToMatch){

                            result = (true, node);

                            break;

                        }

                    }

                }

            }

            return result;



        }

        public static (bool, XmlNode) GetFirstLevelChild(string textToMatch, ref XmlDocument document, string attributeToRead = "", bool matchInnerXml = false) => GetMatchInNodeList(textToMatch, document.FirstChild.ChildNodes, attributeToRead, matchInnerXml);

        /// <summary>
        /// Determines whether an XmlDocument contains a given element two levels lower than the root
        /// </summary>
        /// <param name="textToMatch">The word to check for</param>
        /// <param name="document">The document to lookup</param>
        /// <returns>A tuple containing success, the first level node and the matched node</returns>
        public static (bool, XmlNode, XmlNode) GetSecondLevelChild(string textToMatch, ref XmlDocument document, string attributeToRead = "", bool matchInnerXml = false){

            (bool, XmlNode, XmlNode) result = (false, null, null);
            
            foreach (XmlNode group in document.FirstChild.ChildNodes){

                (bool success, XmlNode node) match = GetMatchInNodeList(textToMatch, group.ChildNodes, attributeToRead, matchInnerXml);

                if (match.success){

                    return (true, group, match.node);

                }

            }

            return result;

        }

        public static XmlNodeList GetFirstLevelNodeChildren(string documentName, string name){

            XmlDocument document = LoadDocument(documentName);

            return GetFirstLevelNodeChildren(name, ref document);

        }

        public static XmlNodeList GetFirstLevelNodeChildren(string name, ref XmlDocument document){

            (bool success, XmlNode node) match = XmlManager.GetFirstLevelChild(name, ref document, "name");

            if (!match.success){

                return null;

            }

            return match.node.ChildNodes;

        }

    }

}