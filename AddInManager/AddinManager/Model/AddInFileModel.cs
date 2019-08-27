using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace AddinManager.Model
{
    class AddInFileModel
    {
        public string Name { get; set; }

        public Guid AddInId { get; set; }

        public string Assembly { get; set; }

        public string Description { get; set; }

        public static AddInFileModel Resolve(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }

            var result = new AddInFileModel();

            try
            {
                var context = File.ReadAllText(file);

                var doc = new XmlDocument();

                doc.LoadXml(context);

                var node = doc.SelectSingleNode("MSAddIn");

                if (node == null)
                {
                    return null;
                }

                result.Name = node.SelectSingleNode("Name").FirstChild.Value;

                result.Description = node.SelectSingleNode("Description").FirstChild.Value;

                result.AddInId = new Guid(node.SelectSingleNode("AddInId").FirstChild.Value);

                result.Assembly = node.SelectSingleNode("Assembly").FirstChild.Value;
            }
            catch (Exception ex)
            {
                
            }

            return result;
        }
    }
}
