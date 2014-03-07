using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Barak.VersionPatcher.Engine.CSProj
{
    public class ProjectFileParser
    {
        public class Attribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class Element
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public IEnumerable<Attribute>  Attributes { get; set; }
        }
        public static ProjectFileType ParseFile(Stream stream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            var projectFileType = new ProjectFileType();

            Parse(doc.DocumentElement, projectFileType);

            return projectFileType;
        }

        private static void Parse(XmlElement xmlNode, BaseElementType instance)
        {
            var instanceType = instance.GetType();
            instance.Value = xmlNode.Value;
            foreach (var attribute in (xmlNode as XmlElement).Attributes.OfType<XmlAttribute>())
            {
                instance.Items = instance.Items.ArrayAdd(new Attribute() {Name = attribute.Name, Value = attribute.Value});
                var prop = instanceType.GetProperty(attribute.Name);
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(instance, attribute.Value, null);
                }
            }

            foreach (var childElement in xmlNode.ChildNodes.OfType<XmlElement>())
            {
                instance.Items = instance.Items.ArrayAdd(new Element()
                                        {
                                            Name = childElement.Name,
                                            Value = childElement.InnerXml,
                                            Attributes = childElement.Attributes.OfType<XmlAttribute>().Select(p=>new Attribute()
                                                                                                                  {
                                                                                                                      Name = p.Name,
                                                                                                                      Value = p.Value,
                                                                                                                  }).ToList(),
                                        });

                var prop = instanceType.GetProperty(childElement.Name);
                if (prop != null)
                {
                    if (prop.PropertyType == typeof (string))
                    {
                        prop.SetValue(instance,childElement.InnerText,null);
                    }
                    else 
                    {
                        if (prop.PropertyType.IsArray)
                        {
                            var newPropInstance = Activator.CreateInstance(prop.PropertyType.GetElementType()) as BaseElementType;
                            Parse(childElement, newPropInstance);


                            Array newArray = null;
                            // Do something
                            var propValue = prop.GetValue(instance, null) as Array;
                            if (propValue == null)
                            {
                                newArray = Activator.CreateInstance(prop.PropertyType, new object[] { 1 }) as Array;
                                newArray.SetValue(newPropInstance, 0);
                            }
                            else
                            {
                                newArray = Activator.CreateInstance(prop.PropertyType, new object[] { (int)propValue.Length + 1 }) as Array;
                                int index = 0;
                                foreach (var value in propValue)
                                {
                                    newArray.SetValue(value, index);
                                    index++;
                                }
                                newArray.SetValue(newPropInstance, index);
                            }
                            prop.SetValue(instance, newArray, null);
                        }
                        else
                        {                            
                            var propInstance = Activator.CreateInstance(prop.PropertyType) as BaseElementType;
                            prop.SetValue(instance, propInstance, null);
                            Parse(childElement, propInstance);
                        }
                    }
                }
            }
        }
    }
}