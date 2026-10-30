using System.Diagnostics;

namespace sensores.Pages.Demos
{
    public partial class JuegoNivelPage : ContentPage
    {
        const double RadioArena = 147;   // 150 menos el trazo de 3 px del borde
        const double RadioEsfera = 18;
        const double RadioObjetivo = 45;
        const double Aceleracion = 900;   // px/s² por cada g de inclinación
        const double Rebote = 0.3;

        readonly Stopwatch _reloj = new();
        IDispatcherTimer? _timer;

        // Última lectura del acelerómetro (en g). Se escribe desde el hilo del sensor.
        volatile float _ax, _ay;

        // Estado de la esfera, relativo al centro de la arena.
        double _px, _py, _vx, _vy;
        double _racha, _mejor;

        public JuegoNivelPage()
        {
            InitializeComponent();
            PintarEsfera();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Accelerometer.Default.IsSupported)
            {
                if (!Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged += OnLectura;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
                Estado.Text = "Jugando con el acelerómetro.";
            }
            else
            {
                Simulador.IsVisible = true;
                Estado.Text = "Sin acelerómetro: usa los deslizadores.";
            }

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += OnTick;
            _reloj.Restart();
            _timer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _timer?.Stop();
            if (_timer is not null) _timer.Tick -= OnTick;

            if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= OnLectura;
            }
        }

        void OnLectura(object? sender, AccelerometerChangedEventArgs e)
        {
            // Se normaliza al convenio de iOS (X > 0 con el borde derecho abajo, Y < 0 con el teléfono vertical).
            // Android entrega los ejes con el signo contrario (comprobado en el emulador).
            float signo = DeviceInfo.Platform == DevicePlatform.Android ? -1f : 1f;
            _ax = signo * e.Reading.Acceleration.X;
            _ay = signo * e.Reading.Acceleration.Y;
        }

        void OnTick(object? sender, EventArgs e)
        {
            double dt = Math.Min(_reloj.Elapsed.TotalSeconds, 0.05);
            _reloj.Restart();

            double ax = Accelerometer.Default.IsSupported ? _ax : SimX.Value;
            double ay = Accelerometer.Default.IsSupported ? _ay : SimY.Value;

            // X > 0: borde derecho abajo -> la esfera rueda a la derecha.
            // Y > 0: borde superior abajo -> rueda hacia arriba (Y de pantalla decrece).
            _vx += ax * Aceleracion * dt;
            _vy += -ay * Aceleracion * dt;

            // Fricción
            double friccion = Math.Pow(0.3, dt);
            _vx *= friccion;
            _vy *= friccion;

            _px += _vx * dt;
            _py += _vy * dt;

            // Choque con el borde circular de la arena
            double maximo = RadioArena - RadioEsfera;
            double distancia = Math.Sqrt(_px * _px + _py * _py);
            if (distancia > maximo)
            {
                double nx = _px / distancia, ny = _py / distancia;
                _px = nx * maximo;
                _py = ny * maximo;

                double vn = _vx * nx + _vy * ny;
                if (vn > 0)
                {
                    _vx -= (1 + Rebote) * vn * nx;
                    _vy -= (1 + Rebote) * vn * ny;
                }
            }

            // Puntaje: tiempo seguido dentro del objetivo
            if (distancia <= RadioObjetivo)
            {
                _racha += dt;
                _mejor = Math.Max(_mejor, _racha);
            }
            else
            {
                _racha = 0;
            }

            PintarEsfera();
            RachaLabel.Text = $"{_racha:F1} s";
            MejorLabel.Text = $"{_mejor:F1} s";
            Objetivo.Stroke = distancia <= RadioObjetivo ? Colors.Gold : Color.FromArgb("#00A86B");
        }

        void PintarEsfera()
        {
            Esfera.TranslationX = RadioArena + _px - RadioEsfera;
            Esfera.TranslationY = RadioArena + _py - RadioEsfera;
        }

        void OnReiniciar(object? sender, EventArgs e)
        {
            _px = _py = _vx = _vy = 0;
            _racha = _mejor = 0;
            SimX.Value = SimY.Value = 0;
        }
    }
}
