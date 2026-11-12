-- ============================================================================
-- INSTITUTO SUPERIOR TECNOLÓGICO CORDILLERA
-- Aplicaciones Distribuidas - Unidad 1 - Práctica 1 (AD01)
-- Script de creación de bases de datos independientes:
--   1. CatalogoDB
--   2. InventarioDB
--   3. PedidosDB
-- ============================================================================

-- ============================================================================
-- 1. BASE DE DATOS: CatalogoDB (Microservicio Catalogo.Api)
-- ============================================================================
IF DB_ID(N'CatalogoDB') IS NULL
BEGIN
    CREATE DATABASE CatalogoDB;
END
GO

USE CatalogoDB;
GO

IF OBJECT_ID('dbo.Producto', 'U') IS NOT NULL DROP TABLE dbo.Producto;
IF OBJECT_ID('dbo.Categoria', 'U') IS NOT NULL DROP TABLE dbo.Categoria;
IF OBJECT_ID('dbo.Marca', 'U') IS NOT NULL DROP TABLE dbo.Marca;
GO

CREATE TABLE dbo.Categoria (
    IdCategoria INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    CONSTRAINT PK_Categoria PRIMARY KEY (IdCategoria)
);
GO

CREATE TABLE dbo.Marca (
    IdMarca INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    PaisOrigen NVARCHAR(100) NULL,
    CONSTRAINT PK_Marca PRIMARY KEY (IdMarca)
);
GO

CREATE TABLE dbo.Producto (
    IdProducto INT IDENTITY(1,1) NOT NULL,
    IdCategoria INT NOT NULL,
    IdMarca INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SKU NVARCHAR(50) NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Estado BIT NOT NULL CONSTRAINT DF_Producto_Estado DEFAULT (1),
    CONSTRAINT PK_Producto PRIMARY KEY (IdProducto),
    CONSTRAINT UQ_Producto_SKU UNIQUE (SKU),
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria) REFERENCES dbo.Categoria(IdCategoria),
    CONSTRAINT FK_Producto_Marca FOREIGN KEY (IdMarca) REFERENCES dbo.Marca(IdMarca)
);
GO

INSERT INTO dbo.Categoria (Nombre, Descripcion) VALUES
('Componentes PC', 'Procesadores, tarjetas madre, memorias RAM y almacenamiento SSD'),
('Periféricos', 'Teclados mecánicos, ratones ópticos y auriculares'),
('Monitores', 'Pantallas IPS, VA y OLED para oficina y diseño');

INSERT INTO dbo.Marca (Nombre, PaisOrigen) VALUES
('ASUS', 'Taiwán'),
('Corsair', 'Estados Unidos'),
('LG', 'Corea del Sur');

INSERT INTO dbo.Producto (IdCategoria, IdMarca, Nombre, SKU, Precio, Estado) VALUES
(1, 1, 'Tarjeta Madre ASUS ROG Strix B550-F', 'ASUS-B550-ROG', 189.99, 1),
(2, 2, 'Teclado Mecánico Corsair K70 RGB', 'CORS-K70-RGB', 129.50, 1),
(3, 3, 'Monitor LG UltraGear 27 Pulgadas IPS', 'LG-27GN800', 295.00, 1);
GO


-- ============================================================================
-- 2. BASE DE DATOS: InventarioDB (Microservicio Inventario.Api)
-- ============================================================================
IF DB_ID(N'InventarioDB') IS NULL
BEGIN
    CREATE DATABASE InventarioDB;
END
GO

USE InventarioDB;
GO

IF OBJECT_ID('dbo.ReservaStock', 'U') IS NOT NULL DROP TABLE dbo.ReservaStock;
IF OBJECT_ID('dbo.MovimientoInventario', 'U') IS NOT NULL DROP TABLE dbo.MovimientoInventario;
IF OBJECT_ID('dbo.Existencia', 'U') IS NOT NULL DROP TABLE dbo.Existencia;
IF OBJECT_ID('dbo.Bodega', 'U') IS NOT NULL DROP TABLE dbo.Bodega;
GO

CREATE TABLE dbo.Bodega (
    IdBodega INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Ciudad NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_Bodega PRIMARY KEY (IdBodega)
);
GO

CREATE TABLE dbo.Existencia (
    IdExistencia INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL, -- FK Lógica hacia CatalogoDB.dbo.Producto
    IdBodega INT NOT NULL,   -- FK Física hacia InventarioDB.dbo.Bodega
    Stock INT NOT NULL CONSTRAINT DF_Existencia_Stock DEFAULT (0),
    CONSTRAINT PK_Existencia PRIMARY KEY (IdExistencia),
    CONSTRAINT FK_Existencia_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega),
    CONSTRAINT UQ_Existencia_ProductoBodega UNIQUE (IdProducto, IdBodega)
);
GO

CREATE TABLE dbo.MovimientoInventario (
    IdMovimiento INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL, -- FK Lógica hacia CatalogoDB.dbo.Producto
    IdBodega INT NOT NULL,   -- FK Física hacia InventarioDB.dbo.Bodega
    Tipo NVARCHAR(50) NOT NULL, -- ENTRADA, SALIDA, AJUSTE
    Cantidad INT NOT NULL,
    Fecha DATETIME2 NOT NULL CONSTRAINT DF_Movimiento_Fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_MovimientoInventario PRIMARY KEY (IdMovimiento),
    CONSTRAINT FK_MovimientoInventario_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO

CREATE TABLE dbo.ReservaStock (
    IdReserva INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL, -- FK Lógica hacia CatalogoDB.dbo.Producto
    IdBodega INT NOT NULL,   -- FK Física hacia InventarioDB.dbo.Bodega
    Cantidad INT NOT NULL,
    Estado NVARCHAR(50) NOT NULL CONSTRAINT DF_Reserva_Estado DEFAULT ('PENDIENTE'),
    CONSTRAINT PK_ReservaStock PRIMARY KEY (IdReserva),
    CONSTRAINT FK_ReservaStock_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO

INSERT INTO dbo.Bodega (Nombre, Ciudad, Direccion) VALUES
('Bodega Matriz Quito Norte', 'Quito', 'Av. Prensa N45-12 y Río Coca'),
('Bodega Distribución Guayaquil', 'Guayaquil', 'Parque Empresarial Colón, Bodega 4'),
('Bodega Regional Cuenca', 'Cuenca', 'Av. España y Gil Ramírez Dávalos');

INSERT INTO dbo.Existencia (IdProducto, IdBodega, Stock) VALUES
(1, 1, 25),
(1, 2, 15),
(2, 1, 40),
(3, 3, 12);

INSERT INTO dbo.MovimientoInventario (IdProducto, IdBodega, Tipo, Cantidad, Fecha) VALUES
(1, 1, 'ENTRADA', 25, SYSDATETIME()),
(2, 1, 'ENTRADA', 40, SYSDATETIME()),
(3, 3, 'ENTRADA', 12, SYSDATETIME());

INSERT INTO dbo.ReservaStock (IdProducto, IdBodega, Cantidad, Estado) VALUES
(1, 1, 2, 'CONFIRMADA');
GO


-- ============================================================================
-- 3. BASE DE DATOS: PedidosDB (Microservicio Pedidos.Api)
-- ============================================================================
IF DB_ID(N'PedidosDB') IS NULL
BEGIN
    CREATE DATABASE PedidosDB;
END
GO

USE PedidosDB;
GO

IF OBJECT_ID('dbo.DetallePedido', 'U') IS NOT NULL DROP TABLE dbo.DetallePedido;
IF OBJECT_ID('dbo.Pedido', 'U') IS NOT NULL DROP TABLE dbo.Pedido;
IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL DROP TABLE dbo.Cliente;
GO

CREATE TABLE dbo.Cliente (
    IdCliente INT IDENTITY(1,1) NOT NULL,
    Nombres NVARCHAR(150) NOT NULL,
    Cedula NVARCHAR(20) NOT NULL,
    Correo NVARCHAR(150) NOT NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (IdCliente),
    CONSTRAINT UQ_Cliente_Cedula UNIQUE (Cedula)
);
GO

CREATE TABLE dbo.Pedido (
    IdPedido INT IDENTITY(1,1) NOT NULL,
    IdCliente INT NOT NULL, -- FK Física hacia PedidosDB.dbo.Cliente
    Fecha DATETIME2 NOT NULL CONSTRAINT DF_Pedido_Fecha DEFAULT (SYSDATETIME()),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedido_Total DEFAULT (0.00),
    Estado NVARCHAR(50) NOT NULL CONSTRAINT DF_Pedido_Estado DEFAULT ('PENDIENTE'),
    CONSTRAINT PK_Pedido PRIMARY KEY (IdPedido),
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente(IdCliente)
);
GO

CREATE TABLE dbo.DetallePedido (
    IdDetalle INT IDENTITY(1,1) NOT NULL,
    IdPedido INT NOT NULL,   -- FK Física hacia PedidosDB.dbo.Pedido
    IdProducto INT NOT NULL, -- FK Lógica hacia CatalogoDB.dbo.Producto
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_DetallePedido PRIMARY KEY (IdDetalle),
    CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (IdPedido) REFERENCES dbo.Pedido(IdPedido) ON DELETE CASCADE
);
GO

INSERT INTO dbo.Cliente (Nombres, Cedula, Correo) VALUES
('Andrés Villacreses', '1723456789', 'andres.villacreses@dominio.ec'),
('Valeria Cevallos', '0918273645', 'valeria.cevallos@dominio.ec');

INSERT INTO dbo.Pedido (IdCliente, Fecha, Total, Estado) VALUES
(1, SYSDATETIME(), 379.98, 'PAGADO');

INSERT INTO dbo.DetallePedido (IdPedido, IdProducto, Cantidad, PrecioUnitario) VALUES
(1, 1, 2, 189.99);
GO
