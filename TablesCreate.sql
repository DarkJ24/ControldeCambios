--Primero se ejecuta esto:

CREATE TABLE Usuarios(
nombre varchar(25) not null,
cedula varchar(11) primary key,
id nvarchar(128) not null,
updatedAt datetime default getDate(),
constraint fk_AspUserId foreign key (id) references AspNetUsers(id)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Usuarios_Telefonos(
usuario varchar(11),
telefono char(8),
constraint pk_Req primary key (usuario, telefono),
constraint fk_User foreign key (usuario) references Usuarios(cedula)
ON UPDATE CASCADE
ON DELETE CASCADE
);

CREATE TABLE Permisos(
codigo int identity(1,1) primary key,
nombre varchar(40) not null
);

CREATE TABLE Rol_Permisos(
rol nvarchar(128),
permiso int,
constraint pk_RP primary key (rol, permiso),
constraint fk_Rol foreign key (rol) references AspNetRoles(id)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_Permiso foreign key (permiso) references Permisos(codigo)
ON UPDATE CASCADE
ON DELETE CASCADE
);

CREATE TABLE Estado_Proyecto(
nombre char(24) primary key --Por iniciar, En ejecución, En pausa, Finalizado, Cerrado
);

CREATE TABLE Proyectos(
nombre varchar(25) primary key,
descripcion varchar(80),
lider varchar(11) not null,
estado char(24) not null,
fechaInicio date not null default getDate(),
fechaFinal date not null,
duracion int,
cliente varchar(11) not null,
constraint fk_EstadoProye foreign key (estado) references Estado_Proyecto(nombre),
constraint fk_UserProyCliente foreign key (lider) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_UserProy foreign key (lider) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
);

CREATE TABLE Tipo_Desarrollador(
nombre char(13) primary key --Tester, Diseñador, Programador
);

CREATE TABLE Proyecto_Equipo(
usuario varchar(11),
proyecto varchar(25),
tipo char(13),
constraint pk_ReqPE primary key (proyecto, usuario),
constraint fk_Proy foreign key (proyecto) references Proyectos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_UserPE foreign key (usuario) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_UserPETipo foreign key (tipo) references Tipo_Desarrollador(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Sprints(
proyecto varchar(25),
numero int,
fechaInicio date not null,
fechaFinal date not null,
constraint pk_ReqSprint primary key (proyecto, numero),
constraint fk_UserSprint foreign key (proyecto) references Proyectos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Progreso_Sprint(
fecha date not null,
sprintProyecto varchar(25) not null,
sprintNumero int not null,
puntos int not null,
constraint pk_ProgSprint primary key (fecha, sprintProyecto, sprintNumero),
constraint fk_ProgSprint foreign key (sprintProyecto,sprintNumero) references Sprints(proyecto, numero)
ON UPDATE CASCADE
ON DELETE CASCADE
);

CREATE TABLE Modulos(
proyecto varchar(25),
numero int identity(1,1),
nombre varchar(25) not null,
constraint pk_ReqModulo1 primary key (proyecto, numero),
constraint fk_ProyectoModulo1 foreign key (proyecto) references Proyectos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Sprint_Modulos(
modulo int,
proyecto varchar(25),
sprint int,
constraint pk_SprintModulos primary key (proyecto, modulo, sprint),
constraint fk_SprintModulo foreign key (proyecto, modulo) references Modulos(proyecto, numero)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_SprintReqSprint foreign key (proyecto, sprint) references Sprints(proyecto, numero)
ON UPDATE NO ACTION
ON DELETE NO ACTION
);

CREATE TABLE Estado_Requerimientos(
nombre char(24) primary key --Pendiente de asignación, Asignado, En ejecución, Finalizado, Cerrado
);

CREATE TABLE Categoria_Requerimientos(
nombre varchar(10) primary key --Actual, Historial, Solicitud, Rechazada
);

CREATE TABLE Requerimientos(
id int identity(1,1) primary key,
codigo char(15) not null,
version int default 1,
creadoEn date not null,
finalizaEn date,
descripcion varchar(120),
nombre varchar(25) not null,
prioridad int not null,
observaciones varchar(150),
esfuerzo int default 1,
estado char(24) not null,
creadoPor varchar(11) not null,
solicitadoPor varchar(11) not null,
proyecto varchar(25) not null,
modulo int,
imagen varbinary(MAX),
categoria varchar(10) not null,
constraint fk_EstadoReq foreign key (estado) references Estado_Requerimientos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_DescReq foreign key (categoria) references Categoria_Requerimientos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_ReqUserCre foreign key (creadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqUserSol foreign key (solicitadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqProyecto foreign key (proyecto) references Proyectos(nombre)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqMod foreign key (proyecto, modulo) references Modulos(proyecto, numero)
ON UPDATE NO ACTION
ON DELETE NO ACTION
);

CREATE TABLE Requerimientos_Cri_Acep(
id int identity(1,1) primary key,
idReq int,
criterio varchar(120),
constraint fk_ReqCriAcp foreign key (idReq) references Requerimientos(id)
ON UPDATE CASCADE
ON DELETE CASCADE,
);

CREATE TABLE Requerimiento_Encargados(
idReq int,
usuario varchar(11),
constraint pk_Req_User primary key (idReq, usuario),
constraint fk_ReqResUser foreign key (usuario) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqResReq foreign key (idReq) references Requerimientos(id)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Estado_Solicitud(
nombre varchar(15) primary key --Aprobado, Rechazado, En revisión
);

CREATE TABLE Tipo_Solicitud(
nombre varchar(15) primary key --Modificar, Eliminar
);

CREATE TABLE Solicitud_Cambios(
id int identity(1,1) primary key,
req1 int not null,
req2 int,
razon varchar(200) not null,
solicitadoPor varchar(11) not null,
solicitadoEn date not null,
aprobadoPor varchar(11),
aprobadoEn date,
estado varchar(15) not null,
tipo varchar(15) not null,
comentario varchar(200), --Comentario del Líder
proyecto varchar(25) not null,
constraint fk_CambiosU1 foreign key (aprobadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosU2 foreign key (solicitadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosEstado foreign key (estado) references Estado_Solicitud(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_CambiosTipo foreign key (tipo) references Tipo_Solicitud(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_CambiosReq1 foreign key (req1) references Requerimientos(id)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosReq2 foreign key (req2) references Requerimientos(id)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosProyecto foreign key (proyecto) references Proyectos(nombre)
ON UPDATE NO ACTION
ON DELETE NO ACTION
);

INSERT INTO Permisos
VALUES('Crear Usuarios'),
('Consular Lista de Usuarios'),
('Consultar Detalles de Usuarios'),
('Eliminar Usuarios'),
('Modificar Usuarios'),
('Gestionar Permisos'),
('Crear Proyectos'),
('Consultar Lista de Proyectos'),
('Modificar Proyectos'),
('Eliminar Proyectos'),
('Consultar Detalles de Proyectos'),
('Crear Módulos'),
('Modificar Módulos'),
('Eliminar Módulos'),
('Consultar Detalles de Módulos'),
('Crear Sprints'),
('Modificar Sprints'),
('Eliminar Sprints'),
('Consultar Detalles de Sprints'),
('Crear Requerimientos'),
('Consultar Lista de Requerimientos'),
('Modificar Requerimientos'),
('Eliminar Requerimientos'),
('Consultar Detalles de Requerimiento');

INSERT INTO Estado_Proyecto
VALUES('Por iniciar'), 
('En ejecución'),
('En pausa'),
('Finalizado'),
('Cerrado');

INSERT INTO Tipo_Desarrollador
VALUES('Tester'),
('Diseñador'),
('Programador');

INSERT INTO Estado_Requerimientos
VALUES('Pendiente de asignación'), 
('Asignado'),
('En ejecución'),
('Finalizado'),
('Cerrado');

INSERT INTO Categoria_Requerimientos
VALUES('Actual'), 
('Historial'),
('Solicitud'),
('Rechazada');

INSERT INTO Estado_Solicitud
VALUES('Aprobado'), 
('En revisión'),
('Rechazado');

INSERT INTO Tipo_Solicitud
VALUES('Modificar'), 
('Eliminar');

--Segundo se ejecuta esto:

CREATE TRIGGER trg_Usuarios_UpdatedAt ON Usuarios for UPDATE AS
BEGIN
  UPDATE Usuarios
	SET updatedAt = getDate()
	FROM Usuarios INNER JOIN Deleted d
	ON Usuarios.id=d.id
END

--Correr Visual y luego ejecutar este tercero
UPDATE AspNetUsers
SET EmailConfirmed = 1
WHERE Email = 'admin@admin.com';

--Para borrar todo:

DROP TRIGGER trg_Usuarios_UpdatedAt;

DROP TABLE Solicitud_Cambios, Tipo_Solicitud, Estado_Solicitud, Requerimiento_Encargados, Requerimientos_Cri_Acep, 
Requerimientos, Categoria_Requerimientos, Estado_Requerimientos, Sprint_Modulos, Modulos, Progreso_Sprint, Sprints, 
Proyecto_Equipo, Tipo_Desarrollador, Proyectos, Estado_Proyecto, Rol_Permisos, Permisos, Usuarios_Telefonos, Usuarios;

DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;