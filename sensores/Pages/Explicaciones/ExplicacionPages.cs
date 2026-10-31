using sensores.Textos;

namespace sensores.Pages.Explicaciones
{
    // Una clase por sensor para poder usarlas en AppShell.xaml con DataTemplate (requiere constructor sin parámetros).
    public class CamaraExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Camara, "camara_demo");
    public class GiroscopioExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Giroscopio, "giroscopio_demo");
    public class GpsExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Gps, "gps_demo");
    public class GpsCamaraExplicacionPage() : ExplicacionPage(Textos.Explicaciones.GpsCamara, "gpscamara_demo");
    public class ArchivosExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Archivos, "archivos_demo");
    public class NfcExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Nfc, "nfc_demo");
}
