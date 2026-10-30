namespace sensores.Textos
{
    public static class Explicaciones
    {
        public static readonly ExplicacionInfo Camara = new(
            "📷", "Cámara",
            "MAUI no incluye un control de cámara, pero la clase MediaPicker (Microsoft.Maui.Media) abre la app de cámara del sistema, " +
            "toma la foto y te devuelve un FileResult con el archivo. Es la forma más simple y no requiere escribir código nativo.",
            [
                "Verifica que el dispositivo pueda capturar: MediaPicker.Default.IsCaptureSupported.",
                "Llama a CapturePhotoAsync(). El sistema pide el permiso de cámara la primera vez.",
                "Si el usuario cancela, el resultado es null.",
                "Abre el archivo con OpenReadAsync() y cópialo a tu almacenamiento (CacheDirectory o AppDataDirectory).",
                "Muestra la imagen con ImageSource.FromFile(ruta)."
            ],
            [
                "Android: <uses-permission android:name=\"android.permission.CAMERA\" /> y un bloque <queries> con la acción android.media.action.IMAGE_CAPTURE.",
                "iOS: clave NSCameraUsageDescription (y NSPhotoLibraryUsageDescription para la galería) en Info.plist.",
                "Windows: funciona con la cámara web; en apps empaquetadas se declara la capacidad webcam."
            ],
            """
            if (!MediaPicker.Default.IsCaptureSupported)
                return;

            FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();
            if (foto is null)
                return; // el usuario canceló

            string ruta = Path.Combine(FileSystem.CacheDirectory, foto.FileName);
            using Stream origen = await foto.OpenReadAsync();
            using FileStream destino = File.Create(ruta);
            await origen.CopyToAsync(destino);

            Foto.Source = ImageSource.FromFile(ruta);
            """,
            "También existe PickPhotosAsync() para elegir una foto de la galería y CaptureVideoAsync() para grabar video. " +
            "Si necesitas una vista previa en vivo dentro de tu pantalla, usa el paquete CommunityToolkit.Maui.Camera.");

        public static readonly ExplicacionInfo Giroscopio = new(
            "🧭", "Giroscopio y acelerómetro",
            "El giroscopio mide la velocidad angular del dispositivo (cuán rápido gira) en radianes por segundo alrededor de los ejes X, Y y Z. " +
            "No mide la inclinación absoluta: para saber hacia dónde está inclinado el teléfono se usa el acelerómetro, que mide la gravedad en unidades g. " +
            "Por eso la pestaña «Coordenadas» usa el giroscopio y el «Juego» usa el acelerómetro.",
            [
                "Comprueba que el sensor exista: Gyroscope.Default.IsSupported.",
                "Suscríbete al evento ReadingChanged. Llega en un hilo secundario, así que usa MainThread para tocar la interfaz.",
                "Inicia la lectura con Start(SensorSpeed.UI). Velocidades: Default, UI, Game y Fastest.",
                "Lee e.Reading.AngularVelocity, un Vector3 con X, Y y Z.",
                "Llama a Stop() al salir de la página para ahorrar batería."
            ],
            [
                "No requieren permisos en Android ni en iOS (a velocidades altas, Android 12+ puede pedir HIGH_SAMPLING_RATE_SENSORS).",
                "iOS: se recomienda NSMotionUsageDescription en Info.plist.",
                "Los equipos de escritorio normalmente no tienen estos sensores: IsSupported devolverá false."
            ],
            """
            if (Gyroscope.Default.IsSupported)
            {
                Gyroscope.Default.ReadingChanged += (s, e) =>
                {
                    Vector3 w = e.Reading.AngularVelocity; // rad/s
                    MainThread.BeginInvokeOnMainThread(() =>
                        XLabel.Text = w.X.ToString("F3"));
                };
                Gyroscope.Default.Start(SensorSpeed.UI);
            }

            // Para el juego: inclinación = gravedad medida por el acelerómetro (en g)
            Accelerometer.Default.ReadingChanged += (s, e) =>
            {
                Vector3 a = e.Reading.Acceleration;
                // a.X > 0 cuando el borde derecho está más abajo
            };
            Accelerometer.Default.Start(SensorSpeed.Game);
            """,
            "Para integrar la rotación acumulada se multiplica la velocidad angular por el tiempo transcurrido entre lecturas (ángulo += ω · Δt). " +
            "Esa integración acumula error (deriva), por eso las apps reales combinan giroscopio, acelerómetro y magnetómetro.");

        public static readonly ExplicacionInfo Gps = new(
            "📍", "GPS (Geolocalización)",
            "La clase Geolocation (Microsoft.Maui.Devices.Sensors) entrega latitud, longitud, altitud, precisión, velocidad y rumbo. " +
            "Puede pedir una lectura puntual o escuchar cambios de posición en primer plano.",
            [
                "Declara los permisos de ubicación en cada plataforma.",
                "Llama a Geolocation.Default.GetLocationAsync(request). MAUI solicita el permiso automáticamente.",
                "Define GeolocationRequest con la precisión deseada y un tiempo máximo de espera.",
                "Maneja las excepciones: FeatureNotSupported, FeatureNotEnabled (GPS apagado) y Permission.",
                "Para seguimiento continuo usa StartListeningForegroundAsync y el evento LocationChanged."
            ],
            [
                "Android: ACCESS_COARSE_LOCATION y ACCESS_FINE_LOCATION en AndroidManifest.xml.",
                "iOS: NSLocationWhenInUseUsageDescription en Info.plist.",
                "Windows: capacidad «location» en Package.appxmanifest si la app es empaquetada."
            ],
            """
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));

            try
            {
                Location? loc = await Geolocation.Default.GetLocationAsync(request);
                if (loc is not null)
                    Console.WriteLine($"{loc.Latitude}, {loc.Longitude} (±{loc.Accuracy} m)");
            }
            catch (FeatureNotEnabledException) { /* GPS apagado */ }
            catch (PermissionException)        { /* permiso denegado */ }
            """,
            "Mayor precisión significa más consumo de batería y más tiempo de espera. Dentro de edificios el GPS puede tardar o fallar; " +
            "usa GetLastKnownLocationAsync() como respaldo rápido.");

        public static readonly ExplicacionInfo GpsCamara = new(
            "🗺️", "GPS + Cámara",
            "Combinar sensores es simplemente coordinar sus APIs. Aquí se toma una foto y se asocia con la ubicación del momento " +
            "(foto geoetiquetada), como hacen las apps de inspección, inventario o reportes en campo.",
            [
                "Pide el permiso de ubicación antes de abrir la cámara, para que los diálogos no se pisen.",
                "Inicia la búsqueda de ubicación SIN esperarla (guarda el Task).",
                "Abre la cámara con CapturePhotoAsync mientras el GPS trabaja en paralelo.",
                "Al volver, espera el Task de ubicación y une ambos resultados.",
                "Guarda o muestra la foto junto con latitud, longitud y fecha."
            ],
            [
                "Los de la cámara: CAMERA + queries en Android, NSCameraUsageDescription en iOS.",
                "Los del GPS: ACCESS_FINE_LOCATION en Android, NSLocationWhenInUseUsageDescription en iOS."
            ],
            """
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            // El GPS busca mientras el usuario toma la foto
            Task<Location?> tareaUbicacion = Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            FileResult? foto = await MediaPicker.Default.CapturePhotoAsync();
            Location? ubicacion = await tareaUbicacion;

            if (foto is not null && ubicacion is not null)
                Etiqueta.Text = $"{ubicacion.Latitude:F5}, {ubicacion.Longitude:F5}";
            """,
            "Para incrustar las coordenadas dentro del archivo (metadatos EXIF) se necesita una librería de imágenes; " +
            "una alternativa simple es guardar un archivo JSON junto a la foto con la latitud, longitud y fecha.");

        public static readonly ExplicacionInfo Archivos = new(
            "📁", "Archivos",
            "FilePicker abre el selector de archivos del sistema y devuelve un FileResult. " +
            "Además, FileSystem te da carpetas privadas de la app (AppDataDirectory, CacheDirectory) donde puedes leer y escribir sin permisos especiales.",
            [
                "Llama a FilePicker.Default.PickAsync(new PickOptions { ... }).",
                "Filtra tipos con FileTypes: FilePickerFileType.Images, Pdf o uno personalizado por plataforma.",
                "Si el usuario cancela, el resultado es null.",
                "Lee el contenido con OpenReadAsync() (FullPath puede ser vacío en Android).",
                "Para guardar tus propios datos escribe en FileSystem.AppDataDirectory con File.WriteAllTextAsync."
            ],
            [
                "El selector del sistema no necesita permisos: el usuario concede acceso al elegir el archivo.",
                "iOS: para ver archivos de iCloud/Archivos no se requiere configuración adicional.",
                "Leer carpetas arbitrarias del dispositivo sí requiere permisos o el Storage Access Framework."
            ],
            """
            FileResult? archivo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elige un archivo",
                FileTypes = FilePickerFileType.Images
            });

            if (archivo is not null)
            {
                using Stream stream = await archivo.OpenReadAsync();
                Console.WriteLine($"{archivo.FileName} ({archivo.ContentType})");
            }

            // Archivo propio de la app
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "nota.txt");
            await File.WriteAllTextAsync(ruta, "Hola desde MAUI");
            """,
            "PickMultipleAsync permite elegir varios archivos. Para que el usuario guarde un archivo donde quiera, " +
            "usa FileSaver de CommunityToolkit.Maui.Storage.");

        public static readonly ExplicacionInfo Nfc = new(
            "📡", "NFC",
            "A diferencia de los demás sensores, MAUI NO tiene una API multiplataforma para NFC. Se accede a través del código nativo de cada plataforma. " +
            "En este proyecto se usa Android (NfcAdapter en modo lector) con una clase parcial: la definición es común y la implementación vive en Platforms/Android.",
            [
                "Declara el permiso NFC en el manifiesto.",
                "Crea una clase estática parcial en código compartido con los métodos que la UI necesita (Iniciar, Detener, evento TagLeido).",
                "Implementa esa clase en Platforms/Android usando NfcAdapter.EnableReaderMode sobre la actividad actual.",
                "En OnTagDiscovered lee el UID (tag.GetId()) y los mensajes NDEF con Ndef.Get(tag).",
                "Llama a DisableReaderMode al salir de la página. En las demás plataformas devuelve «no disponible»."
            ],
            [
                "Android: <uses-permission android:name=\"android.permission.NFC\" /> y <uses-feature android:name=\"android.hardware.nfc\" android:required=\"false\" />.",
                "iOS: requiere el entitlement Near Field Communication Tag Reading, NFCReaderUsageDescription y un perfil de aprovisionamiento propio; no se implementa en este ejemplo.",
                "El usuario debe tener NFC encendido en los ajustes del teléfono."
            ],
            """
            // Platforms/Android/NfcReader.cs
            var adaptador = NfcAdapter.GetDefaultAdapter(Platform.AppContext);

            adaptador?.EnableReaderMode(
                Platform.CurrentActivity,
                new Callback(),
                NfcReaderFlags.NfcA | NfcReaderFlags.NfcB |
                NfcReaderFlags.NfcF | NfcReaderFlags.NfcV,
                null);

            // Callback : Java.Lang.Object, NfcAdapter.IReaderCallback
            public void OnTagDiscovered(Tag? tag)
            {
                string uid = BitConverter.ToString(tag.GetId());
                Ndef? ndef = Ndef.Get(tag);
                // ndef.Connect(); ndef.NdefMessage.GetRecords(); ndef.Close();
            }
            """,
            "Para probarlo necesitas un teléfono Android con NFC y una etiqueta (por ejemplo NTAG213) o una tarjeta. " +
            "Muchas tarjetas bancarias y de transporte solo exponen el UID y no tienen mensajes NDEF.");
    }
}
