using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinariaGestion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    idCliente = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cuildni = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    apellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    direccion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    fechaAlta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.idCliente);
                });

            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    idEspecie = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.idEspecie);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    idProveedor = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    razonSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    cuilCuit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    fechaAlta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.idProveedor);
                });

            migrationBuilder.CreateTable(
                name: "Tipo",
                columns: table => new
                {
                    idTipo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipo", x => x.idTipo);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    idUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombreUsuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    passwordHash = table.Column<string>(type: "TEXT", nullable: true),
                    fechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    activo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.idUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    idVenta = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    numeroVenta = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    fecha = table.Column<DateTime>(type: "TEXT", nullable: true),
                    id_cliente = table.Column<int>(type: "INTEGER", nullable: true),
                    subTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    descuento = table.Column<decimal>(type: "TEXT", nullable: false),
                    recargo = table.Column<decimal>(type: "TEXT", nullable: false),
                    total = table.Column<decimal>(type: "TEXT", nullable: false),
                    formaPago = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    estadoPago = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.idVenta);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    idMascota = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    id_cliente = table.Column<int>(type: "INTEGER", nullable: false),
                    id_especie = table.Column<int>(type: "INTEGER", nullable: false),
                    sexo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    fechaNaci = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascotas", x => x.idMascota);
                    table.ForeignKey(
                        name: "FK_Mascotas_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mascotas_Especies_id_especie",
                        column: x => x.id_especie,
                        principalTable: "Especies",
                        principalColumn: "idEspecie",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    idProducto = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    stock = table.Column<int>(type: "INTEGER", nullable: false),
                    precioMinorista = table.Column<decimal>(type: "TEXT", nullable: false),
                    precioMayorista = table.Column<decimal>(type: "TEXT", nullable: false),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    id_tipo = table.Column<int>(type: "INTEGER", nullable: true),
                    id_proveedor = table.Column<int>(type: "INTEGER", nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.idProducto);
                    table.ForeignKey(
                        name: "FK_Productos_Proveedores_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "Proveedores",
                        principalColumn: "idProveedor",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Productos_Tipo_id_tipo",
                        column: x => x.id_tipo,
                        principalTable: "Tipo",
                        principalColumn: "idTipo",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CuentasCorrientes",
                columns: table => new
                {
                    idCuentaCorriente = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_cliente = table.Column<int>(type: "INTEGER", nullable: false),
                    id_venta = table.Column<int>(type: "INTEGER", nullable: true),
                    fechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    tipo_movimiento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    concepto = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    importe = table.Column<decimal>(type: "TEXT", nullable: false),
                    saldoAnterior = table.Column<decimal>(type: "TEXT", nullable: false),
                    saldoNuevo = table.Column<decimal>(type: "TEXT", nullable: false),
                    fechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estadoCuenta = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    comprobante = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasCorrientes", x => x.idCuentaCorriente);
                    table.ForeignKey(
                        name: "FK_CuentasCorrientes_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasCorrientes_Ventas_id_venta",
                        column: x => x.id_venta,
                        principalTable: "Ventas",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Cuotas",
                columns: table => new
                {
                    idCuota = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    numeroCuota = table.Column<int>(type: "INTEGER", nullable: false),
                    fechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    fechaPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    montoCuota = table.Column<decimal>(type: "TEXT", nullable: false),
                    montoPagado = table.Column<decimal>(type: "TEXT", nullable: false),
                    saldoPendiente = table.Column<decimal>(type: "TEXT", nullable: false),
                    id_venta = table.Column<int>(type: "INTEGER", nullable: false),
                    interesMora = table.Column<decimal>(type: "TEXT", nullable: false),
                    estadoCuota = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuotas", x => x.idCuota);
                    table.ForeignKey(
                        name: "FK_Cuotas_Ventas_id_venta",
                        column: x => x.id_venta,
                        principalTable: "Ventas",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    idTurno = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fecha = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estadoTurno = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    horaTurno = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    id_mascota = table.Column<int>(type: "INTEGER", nullable: false),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.idTurno);
                    table.ForeignKey(
                        name: "FK_Turnos_Mascotas_id_mascota",
                        column: x => x.id_mascota,
                        principalTable: "Mascotas",
                        principalColumn: "idMascota",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas_detalle",
                columns: table => new
                {
                    idVentaDetalle = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_venta = table.Column<int>(type: "INTEGER", nullable: false),
                    id_producto = table.Column<int>(type: "INTEGER", nullable: false),
                    cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    descuentoItem = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    subTotalItem = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas_detalle", x => x.idVentaDetalle);
                    table.ForeignKey(
                        name: "FK_Ventas_detalle_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_detalle_Ventas_id_venta",
                        column: x => x.id_venta,
                        principalTable: "Ventas",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Historial",
                columns: table => new
                {
                    idHistorial = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_mascota = table.Column<int>(type: "INTEGER", nullable: false),
                    id_turno = table.Column<int>(type: "INTEGER", nullable: true),
                    motivoConsulta = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    diagnostico = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    tratamiento = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    indicacion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    fechaConsulta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    proximoControl = table.Column<DateTime>(type: "TEXT", nullable: true),
                    estado = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historial", x => x.idHistorial);
                    table.ForeignKey(
                        name: "FK_Historial_Mascotas_id_mascota",
                        column: x => x.id_mascota,
                        principalTable: "Mascotas",
                        principalColumn: "idMascota",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Historial_Turnos_id_turno",
                        column: x => x.id_turno,
                        principalTable: "Turnos",
                        principalColumn: "idTurno",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasCorrientes_id_cliente",
                table: "CuentasCorrientes",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasCorrientes_id_venta",
                table: "CuentasCorrientes",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "IX_Cuotas_id_venta",
                table: "Cuotas",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "IX_Historial_id_mascota",
                table: "Historial",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "IX_Historial_id_turno",
                table: "Historial",
                column: "id_turno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_id_cliente",
                table: "Mascotas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_id_especie",
                table: "Mascotas",
                column: "id_especie");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_proveedor",
                table: "Productos",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_tipo",
                table: "Productos",
                column: "id_tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_cuilCuit",
                table: "Proveedores",
                column: "cuilCuit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_id_mascota",
                table: "Turnos",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_email",
                table: "Usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_nombreUsuario",
                table: "Usuarios",
                column: "nombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_id_cliente",
                table: "Ventas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_numeroVenta",
                table: "Ventas",
                column: "numeroVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_detalle_id_producto",
                table: "Ventas_detalle",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_detalle_id_venta",
                table: "Ventas_detalle",
                column: "id_venta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentasCorrientes");

            migrationBuilder.DropTable(
                name: "Cuotas");

            migrationBuilder.DropTable(
                name: "Historial");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Ventas_detalle");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Mascotas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Tipo");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Especies");
        }
    }
}
