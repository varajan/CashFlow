using System;
using System.Collections.Generic;
using CashFlow.ViewModels;
using CashFlow.Views;
using Xamarin.Forms;

namespace CashFlow
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
