using System;
using System.Collections.Generic;
using System.ComponentModel;
using CashFlow.Models;
using CashFlow.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CashFlow.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}