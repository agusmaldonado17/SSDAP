# SSDAP - Instrucciones para Codex

Este repositorio corresponde a **SSDAP (Sistema de Soporte a la Decisión para Agua Potable)**.

Antes de realizar cambios relevantes, leer:

- `docs/SSDAP_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/DATABASE.md`
- `docs/ROADMAP.md`

## Objetivo general

SSDAP es una reimplementación moderna de un sistema previo desarrollado aproximadamente en 2016 para asistir en la gestión y toma de decisiones relacionadas con abastecimiento y calidad de agua potable.

La nueva versión debe ser mantenible, modular y utilizable tanto desde una aplicación web como desde una aplicación móvil.

## Stack vigente

- .NET 10
- ASP.NET Core Web API
- Blazor Web App
- .NET MAUI Blazor Hybrid
- C#
- Entity Framework Core
- MySQL / MariaDB
- Git / GitHub

## Proyectos de la solución

- `SSDAP.Api`: backend único y acceso a datos.
- `SSDAP.Web`: aplicación web Blazor.
- `SSDAP.Mobile`: aplicación .NET MAUI Blazor Hybrid.
- `SSDAP.Shared`: modelos, DTO y contratos compartidos cuando corresponda.

Referencias previstas (todavía no configuradas en los `.csproj` al 21/10/2026):

- `SSDAP.Api` -> `SSDAP.Shared`
- `SSDAP.Web` -> `SSDAP.Shared`
- `SSDAP.Mobile` -> `SSDAP.Shared`

## Decisiones confirmadas el 21/10/2026

- Completar primero backend y base de datos; después Web; por último Mobile.
- La aplicación móvil requiere conexión. No implementar operación offline ni sincronización.
- Actualización del 22/10/2026: incluir calendario manual de baterías por días y horarios para estimar producción y costos. La optimización automática sigue postergada. Ver `docs/MODEL_PROPOSAL.md` para el diseño propuesto, todavía no implementado.
- Incluir costos de energía, cloración, compra de agua, personal afectado, amortizaciones y mantenimiento. Usar datos de costos acotados, sin construir gestión completa de personal ni inventario.
- Actualización del 22/10/2026: el caudal del acueducto es un promedio constante ingresado por el usuario, calculado fuera del sistema. El cloro se ingresa como valor medido en la mezcla final, confirmado por el usuario. No inferir consumo ni costos de producto a partir de ese valor.
- La base nueva no contiene datos que deban conservarse. Esto no confirma qué migraciones están aplicadas ni exige recrear la base.
- El usuario entregó código PHP y dos dumps SQL históricos en `C:/Users/ag_ma/Documents/SSDAP/Documentos`. Son referencias de solo lectura: no modificar el sistema anterior. Consultar `docs/LEGACY_REVIEW.md` para ubicaciones y hallazgos.

## Calidad de implementación requerida

El usuario pide una implementación profesional, ordenada y mantenible. Aplicar las convenciones del proyecto descritas en `docs/ARCHITECTURE.md`:

- Separar contratos de API, persistencia, casos de uso y cálculos de dominio.
- Mantener nombres y formato consistentes, métodos acotados y responsabilidades explícitas.
- Validar entradas y reglas de negocio en el backend; asegurar relaciones y unicidad en la base.
- Centralizar cada fórmula, documentar unidades y probar cálculos y casos límite.
- Usar configuración externa para secretos, autorización por localidad y registros de errores sin datos sensibles.
- Revisar migraciones y mantener cambios pequeños verificables; evitar abstracciones sin necesidad concreta.
- Recuperar reglas del legado con evidencia y validación. Un resultado del PHP no demuestra por sí solo que la fórmula sea correcta.

## Reglas de arquitectura

1. Web y Mobile deben consumir el mismo backend `SSDAP.Api`.
2. Web y Mobile **no deben acceder directamente a MySQL**.
3. El acceso a datos debe concentrarse en la API mediante EF Core.
4. Evitar duplicar modelos/DTO entre Web, Mobile y API cuando puedan vivir en `SSDAP.Shared`.
5. Mantener responsabilidades separadas entre entidades de persistencia, DTO, servicios y UI.
6. El DER vigente es la guía principal para relaciones y estructura de datos.
7. No implementar módulos marcados como futuros salvo pedido explícito.
8. Antes de crear una entidad nueva, revisar `docs/DATABASE.md` y confirmar que no exista ya con otro nombre o mediante una relación existente.
9. Antes de crear una funcionalidad nueva, revisar `docs/ROADMAP.md`.
10. Priorizar una implementación simple, clara y mantenible por sobre abstraer prematuramente.

## Forma de trabajo preferida

El proyecto se viene desarrollando por pasos pequeños. Para cambios importantes:

1. Indicar brevemente qué se va a modificar.
2. Implementar una unidad funcional acotada.
3. Mantener el proyecto compilable.
4. Indicar archivos modificados.
5. Proponer o indicar el comando de migración cuando haya cambios de esquema.
6. Dar un resumen corto apto para usar como mensaje de commit.

Cuando se trabaje con EF Core, normalmente entregar en el mismo paso:

- entidad/modelo;
- cambio de `DbContext`;
- configuración de relaciones si corresponde;
- migración/comando necesario;
- endpoint o servicio relacionado cuando forme parte del mismo incremento.

## Estado base que NO debe romperse

La solución ya fue creada y los cuatro proyectos funcionan. `SSDAP.Mobile` fue recreado desde cero después de un problema producido al renombrar un proyecto MAUI; **evitar renombrar el proyecto MAUI**.

MySQL ya fue conectado mediante EF Core y existen migraciones iniciales. No recrear la solución ni cambiar de stack salvo pedido explícito.

## Fuente de verdad ante contradicciones

Si documentación histórica y código actual difieren, usar este orden de prioridad:

Las decisiones explícitas actuales del usuario prevalecen sobre el alcance histórico. Para determinar qué está implementado, usar:

1. Código y migraciones actuales del repositorio.
2. DER vigente (`ERD - SSDAP-v.1.2.pdf`, si está disponible en el repositorio/documentación).
3. `docs/DATABASE.md`.
4. `docs/SSDAP_CONTEXT.md`.
5. Notas históricas.

Si una diferencia afecta el modelo de datos, no asumir silenciosamente: señalarla antes de realizar una modificación destructiva.
