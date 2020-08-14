using SGT_Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailPageApp : MasterDetailPage
    {
        public MasterDetailPageApp()
        {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
        }

        public async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            if (item == null)
                return;

            if(item.Id == 1)
            {
                await DisplayAlert("Alert", "Logout Successful", "OK");
                Application.Current.Properties["token"] = null;
                App.Current.MainPage = new NavigationPage(new Login());
                return;
            }

            item.Pagina.Title = item.Title;
            Detail = new NavigationPage(new MyServices());

            IsPresented = false;

            MasterPage.ListView.SelectedItem = null;
        }
    }
}