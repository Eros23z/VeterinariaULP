IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VetPointDB')
BEGIN
    CREATE DATABASE VetPointDB;
END
GO

USE VetPointDB;
GO

-- ============================================================================
-- 0. ELIMINACIÓN EN CASCADA (Hijas primero -> Padres al final)
-- ============================================================================
DROP TABLE IF EXISTS Pagos;
DROP TABLE IF EXISTS AtencionesClinicas;
DROP TABLE IF EXISTS Turnos;
DROP TABLE IF EXISTS Mascotas;
DROP TABLE IF EXISTS Servicios;
DROP TABLE IF EXISTS Especies;
DROP TABLE IF EXISTS Dueños;
DROP TABLE IF EXISTS Usuarios;
GO

-- ============================================================================
-- 1. CREACIÓN DE TABLAS
-- ============================================================================

CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Clave VARCHAR(255) NOT NULL,
    Rol VARCHAR(20) NOT NULL DEFAULT 'Veterinario', -- 'Administrador' o 'Veterinario'
    Matricula VARCHAR(30) NULL,
    Avatar VARCHAR(255) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Dueños (
    IdDueño INT IDENTITY(1,1) PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(30) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Direccion VARCHAR(150) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Especies (
    IdEspecie INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Servicios (
    IdServicio INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL,
    PrecioBase DECIMAL(12, 2) NOT NULL,
    DuracionMinutos INT NOT NULL DEFAULT 30,
    Activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Mascotas (
    IdMascota INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Raza VARCHAR(50) NOT NULL,
    Sexo VARCHAR(15) NOT NULL, -- 'Macho', 'Hembra'
    FechaNacimiento DATE NOT NULL,
    Foto VARCHAR(255) NULL,
    Observaciones VARCHAR(255) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    IdEspecie INT NOT NULL,
    IdDueño INT NOT NULL,
    CONSTRAINT FK_Mascotas_Especies FOREIGN KEY (IdEspecie) REFERENCES Especies(IdEspecie),
    CONSTRAINT FK_Mascotas_Dueños FOREIGN KEY (IdDueño) REFERENCES Dueños(IdDueño)
);
GO

CREATE TABLE Turnos (
    IdTurno INT IDENTITY(1,1) PRIMARY KEY,
    FechaHora DATETIME NOT NULL,
    Motivo VARCHAR(150) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente', -- 'Pendiente', 'Atendido', 'Cancelado'
    IdMascota INT NOT NULL,
    IdServicio INT NOT NULL,
    IdVeterinario INT NOT NULL DEFAULT 2,
    CONSTRAINT FK_Turnos_Mascotas FOREIGN KEY (IdMascota) REFERENCES Mascotas(IdMascota),
    CONSTRAINT FK_Turnos_Servicios FOREIGN KEY (IdServicio) REFERENCES Servicios(IdServicio),
    CONSTRAINT FK_Turnos_Usuarios FOREIGN KEY (IdVeterinario) REFERENCES Usuarios(IdUsuario)
);
GO

CREATE TABLE AtencionesClinicas (
    IdAtencion INT IDENTITY(1,1) PRIMARY KEY,
    FechaAtencion DATETIME NOT NULL DEFAULT GETDATE(),
    PesoActual DECIMAL(5, 2) NOT NULL,
    Sintomas VARCHAR(500) NOT NULL,
    Diagnostico VARCHAR(500) NOT NULL,
    TratamientoIndicado VARCHAR(500) NOT NULL,
    ArchivoEstudio VARCHAR(255) NULL, -- Requisito: Manejo de archivos extra (radiografías/estudios)
    IdTurno INT NOT NULL UNIQUE,
    CONSTRAINT FK_Atenciones_Turnos FOREIGN KEY (IdTurno) REFERENCES Turnos(IdTurno)
);
GO

CREATE TABLE Pagos (
    IdPago INT IDENTITY(1,1) PRIMARY KEY,
    FechaPago DATETIME NOT NULL DEFAULT GETDATE(),
    Concepto VARCHAR(150) NOT NULL,
    Importe DECIMAL(12, 2) NOT NULL,
    MetodoPago VARCHAR(30) NOT NULL DEFAULT 'Efectivo', -- 'Efectivo', 'Transferencia', 'Tarjeta'
    Anulado BIT NOT NULL DEFAULT 0,
    IdAtencion INT NOT NULL,
    UsuarioCreaId INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Pagos_Atenciones FOREIGN KEY (IdAtencion) REFERENCES AtencionesClinicas(IdAtencion),
    CONSTRAINT FK_Pagos_Usuarios FOREIGN KEY (UsuarioCreaId) REFERENCES Usuarios(IdUsuario)
);
GO

-- ============================================================================
-- 2. DATOS SEMILLA (SEEDS)
-- ============================================================================

INSERT INTO Usuarios (Nombre, Apellido, Email, Clave, Rol, Matricula, Estado) VALUES
('Administrador', 'General', 'admin@vetpoint.com', 'admin123', 'Administrador', NULL, 1),
('Valeria', 'Montes', 'valeria@vetpoint.com', 'vet123', 'Veterinario', 'MP-4582', 1),
('Marcos', 'Ríos', 'marcos@vetpoint.com', 'vet123', 'Veterinario', 'MP-6109', 1);
GO

INSERT INTO Dueños (Dni, Nombre, Apellido, Telefono, Email, Direccion, Estado) VALUES
('34111222', 'Sofía', 'Alvarez', '2664112233', 'sofia.alvarez@gmail.com', 'Av. del Sol 450', 1),
('31444555', 'Ramiro', 'Giménez', '2664445566', 'ramiro.g@hotmail.com', 'Los Almendros 120', 1),
('38999888', 'Camila', 'Torres', '2664778899', 'camitorres@gmail.com', 'Pedernera 890', 1);
GO

INSERT INTO Especies (Nombre) VALUES
('Canino'), ('Felino'), ('Ave'), ('Roedor'), ('Exótico');
GO

INSERT INTO Servicios (Descripcion, PrecioBase, DuracionMinutos, Activo) VALUES
('Consulta Clínica General', 18000.00, 30, 1),
('Vacunación Séxtuple / Antirrábica', 22000.00, 20, 1),
('Desparasitación Interna y Externa', 12000.00, 15, 1),
('Ecografía / Radiografía Diagnóstica', 35000.00, 45, 1),
('Cirugía Menor / Castración', 65000.00, 90, 1);
GO

INSERT INTO Mascotas (Nombre, Raza, Sexo, FechaNacimiento, Foto, Observaciones, Activo, IdEspecie, IdDueño) VALUES
('Salem', 'Bombay / Mestizo', 'Macho', '2023-04-15', NULL, 'Gato negro, muy tranquilo en consulta.', 1, 2, 1),
('Thor', 'Ovejero Alemán', 'Macho', '2021-08-10', NULL, 'Alérgico a ciertos antiinflamatorios.', 1, 1, 2),
('Luna', 'Caniche Toy', 'Hembra', '2024-01-20', NULL, 'Control de primer año.', 1, 1, 1);
GO

INSERT INTO Turnos (FechaHora, Motivo, Estado, IdMascota, IdServicio, IdVeterinario) VALUES
('2026-10-20 10:00:00', 'Control anual y refuerzo de vacuna', 'Atendido', 1, 2, 2),
('2026-11-05 16:30:00', 'Chequeo por renguera en pata trasera', 'Pendiente', 2, 1, 2);
GO

INSERT INTO AtencionesClinicas (FechaAtencion, PesoActual, Sintomas, Diagnostico, TratamientoIndicado, ArchivoEstudio, IdTurno) VALUES
('2026-10-20 10:15:00', 4.50, 'Sin síntomas adversos. Buen estado de pelaje.', 'Paciente sano apto para inmunización.', 'Aplicación de vacuna antirrábica anual.', NULL, 1);
GO

INSERT INTO Pagos (FechaPago, Concepto, Importe, MetodoPago, Anulado, IdAtencion, UsuarioCreaId) VALUES
('2026-10-20 10:40:00', 'Cobro Vacunación Antirrábica - Salem', 22000.00, 'Transferencia', 0, 1, 1);
GO