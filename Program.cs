
//using HuskyKit.Datatables;
//using HuskyKit.Sql;
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
//    Order = [new() { Column = "ClasPtalOrigen", Dir = "Desc"}],
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
//var sql2 = sqlbuilderAltered.Build();


//Console.WriteLine("SQL BUILDER 1");

//Console.WriteLine(sql);

//Console.WriteLine("------------------------");
//Console.WriteLine("------------------------");
//Console.WriteLine("------------------------");
//Console.WriteLine("SQL BUILDER 2");

//Console.WriteLine(sql2);