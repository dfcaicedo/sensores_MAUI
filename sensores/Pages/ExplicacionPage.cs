using sensores.Textos;

namespace sensores.Pages
{
    /// <summary>Página genérica de explicación: se construye en código a partir de un <see cref="ExplicacionInfo"/>.</summary>
    public class ExplicacionPage : ContentPage
    {
        static readonly Color BordeClaro = Color.FromArgb("#C8C8C8");
        static readonly Color BordeOscuro = Color.FromArgb("#404040");

        public ExplicacionPage(ExplicacionInfo info)
        {
            Title = "Explicación";

            var pila = new VerticalStackLayout { Padding = 20, Spacing = 10 };

            pila.Add(new Label
            {
                Text = $"{info.Emoji}  {info.Titulo}",
                FontSize = 26,
                FontAttributes = FontAttributes.Bold
            });
            pila.Add(new Label { Text = info.Resumen, FontSize = 15, LineHeight = 1.2 });

            pila.Add(Encabezado("Cómo se usa"));
            for (int i = 0; i < info.Pasos.Length; i++)
                pila.Add(new Label { Text = $"{i + 1}.  {info.Pasos[i]}", LineHeight = 1.2 });

            pila.Add(Encabezado("Permisos y configuración"));
            foreach (var permiso in info.Permisos)
                pila.Add(new Label { Text = $"•  {permiso}", LineHeight = 1.2 });

            pila.Add(Encabezado("Código clave"));
            pila.Add(CajaDeCodigo(info.Codigo));

            pila.Add(Encabezado("Para tener en cuenta"));
            pila.Add(new Border
            {
                Padding = 12,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Color.FromArgb("#33512BD4"),
                Content = new Label { Text = info.Nota, LineHeight = 1.2 }
            });

            Content = new ScrollView { Content = pila };
        }

        static Label Encabezado(string texto)
        {
            var label = new Label
            {
                Text = texto,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 12, 0, 0)
            };
            label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#512BD4"), Color.FromArgb("#AC99EA"));
            return label;
        }

        static Border CajaDeCodigo(string codigo)
        {
            string fuente = DeviceInfo.Platform == DevicePlatform.Android ? "monospace"
                : DeviceInfo.Platform == DevicePlatform.WinUI ? "Consolas"
                : "Menlo";

            var texto = new Label { Text = codigo, FontFamily = fuente, FontSize = 12 };
            texto.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#212121"), Color.FromArgb("#E1E1E1"));

            var caja = new Border
            {
                Padding = 12,
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Content = new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = texto }
            };
            caja.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#F2F2F2"), Color.FromArgb("#222228"));
            caja.SetAppThemeColor(Border.StrokeProperty, BordeClaro, BordeOscuro);
            return caja;
        }
    }
}
