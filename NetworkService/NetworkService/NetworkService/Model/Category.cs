using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{

    public class Category
    {
        public string Name { get; set; }
        public ObservableCollection<ReactorTemperature> ReactorTemperatures { get; set; }

        public Category(string name)
        {
            Name = name;
            ReactorTemperatures = new ObservableCollection<ReactorTemperature>();
        }
    }

}
