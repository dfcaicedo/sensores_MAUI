namespace sensores.Textos
{
    /// <summary>Contenido de una página de explicación (se dibuja con <see cref="Pages.ExplicacionPage"/>).</summary>
    public record ExplicacionInfo(
        string Emoji,
        string Titulo,
        string Resumen,
        string[] Pasos,
        string[] Permisos,
        string Codigo,
        string Nota);
}
