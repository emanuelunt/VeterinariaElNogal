namespace VeterinariaGestion.Web.Models;

/// <summary>
/// Lista paginada genérica.
/// Soporta paginación en memoria y directamente en base de datos.
/// </summary>
public class PaginatedList<T> : List<T>
{
    public int PageIndex  { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }
    public int PageSize   { get; private set; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage     => PageIndex < TotalPages;

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        TotalCount = count;
        PageSize   = pageSize;
        PageIndex  = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    /// <summary>
    /// Paginación EN MEMORIA — usar solo con listas pequeñas ya cargadas.
    /// </summary>
    public static PaginatedList<T> Create(
        IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var list  = source.ToList();
        var count = list.Count;
        var items = list
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    /// <summary>
    /// Paginación EN BASE DE DATOS — eficiente con millones de registros.
    /// Ejecuta COUNT y SELECT con OFFSET/LIMIT directamente en SQL.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateFromQueryAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        // COUNT en BD (no trae datos)
        var count = await Microsoft.EntityFrameworkCore
                                   .EntityFrameworkQueryableExtensions
                                   .CountAsync(source);

        // SELECT con OFFSET/LIMIT en BD
        var items = await Microsoft.EntityFrameworkCore
                                   .EntityFrameworkQueryableExtensions
                                   .ToListAsync(
                                       source
                                           .Skip((pageIndex - 1) * pageSize)
                                           .Take(pageSize));

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}