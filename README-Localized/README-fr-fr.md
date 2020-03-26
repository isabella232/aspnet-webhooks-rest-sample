---
page_type: sample
description: « Cet exemple MVC ASP.NET montre comment commencer à recevoir des notifications à partir de Microsoft Graph. »
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
# Webhooks ASP.NET Microsoft Graph

Abonnez-vous à des [webhooks Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks) pour être averti lorsque les données de vos utilisateurs changent, de sorte que vous n’avez pas besoin d’interroger les modifications.

Cet exemple ASP.NET MVC montre comment commencer à recevoir des notifications de Microsoft Graph. Microsoft Graph fournit un point de terminaison d’API unifiée pour accéder aux données à partir de Microsoft Cloud.

> Cet exemple utilise le point de terminaison Azure AD version 2 pour obtenir un jeton d’accès pour les comptes professionnels ou scolaires. Cet exemple utilise une autorisation déléguée par l’utilisateur, mais les messages, les événements et les ressources de contacts prennent également en charge les autorisations d’application (app uniquement). Pour l’instant, seules les ressources d’élément racine de lecteur prennent en charge les abonnements pour les comptes personnels. Regardez les documents à mesure que nous continuons à ajouter la prise en charge de ces fonctionnalités et d’autres encore.

Voici les tâches courantes qu’une application effectue avec des abonnements webhook :

- Obtenez le consentement pour vous abonner aux ressources des utilisateurs, puis obtenez un jeton d’accès.
- Utilisez le jeton d'accès pour [créer un abonnement](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscription_post_subscriptions) à une ressource.
- Renvoyez un jeton de validation pour confirmer l'URL de notification.
- Écoutez les notifications de Microsoft Graph et répondez avec un code d’État 202.
- Demandez plus d’informations sur les ressources modifiées à l’aide des données de la notification.

Cette capture d’écran représente la page de démarrage de l’application une fois l’utilisateur connecté.

![Capture d’écran de l’exemple de Webhook Microsoft Graph pour ASP.NET](readme-images/Page1.PNG)

Une fois que l'application a créé un abonnement pour l'utilisateur connecté, Microsoft Graph envoie une notification au terminal enregistré lorsque des événements se produisent dans les données de l'utilisateur. L’application réagit ensuite à l’événement.

Cet exemple s’abonne à la ressource `me/mailFolders('Inbox')/messages` pour les modifications `créées`. Il permet de recevoir une notification lorsque l’utilisateur reçoit un e-mail, puis met à jour une page contenant des informations sur le message.

## Conditions préalables

Pour utiliser l’exemple Microsoft Graph pour ASP.NET Webhooks, vous avez besoin des éléments suivants :

- Visual Studio 2017 installé sur votre ordinateur de développement.
- Un [compte professionnel ou scolaire](http://dev.office.com/devprogram).
- ID de l’application et clé de l’application que vous [inscrivez sur le portail d’enregistrement d’application](#register-the-app).
- Un point de terminaison public HTTPS pour recevoir et envoyer des demandes HTTP. Vous pouvez héberger celui-ci sur Microsoft Azure ou un autre service, ou vous pouvez [utiliser ngrok](#set-up-the-ngrok-proxy-optional) ou un outil similaire pendant le test.

### Inscription de l’application
Dans cet exercice, vous aller créer un nouvel enregistrement d’application Web Azure AD à l’aide du centre d’administration Azure Active Directory.

1. Déterminez l’URL de votre application ASP.NET. Dans l’Explorateur de solutions de Visual Studio, sélectionnez le projet **GraphWebhooks**. Dans la fenêtre **Propriétés**, cherchez la valeur de l’**URL SSL**. Copiez cette valeur.

    ![Capture d'écran de la fenêtre des propriétés de Visual Studio](readme-images/vs-project-url.PNG)

1. Ouvrez un navigateur et accédez au [Centre d’administration Azure Active Directory](https://aad.portal.azure.com). Connectez-vous en utilisant un **compte professionnel ou scolaire**.

1. Sélectionnez **Azure Active Directory** dans le volet de navigation gauche, puis sélectionnez **Inscriptions d’applications** sous **Gérer**.

    ![Une capture d’écran des inscriptions d’applications ](readme-images/aad-portal-app-registrations.png)

1. Sélectionnez **Nouvelle inscription**. Sur la page **Inscrire une application**, définissez les valeurs comme suit.

    - Donnez un **nom** préféré, `Exemple GraphWebhooks` par exemple.
    - Définissez les **Types de comptes pris en charge** sur **Comptes figurant dans un annuaire organisationnel**.
    - Sous **URI de redirection**, définissez la première flèche déroulante sur `Web` et la valeur sur l’URL SSL d’application ASP.NET que vous avez copiée à l’étape 1.

    ![Capture d’écran de la page Inscrire une application](readme-images/aad-register-an-app.png)

1. Choisissez **Inscrire**. Dans la page **Exemple GraphWebhooks**, copiez la valeur de **ID d’application (client)** et enregistrez-la, car vous en aurez besoin à l’étape suivante.

    ![Capture d’écran de l’ID d’application de la nouvelle inscription d'application](readme-images/aad-application-id.PNG)

1. Sélectionnez **Authentification** sous **Gérer**. Recherchez la rubrique **Octroi implicite**, puis activez **Jetons de l’ID**. Choisissez **Enregistrer**.

    ![Une capture d’écran de la rubrique octroi implicite](readme-images/aad-implicit-grant.png)

1. Sélectionnez **Certificats et secrets** sous **Gérer**. Sélectionnez le bouton **Nouvelle clé secrète client**. Entrez une valeur dans la **Description**, sélectionnez l'une des options pour **Expire le**, puis choisissez **Ajouter**.

    ![Une capture d’écran de la boîte de dialogue Ajouter une clé secrète client](readme-images/aad-new-client-secret.png)

1. Copiez la valeur due la clé secrète client avant de quitter cette page. Vous en aurez besoin à l’étape suivante.

    > [!IMPORTANT]
    >Cette clé secrète client n’apparaîtra plus, aussi veillez à la copier maintenant.

    ![Capture d’écran de la clé secrète client récemment ajoutée](readme-images/aad-copy-client-secret.png)


### Configurer le proxy ngrok (facultatif)

Vous devez exposer un point de terminaison public HTTPS pour créer un abonnement et recevoir des notifications de Microsoft Graph. Pendant le test, vous pouvez utiliser ngrok pour autoriser temporairement les messages de Microsoft Graph à passer par un tunnel vers un port *localhost* sur votre ordinateur.

Vous pouvez utiliser l’interface web ngrok ([http://127.0.0.1:4040](http://127.0.0.1:4040)) pour inspecter le trafic HTTP qui traverse le tunnel. Pour en savoir plus sur l’utilisation de ngrok, consultez le [site web ngrok](https://ngrok.com/).

1. Dans l’Explorateur de solutions, sélectionnez le projet **GraphWebhooks**.

1. Copiez le numéro de port de l’**URL** dans la fenêtre **Propriétés**. Si la fenêtre **Propriétés** ne s’affiche pas, sélectionnez **Afficher > Fenêtre Propriétés**.

    ![Numéro de port de l’URL dans la fenêtre Propriétés](readme-images/PortNumber.png)

1. [Télécharger ngrok](https://ngrok.com/download) pour Windows.

1. Décompressez le package et exécutez ngrok.exe.

1. Remplacez les deux valeurs d’espace réservé *{port-number}* dans la commande suivante par le numéro de port que vous avez copié, puis exécutez la commande dans la console ngrok.

    ```Shell
    ngrok http {port-number} -host-header=localhost:{port-number}
    ```

    ![Exemple de commande à exécuter dans la console ngrok](readme-images/ngrok1.PNG)

1. Copiez l’URL HTTPS affichée dans la console. Vous l’utiliserez pour configurer l’URL de notification dans l’exemple.

    ![L’URL HTTPS de transfert dans la console ngrok](readme-images/ngrok2.PNG)

    > **Remarque :** Gardez la console ouverte pendant le test. Si vous la fermez, le tunnel se ferme également et vous devrez générer une nouvelle URL et mettre à jour l’exemple.

Consultez [Hébergement sans tunnel](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Hosting-the-sample-without-a-tunnel) et [Pourquoi dois-je utiliser un tunnel ?](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Why-do-I-have-to-use-a-tunnel) pour plus d’informations.

## Configurer et exécuter l’exemple

1. Exposez un point de terminaison de notification HTTPS public. Il peut être exécuté sur un service tel que Microsoft Azure, ou vous pouvez créer un serveur proxy web en [utilisant ngrok](#set-up-the-ngrok-proxy-optional) ou un outil similaire.

1. Effectuez une copie de **GraphWebHooks/PrivateSettings. example.config** dans le même répertoire. Renommez la copie **PrivateSettings. config**.

1. Ouvrez **GraphWebhooks. sln** dans les exemples de fichiers.

    > **Remarque :** Vous devrez sans doute, à l’invite, approuver les certificats pour localhost.

1. Dans l’Explorateur de solutions, ouvrez le fichier **PrivateSettings.config** dans le répertoire racine du projet.
    - Pour la clé **ClientId**, remplacez *ENTER\_YOUR\_APP\_ID* par l’ID d’application de votre application inscrite.
    - Pour la clé **ClientSecret**, remplacez *ENTER\_YOUR\_SECRET* par la clé secrète de votre application inscrite.
    - Pour la clé **NotificationUrl**, remplacez *ENTER\_YOUR\_URL* par l’URL HTTPS. Conservez la partie */notification/Listen*. Si vous utilisez ngrok, utilisez l’URL HTTPS que vous avez copiée. La valeur se présente comme suit :

    ```xml
    <add key="ida:NotificationUrl" value="https://0f6fd138.ngrok.io/notification/listen" />
    ```

1. Assurez-vous que la console ngrok est encore en cours d’exécution, puis appuyez sur F5 pour générer et exécuter la solution en mode débogage.
    > **Remarque :** Si vous recevez des erreurs pendant l’installation des packages, vérifiez que le chemin d’accès local où vous avez sauvegardé la solution n’est pas trop long/profond. Pour résoudre ce problème, il vous suffit de déplacer la solution dans un dossier plus près du répertoire racine de lecteur.
    >
    > Si vous mettez à jour des dépendances pour cet exemple, **ne mettez pas à jour** `System.IdentityModel.Tokens.JWT` à la version 5, conçue pour être utilisée avec .NET Core. Par ailleurs, ne mettez pas à jour les bibliothèques `Microsoft.Owin` pour à la version 4.

### Utiliser l’application

1. Connectez-vous avec votre compte professionnel ou scolaire.

1. Accordez les autorisations **Lire vos e-mails** et **Vous connecter et lire votre profil**.

    Si l’autorisation **Lire vos e-mails** n’apparaît pas, sélectionnez **Annuler**, puis ajoutez l’autorisation **Lire les e-mails utilisateur** dans l’application dans le portail Azure. Pour obtenir des instructions, voir [Inscription de l’application](#register-the-app).

1. Sélectionnez le bouton **Créer un abonnement**. La page **Abonnement** se charge avec les informations relatives à l’abonnement.

    > **Remarque :** Cet exemple règle l’expiration de l’abonnement à 15 minutes à des fins de test.

    ![Page d’application montrant les propriétés du nouvel abonnement](readme-images/Page2.PNG)

1. Sélectionnez le bouton **Surveiller les notifications**.

1. Envoyez un e-mail à votre compte professionnel ou scolaire. La page **Notification** affiche certaines propriétés du message. La mise à jour de la page peut prendre plusieurs secondes.

    ![Page d’application montrant les propriétés du nouveau message](readme-images/Page3.PNG)

1. Sélectionnez le bouton **Supprimer un abonnement et se déconnecter**.

## Composants clés de l’exemple

### Contrôleurs

- [`NotificationController.cs`](GraphWebhooks/Controllers/NotificationController.cs) Reçoit des notifications.
- [`SubscriptionController.cs`](GraphWebhooks/Controllers/SubscriptionController.cs) Crée et reçoit des abonnements webhook.

### Modèles

- [`Message.cs`](GraphWebhooks/Models/Message.cs) Définit le **MessageViewModel** qui représente les données affichées dans la vue Notifications.
- [`Notification.cs`](GraphWebhooks/Models/Notification.cs) Représente une notification de modification.
- [`Subscription.cs`](GraphWebhooks/Models/Subscription.cs) Définit le **SubscriptionViewModel** qui représente les données affichées dans la vue Abonnement.

### Affichages

- [`Notification/Notification.cshtml`](GraphWebhooks/Views/Notification/Notification.cshtml) Affiche des informations sur les messages reçus et contient le bouton **Supprimer un abonnement et se déconnecter**.
- [`Subscription/index.cshtml`](GraphWebhooks/Views/Subscription/Index.cshtml) Page d’accueil contenant le bouton **Créer un abonnement**.
- [`Subscription/Subscription.cshtml`](GraphWebhooks/Views/Subscription/Subscription.cshtml) Affiche les propriétés de l’abonnement et contient le bouton **Surveiller les notifications**.

### Autres

- [`Web.config`](GraphWebhooks/Web.config) Contient les valeurs utilisées pour l’authentification et l’autorisation.
- [`App_Start/Startup.Auth.cs`](GraphWebhooks/App_Start/Startup.Auth.cs) Contient le code utilisé pour l’authentification et l’autorisation. L’exemple utilise [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-openid-connect-code) et la bibliothèque [Microsoft Authentication Library (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) pour authentifier et autoriser l’utilisateur.
- [`TokenStorage/SampleTokenCache. cs`](GraphWebhooks/TokenStorage/SampleTokenCache.cs) Exemple d’implémentation d’un cache de jeton qui utilise System.Runtime.Caching (pour que les informations du jeton soient disponibles lors de la réception d’une notification). En général, les applications de production utilisent une méthode de stockage persistant.
- [`Helpers/SubscriptionStore.cs`](GraphWebhooks/Helpers/SubscriptionStore.cs) Couche d’accès pour les informations stockées sur l’abonnement. L’exemple d’implémentation stocke temporairement les informations dans HttpRuntime.Cache. En général, les applications de production utilisent une méthode de stockage persistant.

## Résolution des problèmes

Si vous rencontrez des erreurs ou des problèmes liés à l’exemple, consultez le [document sur la résolution des problèmes](TROUBLESHOOTING.md).

## Contribution

Ce projet a adopté le [code de conduite Open Source de Microsoft](https://opensource.microsoft.com/codeofconduct/). Pour en savoir plus, reportez-vous à la [FAQ relative au code de conduite](https://opensource.microsoft.com/codeofconduct/faq/) ou contactez [opencode@microsoft.com](mailto:opencode@microsoft.com) pour toute question ou tout commentaire.

## Questions et commentaires

Nous serions ravis de connaître votre opinion sur l’exemple de Webhooks Microsoft Graph ASP.NET. Vous pouvez nous faire part de vos questions et suggestions dans la rubrique [Problèmes](https://github.com/microsoftgraph/aspnet-webhooks-rest-sample/issues) de ce référentiel.

Les questions générales sur Microsoft Graph doivent être publiées sur la page [Dépassement de capacité de la pile](https://stackoverflow.com/questions/tagged/MicrosoftGraph). Veillez à poser vos questions ou à rédiger vos commentaires en utilisant la balise `MicrosoftGraph`.

Si vous avez des suggestions de fonctionnalité, soumettez votre idée sur notre page [Voix utilisateur](https://officespdev.uservoice.com/) et votez pour votre suggestion.

## Ressources supplémentaires

- [Exemple de Webhooks Microsoft Graph Node.js](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample)
- [Utiliser des Webhooks dans Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)
- [Ressource abonnement](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/subscription)
- [Site des développeurs de Microsoft Graph](https://developer.microsoft.com/en-us/graph/)
- [Appel de Microsoft Graph dans une application ASP.NET MVC](https://developer.microsoft.com/en-us/graph/docs/platform/aspnetmvc)

## Copyright

Copyright (c) 2019 Microsoft. Tous droits réservés.
