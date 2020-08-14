using Newtonsoft.Json;
using SGT_Mobile;
using SGTMobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterMenu : ContentPage
    {
        public ListView ListView;
        private RecursoIngeniero Ingeniero;

        public MasterMenu()
        {
            InitializeComponent();

            BindingContext = new MasterDetailPage1MasterViewModel();
            ListView = MenuItemsListView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                this.Ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);
                lbl_nombreFE.Text = Ingeniero.Name;
                lbl_emailFE.Text = Ingeniero.Email;
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new Login());
            }
        }

        class MasterDetailPage1MasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MenuItem> MenuItems { get; set; }

            public MasterDetailPage1MasterViewModel()
            {
                MenuItems = new ObservableCollection<MenuItem>(new[]
                {
                    new MenuItem { Id = 0, Title = "My Services", Pagina = new MyServices(), Imagen = "homeIcon" },
                    new MenuItem { Id = 1, Title = "Logout", Imagen = "signOut" },

                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}