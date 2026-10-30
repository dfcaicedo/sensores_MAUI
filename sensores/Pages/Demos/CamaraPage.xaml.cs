namespace sensores.Pages.Demos
{
    public partial class CamaraPage : ContentPage
    {
        public CamaraPage()
        {
            InitializeComponent();
        }

        async void OnTomarFoto(object? sender, EventArgs e)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                Estado.Text = "Este dispositivo no permite capturar fotos.";
                return;
            }

            await EjecutarAsync(() => MediaPicker.Default.CapturePhotoAsync());
        }

        async void OnGaleria(object? sender, EventArgs e) =>
            await EjecutarAsync(async () => (await MediaPicker.Default.PickPhotosAsync())?.FirstOrDefault());

        async Task EjecutarAsync(Func<Task<FileResult?>> obtenerFoto)
        {
            try
            {
                FileResult? foto = await obtenerFoto();
                if (foto is null)
                {
                    Estado.Text = "Cancelado por el usuario.";
                    return;
                }

                // Se copia a la caché de la app para poder mostrarla desde un archivo.
                string ruta = Path.Combine(FileSystem.CacheDirectory, foto.FileName);
                using (Stream origen = await foto.OpenReadAsync())
                using (FileStream destino = File.Create(ruta))
                    await origen.CopyToAsync(destino);

                Foto.Source = ImageSource.FromFile(ruta);
                Vacio.IsVisible = false;
                Estado.Text = $"{foto.FileName} · {new FileInfo(ruta).Length / 1024} KB";
            }
            catch (PermissionException)
            {
                Estado.Text = "Permiso de cámara denegado. Actívalo en los ajustes de la app.";
            }
            catch (FeatureNotSupportedException)
            {
                Estado.Text = "La cámara no está disponible en este dispositivo.";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
        }
    }
}
