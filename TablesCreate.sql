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
fechaInicio date default getDate(),
fechaFinal date,
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
constraint fk_ProgSprint foreign key (sprintProyecto,sprintNumero) references Proyectos(proyecto, numero)
ON UPDATE CASCADE
ON DELETE CASCADE
);

CREATE TABLE Modulos(
id int identity(1,1) primary key,
numero int not null,
nombre varchar(25) not null
);

CREATE TABLE Sprint_Modulo(
proyecto varchar(25),
sprint int not null,
modulo int not null,
constraint pk_SpMod primary key (proyecto, sprint, modulo),
constraint fk_SpMod foreign key (proyecto, sprint) references Sprints(proyecto, numero)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_UserSprintMod foreign key (modulo) references Modulos(id)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Estado_Requerimientos(
nombre char(24) primary key --Pendiente de asignación, Asignado, En ejecución, Finalizado, Cerrado
);

CREATE TABLE Requerimientos(
codigo char(15),
version int,
creadoEn date not null,
descripcion varchar(120),
nombre varchar(25) not null,
prioridad int not null,
observaciones varchar(150),
esfuerzo int default 1,
estado char(24) not null,
creadoPor varchar(11) not null,
solicitadoPor varchar(11) not null,
imagen varbinary(MAX),
constraint primarykey_Req primary key (codigo, version),
constraint fk_EstadoReq foreign key (estado) references Estado_Requerimientos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_ReqUserCre foreign key (creadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqUserSol foreign key (solicitadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION
);

CREATE TABLE Requerimientos_Cri_Acep(
reqCodigo char(15),
reqVersion int,
criterio varchar(120),
constraint pk_ReqCriAcep primary key (reqCodigo, reqVersion, criterio)
constraint fk_EstadoReq foreign key (reqCodigo, reqVersion) references Requerimientos(codigo, version)
ON UPDATE CASCADE
ON DELETE CASCADE,
);

CREATE TABLE Requerimiento_Encargados(
requerimiento char(15),
version int,
usuario varchar(11),
constraint pk_Req_User primary key (requerimiento, version, usuario),
constraint fk_ReqResUser foreign key (usuario) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_ReqResReq foreign key (requerimiento,version) references Requerimientos(codigo,version)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Sprint_Mod_Req(
proyecto varchar(25),
sprint int,
modulo int,
requerimiento char(15),
version int,
constraint pk_SpModReq primary key (proyecto, sprint, modulo, requerimiento, version),
constraint fk_SpModReq foreign key (proyecto, sprint, modulo) references Sprint_Modulo(proyecto, sprint, modulo)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_SpModReqReq foreign key (requerimiento, version) references Requerimientos(codigo, version)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE CambiosRequerimientos(
creadoPor varchar(11) not null,
requerimiento char(15),
versionReqVieja int,
versionReqNueva int,
fecha date default getDate(),
comentarios varchar(150),
motivo varchar(80),
constraint pk_ReqCam primary key (requerimiento, versionReqVieja, versionReqNueva),
constraint fk_ReqCam foreign key (requerimiento,versionReqVieja) references Requerimientos(codigo,version)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_ReqCam2 foreign key (requerimiento,versionReqNueva) references Requerimientos(codigo,version)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_UserCam foreign key (creadoPor) references Usuarios(cedula)
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
('Crear Requerimientos'),
('Consultar Lista de Requerimientos'),
('Modificar Requerimientos'),
('Eliminar Requerimientos'),
('Consultar Detalles de Requerimiento');

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

DROP TABLE CambiosRequerimientos, Sprint_Mod_Req, Requerimiento_Encargados, Requerimientos_Cri_Acep, Requerimientos,
Estado_Requerimientos, Sprint_Modulo, Modulos, Progreso_Sprint, Sprints, Proyecto_Equipo, Tipo_Desarrollador,
Proyectos, Estado_Proyecto, Rol_Permisos, Permisos, Usuarios_Telefonos, Usuarios;

DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;