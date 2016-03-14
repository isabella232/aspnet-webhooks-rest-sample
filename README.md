# Microsoft Graph ASP.NET Webhooks

This sample ASP.NET web application shows how to create subscriptions and receive notifications by using the [Subscription API](http://graph.microsoft.io/en-us/docs/api-reference/beta/resources/subscription). 

**Create subscriptions**

- Sign in and authenticate the user. The sample supports SSO and uses [OpenID Connect](https://msdn.microsoft.com/en-us/library/azure/jj573266.aspx) to make the OAuth2 call on behalf of a user.
- Build the `POST /subscriptions` request. The sample provides a UI to specify properties for the subscription.
- Send the `POST /subscriptions` request to Microsoft Graph.  
- Return the validation token to Microsoft Graph.  
- Parse the subscription object, and respond with a 200 HTTP status code.

**Receive notifications**

- Parse the notification object, and respond with a 200 HTTP status code.
- React to the notification. This sample sends a GET request to the resource that you were notified about. 

To learn more about Microsoft Graph webhooks, see [Subscription API](http://graph.microsoft.io/en-us/docs/api-reference/beta/resources/subscription). To learn more about using the Microsoft Graph API in an ASP.NET MVC app, see [Call Microsoft Graph in an ASP.NET MVC app](https://graph.microsoft.io/en-us/docs/platform/aspnetmvc).

## Prerequisites

To use the Microsoft Graph ASP.NET Webhooks sample, you need the following:

* Visual Studio 2015 installed on your development computer. 

//>This sample is written using Visual Studio 2015. If you're using Visual Studio 2013, make sure to change the compiler language version to 5 in the Web.config file:  **compilerOptions="/langversion:5**

* An Office 365 account. You can sign up for an [Office 365 Developer subscription](https://portal.office.com/Signup/Signup.aspx?OfferId=6881A1CB-F4EB-4db3-9F18-388898DAF510&DL=DEVELOPERPACK&ali=1#0) that includes the resources that you need to start building Office 365 apps.

>If you already have a subscription, the previous link sends you to a page with the message *Sorry, you can’t add that to your current account*. In that case use an account from your current Office 365 subscription.

* A Microsoft Azure tenant to register your application. Azure Active Directory (AAD) provides identity services that applications use for authentication and authorization. If you don't already have a tenant, you can [sign up for a trial subscription.](https://account.windowsazure.com/SignUp). 

>Important: Your Azure subscription must be bound to your Office 365 tenant. To do this, see [Manage the directory for your Office 365 subscription in Azure](https://azure.microsoft.com/en-us/documentation/articles/active-directory-manage-o365-subscription/) or [Associate your Office 365 account with Azure AD to create and manage apps](https://msdn.microsoft.com/office/office365/howto/setup-development-environment#bk_CreateAzureSubscription).

* The client ID, key, and other values from the application that you [register in the Azure Management Portal](https://msdn.microsoft.com/office/office365/HowTo/add-common-consent-manually#bk_RegisterWebApp) and [grant permissions](https://github.com/OfficeDev/O365-AspNetMVC-Microsoft-Graph-Connect/wiki/Grant-permissions-to-the-Connect-application-in-Azure). 

   Use *https://localhost:44300* as the sign-on and reply URLs, and grant the following delegated permissions to the **Microsoft Graph** application: 
   - **Read user mail** (Mail.Read)
   - **Read user calendars** (Calendar.Read)
   - **Read user contacts** (Contacts.Read)
 
![Delegating permissions to Microsoft Graph](./README assets/aspnet-webhooks-perms.PNG)

## Configure and run the app
1. Expose a public HTTPS notification endpoint. It can run on a service such as Microsoft Azure, or you can create a proxy web server by [using ngrok](#ngrok) or a similar tool.
2. Open **aspnet-webhooks.sln** in the sample files. 
3. In Solution Explorer, open the **Web.config** file in the root directory.  
   a. Replace *ENTER_YOUR_CLIENT_ID* with the client ID of your registered Azure application.  
   b. Replace *ENTER_YOUR_SECRET* with the key of your registered Azure application.  
   c. Replace *ENTER_YOUR_TENANT_ID* with the ID of your Azure tenant. You can find the ID by choosing **View Endpoints** from the drawer at the bottom of the portal. The ID is the GUID segment of the URLs.  ///just use common?
   d. Replace *ENTER_YOUR_ENDPOINT** with your public HTTPS notification endpoint. Keep the */notification/listen* portion.
4. Press F5 to build and run the solution in debug mode. Sign in to Office 365 with your organizational account.

<a name="ngrok"></a>
### Set up the ngrok proxy

You must expose a public HTTPS endpoint to create a subscription and receive notifications from Microsoft Graph. While testing, you can use ngrok to allow messages from Microsoft Graph to temporarily tunnel to your local port. This makes it easier to test and debug webhooks, and you can use the ngrok web interface to replay requests. See the [ngrok website](https://ngrok.com/) to learn more about using ngrok.  

You'll use the HTTPS Forwarding URL that ngrok provides in your notification endpoint.

1. [Download ngrok](https://ngrok.com/download) for Windows.  
2. Unzip the package and run ngrok.exe.  
3. Enter the following command: `ngrok http 61242 -host-header=localhost:61242`  
4. Copy the HTTPS Forwarding URL that's shown in the console to use in the notification URL for your webhooks subscription. It'll look something like this: `https://21698db0.ngrok.io` 
   
Keep the console open while testing. If you close it, the tunnel closes and you'll need to generate a new URL and update the sample.

>Note that you're using port number 62644, which is the sample's HTTP (not HTTPS) port. However, you'll use the HTTPS Forwarding URL that ngrok provides in your subscription request.

### Use the app 
1. On the *Create a subscription* page, choose your `POST /subscriptions` request properties and click **Create subscription**. After the subscription is created, the *Subscription request and response* page opens and displays the request message body and the response message body on.  
2. Click the **Open client** link to open the page that will display information about the changed resource. The sample uses SignalR to update this page after the notification is received.
3. Sign into Office 365 as the authenticated user and make a change that triggers a notification. For example, if you subscribed to *me/messages* and *Created* notifications, then send an email. 



## Key components of the sample

The following files contain code that pertains to the main purpose of the sample: creating subscriptions and receiving notifications.

**Controllers**  
- [```NotificationController.cs```](https://graph.microsoft.io). Receives notifications.  
- [```SubscriptionContoller.cs```](https://graph.microsoft.io). Creates and receives webhooks subscriptions.
 
**Models**  
- [```Notification.cs```](https://graph.microsoft.io). Represents a change notification. 
- [```Subscription.cs```](https://graph.microsoft.io). Represents a webhooks subscription. 
- [```SubscriptionViewModel.cs```](https://graph.microsoft.io). Represents the data that displays in the Subscription views. 

**Views**  
- [```Notification/Notification.cshtml```](https://graph.microsoft.io). Displays current notifications. 
- [```Subscription/CreateSubscription.cshtml```](https://graph.microsoft.io). Provides a UI to build subscription requests. 
- [```Subscription/SubscriptionRequestResponse.cshtml```](https://graph.microsoft.io). Displays the `POST /subscriptions` request and response message bodies. 

**Other**  
- [```Web.config```](https://graph.microsoft.io). Contains values used for authentication and authorization. 
- [```Startup.Auth.cs```](https://graph.microsoft.io). Contains code used for authentication and authorization when the app starts.

## Troubleshooting

| Issue | Resolution |
|:------|:------|
| The app opens to a *Server Error in '/' Application. The resource cannot be found.* browser page. | Make sure that a CSHTML view file isn't the active tab when you run the app from Visual Studio. |

## Questions and comments

 /// temp link

We'd love to get your feedback about the Microsoft Graph ASP.NET Webhooks sample. You can send your questions and suggestions to us in the [Issues](https://graph.microsoft.io) section of this repository.

Questions about Office 365 development in general should be posted to [Stack Overflow](http://stackoverflow.com/questions/tagged/Office365+API). Make sure that your questions or comments are tagged with [Office365] and [MicrosoftGraph].
  
## Additional resources

* [Microsoft Graph documentation](http://graph.microsoft.io)
* [Microsoft Graph API References](http://graph.microsoft.io/docs/api-reference/v1.0) \\beta?


## Copyright
Copyright (c) 2016 Microsoft. All rights reserved.

 