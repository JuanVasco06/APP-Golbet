# GolBet

Simulador académico de apuestas de fútbol desarrollado con ASP.NET Core MVC, .NET 8 y SQL Server. Incluye los módulos 1 al 10 y el buscador de partidos de la segunda entrega.

La aplicación permite consultar partidos, buscar por nombre de equipo y filtrar por estado. Incluye registro e inicio de sesión; el administrador puede gestionar equipos y partidos. Los apostadores pueden apostar FutCoins en partidos programados futuros y consultar sus apuestas, con la cuota guardada al momento de apostar. El administrador inicia el partido (se cierran las apuestas) y lo cierra con el marcador final: las apuestas se resuelven y los ganadores reciben su pago con esa cuota. La aplicación registra con Serilog las apuestas y los cierres de partido en la consola y en `GolBet.Web/Logs/` (un archivo por día, se conservan los últimos 7) y muestra páginas propias para los errores 404 y 500.

## Ejecutar en Windows

Necesitas Visual Studio con la carga **ASP.NET y desarrollo web**, SQL Server LocalDB y conexión a internet para preparar las dependencias.

1. Ejecuta `Preparar-Entorno.cmd` al usar el proyecto por primera vez. Instala un SDK local de .NET 8 y restaura los paquetes.
2. Abre `Abrir-VisualStudio.cmd` y selecciona **GolBet.Web** como proyecto de inicio.
3. Elige el perfil **GolBet local** y presiona **F5**.

También puedes usar `Iniciar-GolBet.cmd` para abrir la aplicación en [localhost:5229](http://localhost:5229). Ejecuta `Detener-GolBet.cmd` antes de volver a compilar desde Visual Studio.

La primera ejecución crea `GolBetDB_DisenoSoft` en LocalDB y carga equipos y partidos de ejemplo. Los siguientes arranques conservan los datos. Si LocalDB no inicia, ejecuta `Preparar-LocalDB.cmd` y vuelve a intentar.

## Proyectos

- `GolBet.Entities`: entidades y enumeraciones.
- `GolBet.Repositories`: acceso a datos, migraciones y datos iniciales.
- `GolBet.Services`: reglas de negocio, DTOs y mapeos.
- `GolBet.Web`: controladores, vistas y archivos de la interfaz.

## Cuentas de desarrollo

| Cuenta | Contraseña | Rol |
| --- | --- | --- |
| `admin@golbet.com` | `Admin123*` | Administrador |
| `bettor@golbet.com` | `Bettor123*` | Apostador |

Estas cuentas solo se crean en el entorno `Development`. El apostador comienza con 100.000 FutCoins; reiniciar la aplicación no repone el saldo.
