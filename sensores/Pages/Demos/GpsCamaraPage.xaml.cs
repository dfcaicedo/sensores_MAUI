using System.Globalization;

namespace sensores.Pages.Demos
{
    public partial class GpsCamaraPage : ContentPage
    {
        Location? _ubicacion;

        public GpsCamaraPage()
        {
            InitializeComponent();
        }

        async void OnTomar(object? sender, EventArgs e)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                Estado.Text = "Este dispositivo no permite capturar fotos.";
                return;
            }

            Task<Location?>? tareaUbicacion = null;
            try
            {
                TomarButton.IsEnabled = false;

                // 1. Pedir el permiso antes de abrir la cámara para que los diálogos no se pisen.
                var permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (permiso != PermissionStatus.Granted)
                    Estado.Text = "Sin permiso de ubicación: la foto saldrá sin coordenadas.";
                else
                    // 2. El GPS busca mientras el usuario toma la foto.
                    tareaUbicacion = Geolocation.Default.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

                // 3. Cámara
                FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();

                // 4. Unir ambos resultados
                Location? ubicacion = null;
                if (tareaUbicacion is not null)
                {
                    try { ubicacion = await tareaUbicacion; }
                    catch (Exception ex) when (ex is FeatureNotEnabledException or FeatureNotSupportedException)
                    {
                        Estado.Text = "El GPS no está disponible o está apagado.";
                    }
                }

                if (foto is null)
                {
                    Estado.Text = "Cancelado por el usuario.";
                    return;
                }

                string ruta = Path.Combine(FileSystem.CacheDirectory, foto.FileName);
                using (Stream origen = await foto.OpenReadAsync())
                using (FileStream destino = File.Create(ruta))
                    await origen.CopyToAsync(destino);

                Foto.Source = ImageSource.FromFile(ruta);
                Vacio.IsVisible = false;
                _ubicacion = ubicacion;

                string fecha = DateTime.Now.ToString("g");
                EtiquetaLabel.Text = ubicacion is null
                    ? $"Sin ubicación · {fecha}"
                    : $"📍 {ubicacion.Latitude:F5}, {ubicacion.Longitude:F5} (±{ubicacion.Accuracy:F0} m) · {fecha}";
                Etiqueta.IsVisible = true;
                MapaButton.IsEnabled = ubicacion is not null;

                if (ubicacion is not null)
                    Estado.Text = "Foto guardada con su ubicación.";
            }
            catch (PermissionException)
            {
                Estado.Text = "Permiso denegado. Actívalo en los ajustes de la app.";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
            finally
            {
                TomarButton.IsEnabled = true;
            }
        }

        async void OnMapa(object? sender, EventArgs e)
        {
            if (_ubicacion is null) return;

            string lat = _ubicacion.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = _ubicacion.Longitude.ToString(CultureInfo.InvariantCulture);
            await Launcher.Default.OpenAsync($"https://www.openstreetmap.org/?mlat={lat}&mlon={lon}#map=17/{lat}/{lon}");
        }
    }
}
