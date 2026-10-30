using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.Provider;
using Java.Lang;
using System.Text;

namespace sensores
{
    public static partial class NfcReader
    {
        static readonly LectorCallback _callback = new();

        static NfcAdapter? Adaptador => NfcAdapter.GetDefaultAdapter(Microsoft.Maui.ApplicationModel.Platform.AppContext);

        public static partial bool EstaDisponible() => Adaptador is not null;

        public static partial bool EstaActivado() => Adaptador?.IsEnabled == true;

        public static partial void Iniciar()
        {
            var actividad = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (actividad is null || !EstaActivado())
                return;

            // Modo lector: la app recibe las etiquetas mientras está en primer plano.
            Adaptador!.EnableReaderMode(
                actividad,
                _callback,
                NfcReaderFlags.NfcA | NfcReaderFlags.NfcB | NfcReaderFlags.NfcF | NfcReaderFlags.NfcV,
                null);
        }

        public static partial void Detener()
        {
            var actividad = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (actividad is not null)
                Adaptador?.DisableReaderMode(actividad);
        }

        public static partial void AbrirAjustes()
        {
            var intent = new Intent(Settings.ActionNfcSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Microsoft.Maui.ApplicationModel.Platform.AppContext.StartActivity(intent);
        }

        sealed class LectorCallback : Java.Lang.Object, NfcAdapter.IReaderCallback
        {
            // Se llama desde un hilo del sistema, no desde el hilo de la UI.
            public void OnTagDiscovered(Tag? tag)
            {
                if (tag is null)
                    return;

                string uid = BitConverter.ToString(tag.GetId() ?? []);
                string[] tecnologias = (tag.GetTechList() ?? [])
                    .Select(t => t.Replace("android.nfc.tech.", ""))
                    .ToArray();

                Notificar(new NfcTagInfo(uid, tecnologias, LeerRegistros(tag)));
            }

            static string[] LeerRegistros(Tag tag)
            {
                var resultado = new List<string>();
                var ndef = Ndef.Get(tag);
                if (ndef is null)
                    return [];

                try
                {
                    ndef.Connect();
                    var mensaje = ndef.NdefMessage ?? ndef.CachedNdefMessage;
                    foreach (var registro in mensaje?.GetRecords() ?? [])
                        resultado.Add(Describir(registro));
                }
                catch (Java.Lang.Exception ex)
                {
                    resultado.Add($"No se pudo leer NDEF: {ex.Message}");
                }
                finally
                {
                    try { ndef.Close(); } catch { }
                }

                return resultado.ToArray();
            }

            static string Describir(NdefRecord registro)
            {
                byte[] tipo = registro.GetTypeInfo() ?? [];
                byte[] datos = registro.GetPayload() ?? [];

                // Texto (RTD_TEXT): byte de estado + código de idioma + texto
                if (registro.Tnf == NdefRecord.TnfWellKnown && tipo.Length == 1 && tipo[0] == (byte)'T' && datos.Length > 0)
                {
                    int longitudIdioma = datos[0] & 0x3F;
                    var codificacion = (datos[0] & 0x80) == 0 ? Encoding.UTF8 : Encoding.BigEndianUnicode;
                    return "Texto: " + codificacion.GetString(datos, 1 + longitudIdioma, datos.Length - 1 - longitudIdioma);
                }

                // URI (RTD_URI)
                if (registro.Tnf == NdefRecord.TnfWellKnown && tipo.Length == 1 && tipo[0] == (byte)'U')
                    return "URI: " + registro.ToUri();

                if (registro.Tnf == NdefRecord.TnfMimeMedia)
                    return $"MIME {Encoding.ASCII.GetString(tipo)}: {datos.Length} bytes";

                return $"Registro (TNF {registro.Tnf}): {datos.Length} bytes";
            }
        }
    }
}
