namespace sensores
{
    /// <summary>Datos de una etiqueta NFC leída.</summary>
    public record NfcTagInfo(string Uid, string[] Tecnologias, string[] Registros);

    /// <summary>
    /// MAUI no tiene API de NFC, así que la definición es común y cada plataforma aporta su implementación
    /// (ver Platforms/Android/NfcReader.cs). En las demás plataformas NFC aparece como no disponible.
    /// </summary>
    public static partial class NfcReader
    {
        public static event Action<NfcTagInfo>? TagLeido;

        static void Notificar(NfcTagInfo info) => TagLeido?.Invoke(info);

        public static partial bool EstaDisponible();
        public static partial bool EstaActivado();
        public static partial void Iniciar();
        public static partial void Detener();
        public static partial void AbrirAjustes();

#if !ANDROID
        public static partial bool EstaDisponible() => false;
        public static partial bool EstaActivado() => false;
        public static partial void Iniciar() { }
        public static partial void Detener() { }
        public static partial void AbrirAjustes() { }
#endif
    }
}
