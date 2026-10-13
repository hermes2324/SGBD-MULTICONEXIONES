<h1 align="center">🗄️ SGBD Multiconexiones</h1>

<p align="center">
  Un solo programa de escritorio para conectarte a <b>cinco motores de bases de datos</b>,<br />
  crear bases de datos y explorar sus tablas, sin abrir un gestor distinto para cada uno.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/Windows%20Forms-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows Forms" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=flat-square&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Oracle-F80000?style=flat-square&logo=oracle&logoColor=white" alt="Oracle" />
  <img src="https://img.shields.io/badge/Firebird-F40F02?style=flat-square&logo=firebird&logoColor=white" alt="Firebird" />
</p>

---

## ✨ Funciones

- 🔌 **Conexión a varios motores**: SQL Server, MySQL, PostgreSQL, Oracle y Firebird
- 🔐 Inicio de sesión con **servidor, usuario, contraseña** y modo de autenticación (Windows o del motor)
- 🆕 **Crear bases de datos** nuevas desde la app
- 📋 **Explorar tablas** y ver sus datos en una cuadrícula
- 🧩 Varias **instancias** abiertas a la vez

## 🧱 Arquitectura

Cada motor vive en su propia clase y todos comparten una base común, así que agregar un motor nuevo es implementar una clase más:

```text
├── libreriaConsultasBD/   ← ConexionBase + SQLSERVER, MYSQL, POSTGRESQL, ORACLE, FIREBIRD
├── SistemaGestor/         ← interfaz: inicio de sesión, instancias y tablas
├── InicioPrincipal/       ← versión anterior de la interfaz
└── OperacionesBD/, Oracle/, SQL SERVER/, ClassLibrary1/  ← primeras pruebas por motor
```

## 🚀 Cómo correrlo

1. Abre `SGBD MULTICONEXIONES.sln` en **Visual Studio** y restaura los paquetes NuGet (MySql.Data, Npgsql, Oracle.ManagedDataAccess, FirebirdSql).
2. Ejecuta con <kbd>F5</kbd> y conéctate a cualquiera de los motores que tengas instalado.

Las credenciales se escriben al iniciar sesión: no hay contraseñas guardadas en el código.

---

<p align="center">Hecho por <a href="https://github.com/hermes2324">Hermes Verdugo</a> · TecNM Campus Guasave</p>
