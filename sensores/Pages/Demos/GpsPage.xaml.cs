using System.Globalization;

namespace sensores.Pages.Demos
{
    public partial class GpsPage : ContentPage
    {
        Location? _ultima;

        public GpsPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DetenerSeguimiento();
        }

        async void OnObtener(object? sender, EventArgs e)
        {
            try
            {
                ObtenerButton.IsEnabled = false;
                Estado.Text = "Buscando ubicación…";

                var peticion = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
                Location? ubicacion = await Geolocation.Default.GetLocationAsync(peticion)
                                      ?? await Geolocation.Default.GetLastKnownLocationAsync();

                if (ubicacion is null)
                    Estado.Text = "No se pudo obtener la ubicación.";
                else
                {
                    Mostrar(ubicacion);
                    Estado.Text = "Ubicación obtenida.";
                }
            }
            catch (FeatureNotSupportedException)
            {
                Estado.Text = "Este dispositivo no soporta geolocalización.";
            }
            catch (FeatureNotEnabledException)
            {
                Estado.Text = "El GPS está apagado. Actívalo en los ajustes.";
            }
            catch (PermissionException)
            {
                Estado.Text = "Permiso de ubicación denegado.";
            }
            catch (Exception ex)
            {
                Estado.Text = $"Error: {ex.Message}";
            }
            finally
            {
                ObtenerButton.IsEnabled = true;
            }
        }

        async void OnSeguir(object? sender, EventArgs e)
        {
            if (Geolocation.Default.IsListeningForeground)
            {
                DetenerSeguimiento();
                Estado.Text = "Seguimiento detenido.";
                return;
            }

            try
            {
                Geolocation.Default.LocationChanged += OnUbicacionCambio;
                Geolocation.Default.ListeningFailed += OnFalloSeguimiento;

                var peticion = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2));
                bool iniciado = await Geolocation.Default.StartListeningForegroundAsync(peticion);

                if (iniciado)
                {
                    SeguirButton.Text = "⏹  Detener seguimiento";
                    Estado.Text = "Siguiendo tu posición… camina para ver los cambios.";
                }
                else
                {
                    DetenerSeguimiento();
                    Estado.Text = "No se pudo iniciar el seguimiento.";
                }
            }
            catch (PermissionException)
            {
                DetenerSeguimiento();
                Estado.Text = "Permiso de ubicación denegado.";
            }
            catch (Exception ex)
            {
                DetenerSeguimiento();
                Estado.Text = $"Error: {ex.Message}";
            }
        }

        void DetenerSeguimiento()
        {
            Geolocation.Default.LocationChanged -= OnUbicacionCambio;
            Geolocation.Default.ListeningFailed -= OnFalloSeguimiento;

            if (Geolocation.Default.IsListeningForeground)
                Geolocation.Default.StopListeningForeground();

            SeguirButton.Text = "▶  Seguir en tiempo real";
        }

        void OnUbicacionCambio(object? sender, GeolocationLocationChangedEventArgs e) =>
            MainThread.BeginInvokeOnMainThread(() => Mostrar(e.Location));

        void OnFalloSeguimiento(object? sender, GeolocationListeningFailedEventArgs e) =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DetenerSeguimiento();
                Estado.Text = $"El seguimiento falló: {e.Error}";
            });

        void Mostrar(Location loc)
        {
            _ultima = loc;
            LatLabel.Text = loc.Latitude.ToString("F6");
            LonLabel.Text = loc.Longitude.ToString("F6");
            AltLabel.Text = loc.Altitude is double alt ? $"{alt:F1} m" : "no disponible";
            PrecLabel.Text = loc.Accuracy is double acc ? $"±{acc:F0} m" : "no disponible";
            VelLabel.Text = loc.Speed is double vel ? $"{vel * 3.6:F1} km/h" : "no disponible";
            RumboLabel.Text = loc.Course is double rumbo ? $"{rumbo:F0}°" : "no disponible";
            HoraLabel.Text = loc.Timestamp.LocalDateTime.ToString("T");
            MapaButton.IsEnabled = true;
        }

        async void OnMapa(object? sender, EventArgs e)
        {
            if (_ultima is null) return;

            string lat = _ultima.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = _ultima.Longitude.ToString(CultureInfo.InvariantCulture);
            await Launcher.Default.OpenAsync($"https://www.openstreetmap.org/?mlat={lat}&mlon={lon}#map=17/{lat}/{lon}");
        }
    }
}
