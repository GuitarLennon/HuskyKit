
using HuskyKit.Datatables;
using HuskyKit.Sql;
using HuskyKit.Sql.Columns;
using HuskyKit.Sql.Sources;


string test1()
{
    return SqlBuilder
        .Select("Hola"
            , "COUNT(Mundo)".As("Grupo", true)
            , "COUNT(Mundo)".As("Grupo 2", true, OrderDirection.ASC)
            , "Grupo 3".OrderBy(2)
        )
        .From("Tabla")
        .Where("Tabla = 1")
        .Where("Tabla = 2")
        .Offset(1)
        .Top(2)
        .Build();
}


string test2()
{
    string TablaIndicadores = "Indicadores";
    var Indicadores = ("IMSS", TablaIndicadores);
    var Geography = ("IMSS", "OOAD", "Geography");
    string? claveIndicador = "CVE_PRODSEM_MF_DIFT,CVE_PRODSEM_IQX_DIFT";
    string? claves = null;
    string? tipo = "Nacional";
    string? periodicidad = "Semanal,Semanal acumulado";
    DateTime? fechaInicio = null;
    DateTime? fechaTermino = null;
    int? topClaves = null;
    int? topFilas = null;
    bool includeMap = false;

    var claveIndicadores = claveIndicador.Split(',');

    var indicadores = SqlBuilder
        .Select(
            "Clave indicador",
            "Clave",
            "Fila",
            "Periodicidad",
            "Fecha",
            "Tipo",
            "[Indicador crudo]".As("Indicador"),
            "[Numerador crudo]".As("Numerador"),
            "[Denominador crudo]".As("Denominador"),
            "Desempeño")
        .From(Indicadores)
        .Top(topClaves)
        .Where(x => x["Clave indicador"], claveIndicadores);

    SqlQueryColumn column(string name) => SqlBuilder
      .Select(
          "Fecha".As("Fecha", order: OrderDirection.DESC),
          "Periodicidad",
          "Indicador",
          "Numerador",
          "Denominador",
          "Desempeño"
      )
      .From(indicadores.Alias)
      .Top(topFilas)
      .Where($"Indicadores.Clave = Scope.Clave")
      .Where($"Indicadores.Tipo = Scope.Tipo")
      .Where($"Indicadores.Indicador IS NOT NULL")
      .Where($"Indicadores.[Clave indicador] = {name}")
      .AsColumn($"Indicadores.{name}", ForJsonOptions.PATH);


    var scope = SqlBuilder.With(indicadores)
        .Select("Clave", "Tipo", "Nombre")
        .Select(claveIndicadores.Select(column).ToArray())
        .SelectIf(includeMap, "GeoJson")
        .From(Geography);


    if (string.IsNullOrWhiteSpace(tipo))
        scope.Where($"[Tipo] = 'Nacional'");
    else
        scope.Where($"[Tipo] IN ({tipo})");


    if (!string.IsNullOrWhiteSpace(claves))
        indicadores.Where($"[Clave]           IN ({claves})");

    return scope.Build();
}

string test3()
{
    return SqlBuilder.Select(
                "PK"
                , "ID"
                , "Cantidad"
                , "Dosis unitaria"
                , "Unidades dosis"
                , "FormaFarmacéutica"
                , "COMPRA2025"
            )
            .From("catMedicamentoFármaco")
            .Build();
}

string test4()
{
    return SqlBuilder.Select(
                "PK"
                , "ID"
                , "Cantidad"
                , "Dosis unitaria"
                , "Unidades dosis"
                , "FormaFarmacéutica"
                , "COMPRA2025"
            )
            .From("catMedicamentoFármaco")
            .Join(JoinTypes.INNER, "CatMedicamento"
                , x => x["PK"]
                , "PK"
                , "desc_art"
                , "COMPRA2025")
            .Build();
}

string test5()
{
    return SqlBuilder.Select(
                "PK"
                , "ID"
                , "Cantidad"
                , "Dosis unitaria"
                , "Unidades dosis"
                , "FormaFarmacéutica"
                , "COMPRA2025"
            )
            .From("catMedicamentoFármaco")
            .Join(JoinTypes.INNER, "CatMedicamento"
                , x => x["PK"]
                , "PK"
                , "desc_art"
                , "COMPRA2025")
            .BuildForJson(ForJsonOptions.PATH);
}

string test6()
{
    DTRequest dTRequest = new();

    var claveIndicadores = "CVE_PRODSEM_MF";

    var indicadores = SqlBuilder
        .Select(
            "Clave indicador",
            "Clave",
            "Periodicidad",
            "Fecha",
            "Tipo",
            "[Indicador crudo]".As("Indicador"),
            "[Numerador crudo]".As("Numerador"),
            "[Denominador crudo]".As("Denominador"),
            "Desempeño")
        .From(("dbo", "Indicadores"))
        .Where(x => x["Clave indicador"], new[] { "CVE_PRODSEM_MF" });

    var column = (string Name) =>
    {
        var builder = SqlBuilder
            .Select(
                "Fecha".As("Fecha", order: OrderDirection.DESC),
                "Periodicidad",
                "Indicador",
                "Numerador",
                "Denominador",
                "Desempeño"
            )
            .From(indicadores)
            .Top(2)
            .WhereIsNotNull(x => x["Indicador"])
            .Where(x => x["Clave indicador"], Name);

        return new
        {
            Name,
            SortName = $"Sort.{Name}",
            JsonName = $"Indicadores.{Name}",
            json = builder.AsColumn($"Indicadores.{Name}", ForJsonOptions.PATH)
                .JoinEnvolvingTable(x => [x["Clave"], x["Tipo"]], x => [x["Clave"], x["Tipo"]]),
            order = builder.AsValueColumn("Indicador", $"Sort.{Name}")
                .JoinEnvolvingTable(x => [x["Clave"], x["Tipo"]], x => [x["Clave"], x["Tipo"]]),
        };
    };

    var cols = new[] { "CVE_PRODSEM_MF" }.Select(column).ToArray();

    var scope = SqlBuilder.With(indicadores)
        .Select("Clave", "Tipo", "Nombre")
        .Select([.. cols.Select(x => x.json)])
        .From("dbo", "Geography")
        .Join(JoinTypes.LEFT, ("dbo", "GeographyHierarchy"),
            x => [x["Tipo"], x["Clave"]],
            "Padre_Tipo".As("Padre_Tipo"), "Padre_Clave".As("Padre_Clave")
        );

    if (dTRequest is not null && dTRequest.Columns.Length > 0)
    {
        var dtr = scope.ApplyDTRequest(dTRequest, true); //Not default order


        List<OrderByClause> orderByClauses = [];

        foreach (var order in dTRequest.Order)
        {
            if (!string.IsNullOrWhiteSpace(order.ColumnName))
            {
                var col = cols.FirstOrDefault(x => x.JsonName == order.ColumnName);

                if (col != null)
                {
                    dtr.UnfilteredData.Select(col.order);
                    dtr.FilteredData.Select(col.SortName);
                    orderByClauses.Add(new OrderByClause($"CASE WHEN [{col.SortName}] IS NULL THEN 1 ELSE 0 END"));
                    orderByClauses.Add(new OrderByClause($"[{col.SortName}]", order.Direction));
                }
            }
        }

        if (orderByClauses.Count > 0)
            dtr.Data.SqlBuilder.OrderBy(orderByClauses.ToArray());

        scope = dtr.DTResponse;
    }

    return scope.ToString();
}

void test(Func<string> method)
{
    Console.WriteLine("-------------");
    Console.WriteLine("-------------");
    Console.WriteLine("-------------");
    Console.WriteLine(method());
}

void testting(params Func<string>[] methods)
{
    foreach (var item in methods)
        test(item);
}




testting(test6, test5, test1, test2, test3, test4);



//using System.Diagnostics.Contracts;

//const string TablaContratos = "Contratos";
//const string TablaProveedores = "Proveedores";
//const string TablaMedicamentosContrato = "Medicamentos_contratos";
//(string schema, string table) Contrato = ("dbo", TablaContratos);
//(string schema, string table) Medicamentos = ("dbo", TablaMedicamentosContrato);
//(string schema, string table) Proveedores = ("dbo", TablaProveedores);

//DTRequest dTRequest = new()
//{
//    Columns = [new() { Name = "ClasPtalOrigen", Data = "ClasPtalOrigen" }],
//    Draw = 1,
//    Start = 0,
//    Length = 50,
//    Order = [new() { Column = 0, Dir = "Desc" }],
//};



//var sqlbuilder = SqlBuilder
//         .Select(
//             "[{0}].[CLAS PTAL ORIGEN]".As("ClasPtalOrigen"),
//             "[{0}].[NUMERO CONTRATO]".As("NumeroContrato"),
//             "[{0}].[NOMBRE OOAD]".As("NombreOoad"),
//             "[{0}].[FECHA ACTUALIZACION]".As("FechaActualizacion"),
//             "[{0}].[NUMERO DICTAMEN DEFINITIVO]".As("NumeroDictamenDefinitivo"),
//             "[{0}].[MONTO MAXIMO CONTRATO CON IVA]".As("MontoMaximoContratoConIva"),
//             "[{0}].[MONTO MINIMO CONTRATO CON IVA]".As("MontoMinimoContratoConIva"),
//             "[{0}].[NUMERO LICITACION]".As("NumeroLicitacion"),
//             "[{0}].[NUMERO EVENTO COMPRANET]".As("NumeroEventoCompranet"),
//             "[{0}].[PORCENTAJE SANCION CONTRATO]".As("PorcentajeSancionContrato"),
//             "[{0}].[DIAS DE ENTREGA CON SANCION]".As("DiasDeEntregaConSancion"),
//             "[{0}].[NUMERO PROVEEDOR]".As("NumeroProveedor"),
//             "[{0}].[FECHA INICIO]".As("FechaInicio"),
//             "[{0}].[FECHA TERMINACION]".As("FechaTerminacion"),
//             "[{0}].[FECHA DICTAMEN]".As("FechaDictamen"),
//             "[{0}].[TIPO CONTRATO]".As("TipoContrato"),
//             "[{0}].[ESTADO CONTRATO]".As("EstadoContrato"),
//             "[{0}].[CUENTA CONTABLE]".As("CuentaContable"),
//             "[{0}].[SALDO DISPONIBLE DICTAMEN PREI]".As("SaldoDisponibleDictamenPrei"),
//             "[{0}].[MONTO EJERCIDO DICTAMEN SAI]".As("MontoEjercidoDictamenSai"),
//             "[{0}].[MONTO PAGADO]".As("MontoPagado"),
//             "[{0}].[IVA]".As("Iva"),
//             SqlBuilder.Select(
//                 "[PK]".As("Pk"),
//                 "[ESTATUS CLAVE]".As("EstatusClave"),
//                 "[MONTO MAXIMO CLAVE CON IVA]".As("MontoMaximoClaveConIva"),
//                 "[MONTO MINIMO CLAVE CON IVA]".As("MontoMinimoClaveConIva"),
//                 "[DESCRIPCION ARTICULO]".As("DescripcionArticulo"),
//                 "[UNIDAD PRESENTACION]".As("UnidadPresentacion"),
//                 "[CANTIDAD PRESENTACION]".As("CantidadPresentacion"),
//                 "[TIPO PRESENTACION]".As("TipoPresentacion"),
//                 "[PARTIDA PRESUPUESTAL]".As("PartidaPresupuestal"),
//                 "[DESC PARTIDA PRESUPUESTAL]".As("DescPartidaPresupuestal"),
//                 "[CUADRO BASICO SAI]".As("CuadroBasicoSai"),
//                 "[PRECIO NETO CONTRATO]".As("PrecioNetoContrato"),
//                 "[CANTIDAD MAXIMA CLAVE]".As("CantidadMaximaClave"),
//                 "[CANTIDAD CONTRATACION ORIGINAL]".As("CantidadContratacionOriginal"),
//                 "[CANTIDAD MINIMA CLAVE]".As("CantidadMinimaClave"),
//                 "[CANTIDAD EJERCIDA O SOLICITADA]".As("CantidadEjercidaOSolicitada"),
//                 "[CANT SOLIC VIGENTE EN TRANSITO]".As("CantSolicVigenteEnTransito"),
//                 "[CANTIDAD DISPONIBLE]".As("CantidadDisponible"),
//                 "[CANTIDAD ATENDIDA]".As("CantidadAtendida"),
//                 "[CANTIDAD DE PIEZAS TOPADAS]".As("CantidadDePiezasTopadas"),
//                 "[PORCENTAJE EJERCIDO]".As("PorcentajeEjercido"),
//                 "[PORCENTAJE TOPADO]".As("PorcentajeTopado"),
//                 "[PORCEN ATENCION SIN TRANSITO]".As("PorcenAtencionSinTransito"),
//                 "[UNIDAD PRESENTACION] + ' con ' + CONVERT(VARCHAR, [CANTIDAD PRESENTACION]) + ' ' + [TIPO PRESENTACION]".As("Presentacion")
//             )
//            .Where($"[{TablaMedicamentosContrato}].[CLAS PTAL ORIGEN] = [{TablaContratos}].[CLAS PTAL ORIGEN]")
//            .Where($"[{TablaMedicamentosContrato}].[NUMERO CONTRATO] = [{TablaContratos}].[NUMERO CONTRATO]")
//            .AsColumn("Medicamentos", forJson: ForJsonOptions.PATH)
//         )
//         .From(Contrato)
//         .Join(JoinTypes.INNER, Proveedores, $"{TablaContratos}.[NUMERO PROVEEDOR] = {TablaProveedores}.[NUMERO PROVEEDOR]",
//             "COALESCE([{0}].[Nombre amistoso], TRIM([{0}].[RAZON SOCIAL]))".As("Proveedor.RazonSocial"),
//             "[{0}].[Numero proveedor]".As("Proveedor.Id")
//         )
//         .Join(JoinTypes.INNER, Medicamentos,
//             $"{TablaContratos}.[CLAS PTAL ORIGEN] = {TablaMedicamentosContrato}.[CLAS PTAL ORIGEN] AND {TablaContratos}.[NUMERO CONTRATO] = {TablaMedicamentosContrato}.[NUMERO CONTRATO]"
//         ); ;

//var sqlbuilderAltered = sqlbuilder
//    .ApplyDTRequest(dTRequest);


//var sql = sqlbuilder.Build();
//var sql2 = sqlbuilderAltered.DTResponse.Build();


//Console.WriteLine("SQL BUILDER 1");

//Console.WriteLine(sql);

//Console.WriteLine("------------------------");
//Console.WriteLine("------------------------");
//Console.WriteLine("------------------------");
//Console.WriteLine("SQL BUILDER 2");

//Console.WriteLine(sql2);