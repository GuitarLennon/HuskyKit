# HuskyKit.Sql

**HuskyKit.Sql** es una biblioteca para construir consultas SQL de manera dinámica y fluida. Ofrece una API expresiva que soporta funciones avanzadas como funciones de ventana, agregaciones, y generación de expresiones SQL dinámicas.

## **Características principales**

- Construcción fluida de consultas SQL con soporte para:
  - Cláusulas `SELECT`, `FROM`, `WHERE`, `GROUP BY`, `ORDER BY` y más.
  - Funciones de ventana como `ROW_NUMBER`, `RANK`, `DENSE_RANK`, y `SUM`.
  - Agregaciones comunes como `COUNT`, `SUM`, `AVG`, `MIN`, `MAX`.
  - Generación de objetos JSON directamente en SQL con `JsonObject`.
- Soporte para alias, particiones y ordenación.
- Totalmente compatible con consultas complejas y personalizables.

---

## **Instalación**

Agrega la biblioteca a tu proyecto:

```bash
# Si está disponible como paquete NuGet
Install-Package HuskyKit.Sql
```

Si trabajas con el código fuente, asegúrate de incluirlo en tu solución y referenciarlo en tu proyecto.

---

## **Uso básico**

### Crear una consulta básica

```csharp
using HuskyKit.Sql;

var builder = SqlBuilder.Select(
    "Column1",
    "Column2",
    "Column3"
).From("dbo", "MyTable");

var sql = builder.Build();
Console.WriteLine(sql);

// Salida:
// SELECT [Column1], [Column2], [Column3]
// FROM [dbo].[MyTable]
```

---

## **Funciones avanzadas**

### Funciones de ventana

Usa funciones de ventana como `ROW_NUMBER` y `SUM` para cálculos avanzados:

```csharp
var builder = SqlBuilder.Select(
    "Region",
    Funciones.RowNumber()
        .PartitionBy("Region")
        .OrderBy("Sales", ("Date", "DESC"))
        .As("RowNum"),
    Funciones.SumWindow("Sales")
        .PartitionBy("Region")
        .OrderBy("Date")
        .As("TotalSales")
)
.From("dbo", "SalesData");

var sql = builder.Build();
Console.WriteLine(sql);

// Salida:
// SELECT 
//     [Region],
//     ROW_NUMBER() OVER (PARTITION BY [Region] ORDER BY [Sales], [Date] DESC) AS RowNum,
//     SUM([Sales]) OVER (PARTITION BY [Region] ORDER BY [Date]) AS TotalSales
// FROM [dbo].[SalesData]
```

### Agregaciones comunes

```csharp
var builder = SqlBuilder.Select(
    "Product",
    "Price".Avg().As("AveragePrice"),
    "Sales".Sum().As("TotalSales")
).From("dbo", "Products");

var sql = builder.Build();
Console.WriteLine(sql);

// Salida:
// SELECT 
//     [Product],
//     AVG([Price]) AS AveragePrice,
//     SUM([Sales]) AS TotalSales
// FROM [dbo].[Products]
```

---

## **Funciones personalizadas**

### Generación de JSON

```csharp
var jsonColumn = Funciones.JsonObject(
    new Dictionary<string, string> {
        { "id", "ProductId" },
        { "name", "ProductName" }
    },
    "ProductJson"
);

var builder = SqlBuilder.Select(
    jsonColumn
).From("dbo", "Products");

var sql = builder.Build();
Console.WriteLine(sql);

// Salida:
// SELECT 
//     '{"id": ' + COALESCE(CONVERT(varchar(max), [ProductId]), 'null') + ', "name": ' + COALESCE(CONVERT(varchar(max), [ProductName]), 'null') AS ProductJson
// FROM [dbo].[Products]
```

---

## **Extensibilidad**

### Crear tus propias funciones

Puedes extender la biblioteca agregando métodos personalizados. Por ejemplo, para una función `CONCAT`:

```csharp
public static SqlColumn Concat(this IEnumerable<string> columns, string alias)
{
    var expression = string.Join(" + ", columns.Select(col => $"[{col}]").ToArray());
    return $"CONCAT({expression})".As(alias, false);
}
```

Usando la función personalizada:

```csharp
var builder = SqlBuilder.Select(
    new[] { "FirstName", "LastName" }.Concat("FullName")
).From("dbo", "Users");

var sql = builder.Build();
Console.WriteLine(sql);

// Salida:
// SELECT CONCAT([FirstName] + [LastName]) AS FullName
// FROM [dbo].[Users]
```

---

## **Contribuciones**

Si tienes ideas, mejoras o encuentras algún problema, ¡no dudes en colaborar! Puedes:

- Crear un **issue** en el repositorio.
- Enviar un **pull request** con tus cambios.

---

## **Licencia**

Este proyecto está licenciado bajo la [MIT License](LICENSE).
