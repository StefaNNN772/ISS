using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class ReactorTemperature : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int _id;
        string _name;
        EntityType _entityType;
        int _value;
        List<Pair<DateTime, int>> _last_5;
        ValueMeasure _valueMeasure;

        public ReactorTemperature()
        {
            Last_5 = new List<Pair<DateTime, int>>();
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ValueMeasure ValueMeasure
        {
            get
            {
                return _valueMeasure;
            }
            set
            {
                if (_valueMeasure != value)
                {
                    _valueMeasure = value;
                    OnPropertyChanged(nameof(ValueMeasure));
                }
            }
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        public EntityType EntityType
        {
            get
            {
                return _entityType;
            }
            set
            {
                if (_entityType != value)
                {
                    _entityType = value;
                    OnPropertyChanged(nameof(EntityType));
                }
            }
        }

        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));

                    if (_value < 250)
                    {
                        ValueMeasure = ValueMeasure.Low;
                    }
                    else if (_value > 350)
                    {
                        ValueMeasure = ValueMeasure.High;
                    }
                    else
                    {
                        ValueMeasure = ValueMeasure.Normal;
                    }
                }
            }
        }

        public List<Pair<DateTime, int>> Last_5
        {
            get
            {
                return _last_5;
            }
            set
            {
                if (_last_5 != value)
                {
                    _last_5 = value;
                    OnPropertyChanged(nameof(Last_5));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
