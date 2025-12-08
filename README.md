## Arquitectura Limpia (MVC)

# PlanIT.Domain
Define el conjunto de logica funcional

    -> Entities sueltas (user, travel)
    -> Interfaces, medio por donde se hacen los pases de datos y que se hace

# PlanIT.BussinessLogic
Parte logica que realiza las tareas

    -> DTO pasar info entre capas
    -> Interfaces son los servicios que va a utilizar WebApi
    -> Services el implemento y la logica real

# PlanIT.Infrastructure
Aca se implementa todo lo de Domain y BusinessLogic (y de paso depende de ellas)

    -> Data implementacion de Entity y UnitOfWork
    -> Repositories Clases implementadas por el repositorio, para escribir y leer el sql server
    -> Authentication implementacion de jwt para crear y formar tokens
    -> Migraciones actualizaciones de las tablas del server

# PlanIT.WebApi
Recibe peticiones de React y lo muestra con el exterior

    Program.cs 
                -> Registra inyeccion de dependencias (conectar las interfaces con las clases concretas)
                -> Configura los middlewares (peticiones)
                -> Define endpoints (post, get)
    -> Appsettings configuracion de archivos db y jwt 

<--------------------------------------------------------------->

## Caso de Uso

1) Agregar Entidad (Objeto de negocio)

    - Domain, agregas las entities y definir propiedades
    - Infrastructure, avisa que existe nuestra entidad a la base de datos
    - Crear la/s tables en SQL Server (add, update)

2) Agregar Nueva Función (Lo que la API puede hacer)

    - Interfaces, agregar la interfaz del servicio como metodo
    - Services, implementar la lógica de la función
    - Domain, Si la función necesita nuevos metodos de base de datos
    - Infrastructure, implementar los métodos

- Finalmente, colocar la función a React
    -   Crear endpoint en Program.cs

<--------------------------------------------------------------->


# Documentacion extra guiada

- https://learn.microsoft.com/es-es/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application

- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-9.0
