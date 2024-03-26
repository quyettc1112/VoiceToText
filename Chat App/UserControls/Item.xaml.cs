using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chat_App.UserControls
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        public Item()
        {
            InitializeComponent();
        }

        public string Title1
        {
            get { return (string)GetValue(Title1Property); }
            set { SetValue(Title1Property, value); }
        }

        public static readonly DependencyProperty Title1Property = DependencyProperty.Register("Title1", typeof(string), typeof(AccountMenuButton));

        public bool IsActive1
        {
            get { return (bool)GetValue(IsActive1Property); }
            set { SetValue(IsActive1Property, value); }
        }

        public static readonly DependencyProperty IsActive1Property = DependencyProperty.Register("IsActive1", typeof(bool), typeof(AccountMenuButton));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(AccountMenuButton));

        public Visibility Visible
        {
            get { return (Visibility)GetValue(VisiblProperty); }
            set { SetValue(VisiblProperty, value); }
        }

        public static readonly DependencyProperty VisiblProperty = DependencyProperty.Register("Visibl", typeof(Visibility), typeof(AccountMenuButton));

    }
}
