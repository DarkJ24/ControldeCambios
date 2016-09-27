Drop table Estado_Requerimientos, CambiosRequerimientos, Requerimiento_Usuarios, Requerimientos, 
Estado_Requerimientos, Tipo_Vinculacion, Sprints, Modulos, Rol_Permisos, 
Permisos, Proyecto_Equipo, Tipo_Desarrollador, Proyectos, Usuarios_Telefonos, Usuarios;

CREATE TABLE Usuarios(
nombre varchar(25) not null,
cedula varchar(11) primary key,
id nvarchar(128) not null,
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
nombre varchar(20) not null
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

CREATE TABLE Proyectos(
nombre varchar(25) primary key,
descripcion varchar(80)
);

CREATE TABLE Tipo_Desarrollador(
nombre char(13) primary key
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
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_UserPETipo foreign key (tipo) references Tipo_Desarrollador(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Sprints(
proyecto varchar(25),
numero int identity(1,1),
fechaInicio date not null,
fechaFinal date not null,
constraint pk_ReqSprint primary key (proyecto, numero),
constraint fk_UserSprint foreign key (proyecto) references Proyectos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
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
nombre char(13) primary key --Pendiente, En Progreso, En Pruebas, Terminado
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
estado char(13) not null,
constraint primarykey_Req primary key (codigo, version),
constraint fk_EstadoReq foreign key (estado) references Estado_Requerimientos(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION
);

CREATE TABLE Tipo_Vinculacion(
nombre char(13) primary key --Creado Por, Solicitado Por, Encargado1, 2, etc...
);

CREATE TABLE Requerimiento_Usuarios(
requerimiento char(15),
version int,
usuario varchar(11),
vinculacion char(13),
constraint pk_Req_User primary key (requerimiento, version, usuario),
constraint fk_ReqResUser foreign key (usuario) references Usuarios(cedula)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_ReqResReq foreign key (requerimiento,version) references Requerimientos(codigo,version)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_ReqResVin foreign key (vinculacion) references Tipo_Vinculacion(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
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
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_UserCam foreign key (creadoPor) references Usuarios(cedula)
ON UPDATE CASCADE
ON DELETE NO ACTION
);