using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetworkService.Helpers
{
    public static class XmlHelper
    {
        // Method to deserialize XML file into a collection of entities
        public static ObservableCollection<ReactorTemperature> LoadData(string filePath)
        {
            ObservableCollection<ReactorTemperature> entityList = new ObservableCollection<ReactorTemperature>();

            // Check if the XML file exists
            if (File.Exists(filePath))
            {
                // Create an XmlSerializer for the YourEntity type
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ReactorTemperature>));

                // Read the XML file and deserialize its contents into the entityList
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    entityList = (ObservableCollection<ReactorTemperature>)serializer.Deserialize(fileStream);
                }
            }

            return entityList;
        }

        public static void SaveData(ObservableCollection<ReactorTemperature> data, string filePath)
        {
            // Create an XmlSerializer for the FlowMeter type
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<ReactorTemperature>));

            // Write the data to an XML file
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, data);
            }
        }
    }
}
