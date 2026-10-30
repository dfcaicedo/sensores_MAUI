namespace sensores.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        async void OnIr(object? sender, EventArgs e)
        {
            if (sender is Button { CommandParameter: string ruta })
                await Shell.Current.GoToAsync($"//{ruta}");
        }
    }
}
