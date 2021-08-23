using System.ComponentModel;
using CashFlow.ViewModels;
using Xamarin.Forms;

namespace CashFlow.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}