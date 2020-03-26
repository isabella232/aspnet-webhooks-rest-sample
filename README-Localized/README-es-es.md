---
page_type: sample
description: "Este ejemplo de MVC en ASP.NET muestra cómo empezar a obtener notificaciones de Microsoft Graph."
products:
- office-365
languages:
- javascript
extensions:
  contentType: samples
  technologies:
  - Mirosoft Graph
  - Microsoft identity platform
  platforms:
  - REST API
  createdDate: 3/9/2016 4:10:08 PM
---
# Webhooks de ASP.NET de Microsoft Graph

Suscríbase a [ webhooks de Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks) para recibir una notificación cuando se produzcan cambios en los datos del usuario, de modo que no tenga que realizar un sondeo de los cambios.

En este ejemplo de ASP.NET MVC, se muestra cómo empezar a obtener notificaciones de Microsoft Graph. Microsoft Graph ofrece un punto de conexión unificado de API para obtener acceso a datos desde la nube de Microsoft.

> Este ejemplo usa el punto de conexión de Azure AD V2 para obtener un token de acceso para cuentas profesionales o educativas. El ejemplo usa un permiso delegado por el usuario, pero los recursos de los mensajes, eventos y contactos también son compatibles con los permisos de aplicación (solo para la aplicación). Actualmente, solo los recursos de elementos raíz de la unidad admiten suscripciones para cuentas personales. Consulte los documentos a medida que agregamos soporte para estas y otras características.

Las tareas comunes que una aplicación realiza con las suscripciones de webhooks son las siguientes:

- Obtener consentimiento para suscribirse a los recursos de los usuarios y después, obtener un token de acceso.
- Usar el token de acceso para [crear una suscripción](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscription_post_subscriptions) a un recurso.
- Devolver un token de validación para confirmar la dirección URL de notificación.
- Escuchar las notificaciones de Microsoft Graph y responder con un código de estado 202.
- Solicitar más información sobre los recursos modificados utilizando los datos en la notificación.

Esta captura de pantalla muestra la página de inicio de la aplicación después de que el usuario inicie sesión.

![Captura de pantalla del ejemplo de Webhook de Microsoft Graph para ASP.NET](readme-images/Page1.PNG)

Después de que la aplicación cree una suscripción en nombre del usuario que ha iniciado sesión, Microsoft Graph envía una notificación al extremo registrado cuando se produce un evento en los datos del usuario. Entonces, la aplicación reaccionará al evento.

Este ejemplo se subscribe al recurso `me/mailFolders('Inbox')/messages` para los cambios `creados`. Recibe una notificación cuando el usuario recibe un mensaje de correo electrónico y, después, actualiza una página con información sobre el mensaje.

## Requisitos previos

Para usar el ejemplo de webhook de Microsoft Graph ASP.NET, necesita lo siguiente:

- Visual Studio 2017 instalado en el equipo de desarrollo.
- Una [cuenta profesional o educativa](http://dev.office.com/devprogram)
- El Id. de la aplicación y la clave que [registró en el portal de registro de aplicaciones](#register-the-app).
- Un extremo HTTPS público para recibir y enviar solicitudes HTTP. Puede hospedar esto en Microsoft Azure u otro servicio, o bien puede [usar ngrok](#set-up-the-ngrok-proxy-optional) o una herramienta similar mientras realiza las pruebas.

### Registrar la aplicación
En este ejercicio, va a crear un nuevo registro de la aplicación web de Azure AD con el centro de administración de Azure Active Directory.

1. Determine la dirección URL de la aplicación ASP.NET. En el Explorador de soluciones de Visual Studio, seleccione el proyecto **GraphWebhooks**. En la ventana de **propiedades**, busque el valor de la **SSL URL**. Copie este valor.

    ![Captura de pantalla de la ventana de propiedades de Visual Studio](readme-images/vs-project-url.PNG)

1. Abra el explorador y vaya al [centro de administración de Azure Active Directory](https://aad.portal.azure.com). Inicie sesión con **una cuenta profesional o educativa**.

1. Seleccione **Azure Active Directory** en el panel de navegación izquierdo y, después seleccione**registros de aplicaciones** en **Administrar**.

    ![Captura de pantalla de los registros de la aplicación ](readme-images/aad-portal-app-registrations.png)

1. Seleccione **Nuevo registro**. En la página **Registrar una aplicación**, establezca los valores siguientes.

    - Establezca un **nombre** preferido, por ejemplo, `ejemplo de GraphWebhooks`.
    - Establezca los **tipos de cuenta compatibles** en las **cuentas en cualquier directorio de la organización**.
    - En la **URI de redirección`, establezca la primera lista desplegable a la `web** y establezca el valor en la dirección URL de la aplicación ASP.NET que copió en el paso 1.

    ![Captura de pantalla de la página Registrar una aplicación](readme-images/aad-register-an-app.png)

1. Elija **Registrar**. En la página **Ejemplo de GraphWebhooks**, copie el valor del **Id. de aplicación (cliente)** y guárdelo. Lo necesitará en el paso siguiente.

    ![Captura de pantalla del id. de aplicación del nuevo registro de la aplicación](readme-images/aad-application-id.PNG)

1. Seleccione **Autenticación** en **Administrar**. Localice la sección **concesión implícita** y habilite los **tokens de id.** Elija **Guardar**.

    ![Captura de pantalla de la sección de Concesión implícita](readme-images/aad-implicit-grant.png)

1. Seleccione **certificados y secretos** en **administrar**. Seleccione el botón **Nuevo secreto de cliente**. Escriba un valor en **Descripción** y seleccione una de las opciones de **Expirar** y luego seleccione **Agregar**.

    ![Captura de pantalla del diálogo Agregar un cliente secreto](readme-images/aad-new-client-secret.png)

1. Copie el valor del secreto del cliente antes de salir de esta página. Lo necesitará en el siguiente paso.

    > [¡IMPORTANTE!]
    > El secreto del cliente no volverá a ser mostrado, asegúrese de copiarlo en este momento.

    ![Captura de pantalla del nuevo secreto del cliente agregado](readme-images/aad-copy-client-secret.png)


### Configurar el proxy ngrok (opcional)

Debe exponer un extremo HTTPS público para crear una suscripción y recibir notificaciones de Microsoft Graph. Mientras realiza las pruebas, puede usar ngrok para permitir temporalmente que los mensajes de Microsoft Graph hagan un túnel a un puerto *localhost* en su equipo.

Puede usar la interfaz web de ngrok ([http://127.0.0.1:4040](http://127.0.0.1:4040)) para inspeccionar el tráfico HTTP que pasa por el túnel. Para obtener más información sobre el uso de ngrok, visite el [sitio web de ngrok](https://ngrok.com/).

1. En el Explorador de soluciones, seleccione el proyecto **GraphWebhooks**.

1. Copie el número de puerto de la **dirección URL** en la ventana de **propiedades**. Si no se muestra la ventana de **propiedades**, elija **Ver > ventana de propiedades**.

    ![El número de puerto de la dirección URL en la ventana Propiedades](readme-images/PortNumber.png)

1. [Descargar ngrok](https://ngrok.com/download) para Windows.

1. Descomprima el paquete y ejecute ngrok.exe.

1. Reemplace los dos valores de marcador de posición *{port-number}* en el siguiente comando con el número de puerto que copió, y después ejecute el comando en la consola de ngrok.

    ```Shell
    ngrok http {port-number} -host-header=localhost:{port-number}
    ```

    ![Comando de ejemplo para ejecutar en la consola de ngrok](readme-images/ngrok1.PNG)

1. Copie la dirección URL HTTPS que se muestra en la consola. Esto se usará para configurar su dirección URL de notificación en el ejemplo.

    ![La dirección URL HTTPS de reenvío en la consola de ngrok](readme-images/ngrok2.PNG)

    > **Nota:** Mantenga la consola abierta mientras realiza las pruebas. Si la cierra, el túnel también se cerrará y tendrá que generar una nueva dirección URL y actualizar el ejemplo.

Para obtener más información, consulte [Hospedaje sin un túnel](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Hosting-the-sample-without-a-tunnel) y [¿Por qué tengo que usar un túnel?](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Why-do-I-have-to-use-a-tunnel).

## Configurar y ejecutar el ejemplo

1. Exponga un extremo de notificación HTTPS público. Este puede ejecutarse en un servicio como Microsoft Azure, o puede crear un servidor web proxy [mediante ngrok](#set-up-the-ngrok-proxy-optional) o una herramienta similar.

1. Realice una copia de **GraphWebHooks/PrivateSettings.example.config** en el mismo directorio. Cambie el nombre de la copia a **PrivateSettings.config**.

1. Abra **GraphWebhooks.sln** en los archivos de ejemplo.

    > **Nota:** Es posible que se le pida que confíe en los certificados para localhost.

1. En el Explorador de soluciones, abra el archivo **PrivateSettings.config** en el directorio raíz del proyecto.
    - Para la clave **ClientId**, reemplace *ENTER\_YOUR\_APP\_ID* con el Id. de aplicación de la aplicación registrada.
    - Para la clave **ClientSecret**, reemplace *ENTER\_YOUR\_SECRET* con el secreto de la aplicación registrada.
    - En la clave **NotificationUrl**, reemplace *ENTER\_YOUR\_URL* por la dirección URL HTTPS. Mantenga la porción */notification/listen*. Si está usando ngrok, utilice la dirección URL HTTPS que ha copiado. El valor será similar al siguiente:

    ```xml
    <add key="ida:NotificationUrl" value="https://0f6fd138.ngrok.io/notification/listen" />
    ```

1. Asegúrese de que la consola de ngrok aún se esté ejecutando y después, presione F5 para crear y ejecutar la solución en el modo de depuración.
    > **Nota:** Si recibe errores durante la instalación de los paquetes, asegúrese de que la ruta de acceso local donde colocó la solución no es demasiado larga o profunda. Para resolver este problema, mueva la solución más cerca de la unidad raíz.
    >
    > Si actualiza alguna dependencia para este ejemplo, **no actualice** `System.IdentityModel.Tokens.Jwt` a v5, que está diseñada para usarse con .NET Core. Tampoco actualice ninguna de las bibliotecas de `Microsoft.Owin` a v4.

### Usar la aplicación

1. Inicie sesión con su cuenta profesional o educativa.

1. Conceda los permisos **leer su correo** y **iniciar su sesión y leer su perfil**.

    Si no ve el permiso **leer su correo**, elija **cancelar** y agregue el permiso **leer correo del usuario** a la aplicación en el portal de Azure. Vea la sección [registrar la aplicación](#register-the-app) para obtener instrucciones.

1. Elija el botón **crear suscripción**. La página de **Suscripción** se carga con información sobre la suscripción.

    > **Nota:** Este ejemplo establece la expiración de la suscripción en 15 minutos para propósitos de prueba.

    ![Página de la aplicación que muestra las propiedades de la nueva suscripción](readme-images/Page2.PNG)

1. Seleccione el botón **Ver las notificaciones**.

1. Envíe un correo electrónico a su cuenta profesional o educativa. La página de **Notificación** muestra algunas propiedades del mensaje. La actualización de la página puede tardar unos segundos.

    ![Página de la aplicación que muestra las propiedades del mensaje nuevo](readme-images/Page3.PNG)

1. Elija el botón **eliminar suscripción y cerrar sesión**.

## Componentes clave del ejemplo

### Controladores

- [`NotificationController.cs`](GraphWebhooks/Controllers/NotificationController.cs) Recibe notificaciones.
- [`SubscriptionController.cs`](GraphWebhooks/Controllers/SubscriptionController.cs) Crea y recibe suscripciones de webhook.

### Modelos

- [`Message.cs`](GraphWebhooks/Models/Message.cs) Define el **MessageViewModel** que representa los datos que se muestran en la vista Notificación.
- [`Notification.cs`](GraphWebhooks/Models/Notification.cs) Representa una notificación de cambios.
- [`Subscription.cs`](GraphWebhooks/Models/Subscription.cs) define el **SubscriptionViewModel** que representa los datos que se muestran en la vista suscripciones.

### Vistas

- [`Notification/Notification.cshtml`](GraphWebhooks/Views/Notification/Notification.cshtml) muestra información sobre los mensajes recibidos y contiene el botón **eliminar suscripción y cerrar sesión**.
- [`Subscription/Index.cshtml`](GraphWebhooks/Views/Subscription/Index.cshtml) página de aterrizaje que contiene el botón **crear suscripción**.
- [`Subscription/Subscription.cshtml`](GraphWebhooks/Views/Subscription/Subscription.cshtml) muestra las propiedades de la suscripción y contiene el botón **supervisar notificaciones**.

### Otros

- [`Web.config`](GraphWebhooks/Web.config) contiene los valores utilizados para la autenticación y la autorización.
- [`App_Start/Startup.Auth.cs`](GraphWebhooks/App_Start/Startup.Auth.cs) contiene el código que se usa para la autenticación y autorización. El ejemplo usa [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-openid-connect-code) y [la biblioteca de autenticación de Microsoft (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) para autenticar y autorizar al usuario.
- [`TokenStorage/SampleTokenCache.cs`](GraphWebhooks/TokenStorage/SampleTokenCache.cs) implementación de ejemplo de una caché de tokens que use System.Runtime.Caching (para que la información de token se encuentre disponible cuando se reciba una notificación). Las aplicaciones de producción suelen usar algún método de almacenamiento persistente.
- [`Helpers/SubscriptionStore.cs`](GraphWebhooks/Helpers/SubscriptionStore.cs) Capa de acceso para la información de la suscripción almacenada. El ejemplo de implementación almacena temporalmente la información en HttpRuntime.Cache. Las aplicaciones de producción suelen usar algún método de almacenamiento persistente.

## Solución de problemas

Si tiene errores o problemas con el ejemplo, consulte la [documentación de solución de problemas](TROUBLESHOOTING.md).

## Colaboradores

Este proyecto ha adoptado el [Código de conducta de código abierto de Microsoft](https://opensource.microsoft.com/codeofconduct/). Para obtener más información, vea [Preguntas frecuentes sobre el código de conducta](https://opensource.microsoft.com/codeofconduct/faq/) o póngase en contacto con [opencode@microsoft.com](mailto:opencode@microsoft.com) si tiene otras preguntas o comentarios.

## Preguntas y comentarios

Nos encantaría recibir sus comentarios sobre el ejemplo de webhook de Microsoft Graph ASP.NET. Puede enviarnos sus preguntas y sugerencias a través de la sección [Problemas](https://github.com/microsoftgraph/aspnet-webhooks-rest-sample/issues) de este repositorio.

Las preguntas sobre Microsoft Graph en general deben publicarse en [Stack Overflow](https://stackoverflow.com/questions/tagged/MicrosoftGraph). Asegúrese de que sus preguntas o comentarios estén etiquetados con `MicrosoftGraph`.

Si quiere sugerir alguna función, publique su idea en nuestra página de [User Voice](https://officespdev.uservoice.com/) y vote por sus sugerencias.

## Recursos adicionales

- [Ejemplo de webhooks de Microsoft Graph Node.js](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample)
- [Trabajar con webhooks en Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)
- [Recurso de suscripción](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/subscription)
- [Sitio para desarrolladores de Microsoft Graph](https://developer.microsoft.com/en-us/graph/)
- [Llamar a Microsoft Graph desde una aplicación de ASP.NET MVC](https://developer.microsoft.com/en-us/graph/docs/platform/aspnetmvc)

## Derechos de autor

Copyright (c) 2019 Microsoft. Todos los derechos reservados.
