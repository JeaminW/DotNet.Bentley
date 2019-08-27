using System;
using System.Collections.Generic;
using System.Text;

namespace AddinManager.MVVM
{
    /// <summary>
    /// 属性通知基类.
    /// </summary>
     class ObservableObject : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变后触发事件.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性改变前触发事件.
        /// </summary>
        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// 属性改变委托处理.
        /// </summary>
        protected System.ComponentModel.PropertyChangedEventHandler PropertyChangedHandler
        {
            get
            {
                return this.PropertyChanged;
            }
        }

        /// <summary>
        /// 属性改变委托处理.
        /// </summary>
        protected System.ComponentModel.PropertyChangingEventHandler PropertyChangingHandler
        {
            get
            {
                return this.PropertyChanging;
            }
        }

        /// <summary>
        /// 属性改变之前回调.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanging(string propertyName)
        {

            this.PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// 属性改变之后回调.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
