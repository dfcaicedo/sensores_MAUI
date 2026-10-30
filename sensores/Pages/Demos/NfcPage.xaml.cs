namespace sensores.Pages.Demos
{
    public partial class NfcPage : ContentPage
    {
        public NfcPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AjustesButton.IsVisible = false;

            if (!NfcReader.EstaDisponible())
            {
                Estado.Text = "NFC no está disponible: este ejemplo solo lo implementa en Android con hardware NFC.";
                return;
            }

            if (!NfcReader.EstaActivado())
            {
                Estado.Text = "El NFC está apagado. Actívalo y vuelve a esta pantalla.";
                AjustesButton.IsVisible = true;
                return;
            }

            NfcReader.TagLeido += OnTagLeido;
            NfcReader.Iniciar();
            Estado.Text = "📡 Acerca una etiqueta o tarjeta NFC a la parte trasera del teléfono.";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            NfcReader.TagLeido -= OnTagLeido;
            NfcReader.Detener();
        }

        void OnTagLeido(NfcTagInfo info) =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UidLabel.Text = $"UID: {info.Uid}";
                TecnologiasLabel.Text = $"Tecnologías: {string.Join(", ", info.Tecnologias)}";
                RegistrosLabel.Text = info.Registros.Length == 0
                    ? "Sin mensajes NDEF (la etiqueta está vacía o solo expone su UID)."
                    : string.Join("\n", info.Registros);
                ResultadoBorder.IsVisible = true;
                Estado.Text = "✅ Etiqueta leída. Puedes acercar otra.";
            });

        void OnAjustes(object? sender, EventArgs e) => NfcReader.AbrirAjustes();
    }
}
