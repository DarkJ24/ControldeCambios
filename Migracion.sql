--Migracion de Cambios

DROP TABLE Requerimiento_Encargados, Requerimientos_Cri_Acep, Requerimientos;

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

CREATE TABLE Solicitud_Cambios(
id int identity(1,1) primary key,
req1 int not null,
req2 int not null,
razon varchar(200) not null,
solicitadoPor varchar(11) not null,
solicitadoEn date not null,
aprobadoPor varchar(11),
aprobadoEn date,
estado varchar(15) not null,
constraint fk_CambiosU1 foreign key (aprobadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosU2 foreign key (solicitadoPor) references Usuarios(cedula)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosEstado foreign key (estado) references Estado_Solicitud(nombre)
ON UPDATE CASCADE
ON DELETE NO ACTION,
constraint fk_CambiosReq1 foreign key (req1) references Requerimientos(id)
ON UPDATE NO ACTION
ON DELETE NO ACTION,
constraint fk_CambiosReq2 foreign key (req2) references Requerimientos(id)
ON UPDATE NO ACTION
ON DELETE NO ACTION
);

INSERT INTO Categoria_Requerimientos
VALUES('Actual'), 
('Historial'),
('Solicitud'),
('Rechazada');

INSERT INTO Estado_Solicitud
VALUES('Aprobado'), 
('En revisión'),
('Rechazado');