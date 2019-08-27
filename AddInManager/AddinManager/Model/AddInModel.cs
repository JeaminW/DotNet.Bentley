using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace AddinManager.Model
{
    class AddInModel : AddinManager.MVVM.ObservableObject
    {
        private AddInModel m_Parent;
        private string m_Name;
        private string m_Path;
        private bool m_IsExpand;
        private ObservableCollection<AddInModel> m_Childs;

        public AddInModel Parent
        {
            get
            {
                return m_Parent;
            }

            set
            {
                m_Parent = value;
                this.RaisePropertyChanged(nameof(Parent));
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                m_Name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get
            {
                return m_Path;
            }

            set
            {
                m_Path = value;
                this.RaisePropertyChanged(nameof(Path));
            }
        }

        public bool IsExpand
        {
            get
            {
                return m_IsExpand;
            }

            set
            {
                m_IsExpand = value;
                this.RaisePropertyChanged(nameof(IsExpand));
            }
        }

        public ObservableCollection<AddInModel> Childs
        {
            get
            {
                return m_Childs;
            }

            set
            {
                m_Childs = value;
                this.RaisePropertyChanged(nameof(Childs));
            }
        }


        public AddInModel()
        {
            this.IsExpand = true;
            this.Childs = new ObservableCollection<AddInModel>();
        }
    }
}
