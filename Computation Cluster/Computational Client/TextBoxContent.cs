using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Client
{
    public class TextBoxContent : INotifyPropertyChanged
    {
        public string textBoxContent;

        public event PropertyChangedEventHandler PropertyChanged;

        public TextBoxContent(){}

        public TextBoxContent(string value)
        {
            this.textBoxContent = value;
        }

        public string Content
        {
            get
            {
                return this.textBoxContent; 
            }

            set
            {
                this.textBoxContent = value;
                OnPropertyChanged("TextBoxContent");
            }
        }

        private void OnPropertyChanged(string textBoxContent)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(textBoxContent));
            }
        }
    }
}
