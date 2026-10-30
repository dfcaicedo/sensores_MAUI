namespace sensores.Pages.Demos
{
    public partial class ArchivosPage : ContentPage
    {
        static readonly string[] ExtensionesTexto = [".txt", ".csv", ".json", ".md", ".xml", ".log"];

        static string RutaNota => Path.Combine(FileSystem.AppDataDirectory, "nota.txt");

        public ArchivosPage()
        {
            InitializeComponent();
            TipoPicker.SelectedIndex = 0; // en XAML se aplica antes de cargar los elementos y no se ve
        }

        PickOptions CrearOpciones()
        {
            var opciones = new PickOptions { PickerTitle = "Elige un archivo" };

            switch (TipoPicker.SelectedIndex)
            {
                case 1:
                    opciones.FileTypes = FilePickerFileType.Images;
                    break;
                case 2:
                    opciones.FileTypes = FilePickerFileType.Pdf;
                    break;
                case 3:
                    // Cada plataforma identifica los tipos de forma distinta.
                    opciones.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "text/*", "application/json" } },
                        { DevicePlatform.iOS, new[] { "public.text" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.text" } },
                        { DevicePlatform.WinUI, ExtensionesTexto },
                    });
                    break;
            }

            return opciones;
        }

        async void OnElegir(object? sender, EventArgs e)
        {
            try
            {
                FileResult? archivo = await FilePicker.Default.PickAsync(CrearOpciones());
                if (archivo is null)
                {
                    Estado.Text = "Cancelado por el usuario.";
                    return;
                }

                InfoBorder.IsVisible = true;
                Vista.IsVisible = false;
                TextoLabel.IsVisible = false;

                long bytes;
                using (Stream stream = await archivo.OpenReadAsync())
                    bytes = stream.CanSeek ? stream.Length : -1;

                InfoLabel.Text =
                    $"Nombre: {archivo.FileName}\n" +
                    $"Tipo: {archivo.ContentType ?? "desconocido"}\n" +
                    $"Tamaño: {(bytes >= 0 ? $"{bytes / 1024.0:F1} KB" : "desconocido")}\n" +
                    $"Ruta: {archivo.FullPath}";

                string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

                if (archivo.ContentType?.StartsWith("image/") == true)
                {
                    Vista.Source = ImageSource.FromStream(() => archivo.OpenReadAsync().GetAwaiter().GetResult());
                    Vista.IsVisible = true;
                }
                else if (archivo.ContentType?.StartsWith("text/") == true || ExtensionesTexto.Contains(extension))
                {
                    using var lector = new StreamReader(await archivo.OpenReadAsync());
                    char[] buffer = new char[500];
                    int leidos = await lector.ReadAsync(buffer, 0, buffer.Length);
                    TextoLabel.Text = new string(buffer, 0, leidos) + (leidos == buffer.Length ? "…" : "");
                    TextoLabel.IsVisible = true;
                }

                Estado.Text = "Archivo seleccionado.";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
        }

        async void OnGuardar(object? sender, EventArgs e)
        {
            try
            {
                await File.WriteAllTextAsync(RutaNota, NotaEditor.Text ?? string.Empty);
                Estado.Text = $"Guardado en {RutaNota}";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
        }

        async void OnLeer(object? sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(RutaNota))
                {
                    Estado.Text = "Todavía no hay una nota guardada.";
                    return;
                }

                NotaEditor.Text = await File.ReadAllTextAsync(RutaNota);
                Estado.Text = "Nota leída del archivo.";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
        }
    }
}
