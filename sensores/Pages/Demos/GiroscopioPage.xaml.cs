using System.Diagnostics;

namespace sensores.Pages.Demos
{
    public partial class GiroscopioPage : ContentPage
    {
        const double RadAGrados = 180.0 / Math.PI;

        readonly Stopwatch _reloj = new();
        double _rotX, _rotY, _rotZ;

        public GiroscopioPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!Gyroscope.Default.IsSupported)
            {
                Estado.Text = "Este dispositivo no tiene giroscopio (normal en equipos de escritorio y algunos emuladores).";
                ToggleButton.IsEnabled = false;
                return;
            }

            Iniciar();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Detener();
        }

        void Iniciar()
        {
            if (Gyroscope.Default.IsMonitoring)
                return;

            Gyroscope.Default.ReadingChanged += OnLectura;
            Gyroscope.Default.Start(SensorSpeed.UI);
            _reloj.Restart();
            Estado.Text = "Gira el teléfono para ver cómo cambian los valores.";
            ToggleButton.Text = "Detener";
        }

        void Detener()
        {
            if (!Gyroscope.Default.IsSupported || !Gyroscope.Default.IsMonitoring)
                return;

            Gyroscope.Default.Stop();
            Gyroscope.Default.ReadingChanged -= OnLectura;
            Estado.Text = "Lectura detenida.";
            ToggleButton.Text = "Iniciar";
        }

        // Este evento llega en un hilo secundario.
        void OnLectura(object? sender, GyroscopeChangedEventArgs e)
        {
            var w = e.Reading.AngularVelocity; // rad/s
            double dt = _reloj.Elapsed.TotalSeconds;
            _reloj.Restart();

            _rotX += w.X * RadAGrados * dt;
            _rotY += w.Y * RadAGrados * dt;
            _rotZ += w.Z * RadAGrados * dt;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                XLabel.Text = w.X.ToString("F3");
                YLabel.Text = w.Y.ToString("F3");
                ZLabel.Text = w.Z.ToString("F3");

                Cubo.RotationX = _rotX;
                Cubo.RotationY = _rotY;
                Cubo.Rotation = _rotZ;
                AngulosLabel.Text = $"X {_rotX % 360:F0}°   Y {_rotY % 360:F0}°   Z {_rotZ % 360:F0}°";
            });
        }

        void OnToggle(object? sender, EventArgs e)
        {
            if (Gyroscope.Default.IsMonitoring) Detener(); else Iniciar();
        }

        void OnReiniciar(object? sender, EventArgs e)
        {
            _rotX = _rotY = _rotZ = 0;
            Cubo.RotationX = Cubo.RotationY = Cubo.Rotation = 0;
            AngulosLabel.Text = string.Empty;
        }
    }
}
