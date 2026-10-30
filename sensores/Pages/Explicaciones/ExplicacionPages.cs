using sensores.Textos;

namespace sensores.Pages.Explicaciones
{
    // Una clase por sensor para poder usarlas en AppShell.xaml con DataTemplate (requiere constructor sin parámetros).
    public class CamaraExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Camara);
    public class GiroscopioExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Giroscopio);
    public class GpsExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Gps);
    public class GpsCamaraExplicacionPage() : ExplicacionPage(Textos.Explicaciones.GpsCamara);
    public class ArchivosExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Archivos);
    public class NfcExplicacionPage() : ExplicacionPage(Textos.Explicaciones.Nfc);
}
