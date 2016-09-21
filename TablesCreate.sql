CREATE TABLE Usuarios(
nombre varchar(25) not null,
cedula varchar(11) primary key,
id nvarchar(128) not null,
constraint fk_AspUserId foreign key (id) references AspNetUsers(id)
);

CREATE TABLE Usuarios_Telefonos(
usuario varchar(11),
telefono char(8),
constraint pk_Req primary key (usuario, telefono),
constraint fk_User foreign key (usuario) references Usuarios(cedula)
);

CREATE TABLE Permisos(
codigo int identity(1,1) primary key,
nombre varchar(20) not null
);

CREATE TABLE Rol_Permisos(
rol nvarchar(128),
permiso int,
constraint pk_RP primary key (rol, permiso),
constraint fk_Rol foreign key (rol) references AspNetRoles(id),
constraint fk_Permiso foreign key (permiso) references Permisos(codigo)
);

CREATE TABLE Proyectos(
nombre varchar(25) primary key,
lider varchar(11) not null,
descripcion varchar(80),
constraint fk_UserProyecto foreign key (lider) references Usuarios(cedula)
);

CREATE TABLE Proyecto_Equipo(
usuario varchar(11),
proyecto varchar(25),
constraint pk_ReqPE primary key (proyecto, usuario),
constraint fk_Proy foreign key (proyecto) references Proyectos(nombre),
constraint fk_UserPE foreign key (usuario) references Usuarios(cedula)
);

CREATE TABLE Modulos(
numero int identity(1,1),
nombre varchar(25) not null,
proyecto varchar(25),
constraint pk_ReqMod primary key (proyecto, numero),
constraint fk_ProyMod foreign key (proyecto) references Proyectos(nombre)
);

CREATE TABLE Sprints(
proyecto varchar(25),
numero int identity(1,1),
fechaInicio date not null,
fechaFinal date not null,
constraint pk_ReqSprint primary key (proyecto, numero),
constraint fk_UserSprint foreign key (proyecto) references Proyectos(nombre)
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
estado char(13) check (estado in ('Pendiente','Implementando','Probando', 'Terminado')),
creadoPor varchar(11) not null,
solicitadoPor varchar(11) not null,
proyecto varchar(25) not null,
modulo int not null,
constraint primarykey_Req primary key (codigo, version),
constraint fk_ReqUser foreign key (creadoPor) references Usuarios(cedula),
constraint fk_ReqUser2 foreign key (solicitadoPor) references Usuarios(cedula),
constraint fk_ModReq foreign key (proyecto, modulo) references Modulos(proyecto, numero)
);

CREATE TABLE Sprint_Requerimientos(
proyecto varchar(25),
sprint int,
requerimiento char(15),
versionRequerimiento int,
constraint pk_SR primary key (proyecto, sprint, requerimiento, versionRequerimiento),
constraint fk_ProySR foreign key (proyecto,sprint) references Sprints(proyecto,numero),
constraint fk_ReqSR foreign key (requerimiento,versionRequerimiento) references Requerimientos(codigo,version),
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
constraint fk_ReqCam foreign key (requerimiento,versionReqVieja) references Requerimientos(codigo,version),
constraint fk_ReqCam2 foreign key (requerimiento,versionReqNueva) references Requerimientos(codigo,version),
constraint fk_UserCam foreign key (creadoPor) references Usuarios(cedula)
);