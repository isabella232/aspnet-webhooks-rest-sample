---
page_type: sample
description: "此 ASP.NET MVC 示例演示如何开始从 Microsoft Graph 获取通知。"
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
# Microsoft Graph ASP.NET Webhook

订阅当用户数据更改时通知 [Microsoft Graph Webhook](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)，使你无需轮询更改。

此 ASP.NET MVC 示例演示如何开始从 Microsoft Graph 获取通知。Microsoft Graph 提供一个统一的 API 终结点，用于从 Microsoft 云访问数据。

> 此示例使用 Azure AD V2 终结点获取工作或学校帐户的访问令牌。此示例使用用户委派的权限，但邮件、事件和联系人资源也支持应用程序（仅限应用）权限。目前，只有驱动器根项目资源支持个人帐户订阅。查看此文档，因为我们将会继续添加对这些以及其他功能的支持。

以下是应用程序可通过 Webhook 订阅执行的常见任务：

- 获得订阅用户资源的许可，然后获取访问令牌。
- 使用访问令牌为资源[创建订阅](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscription_post_subscriptions)。
- 回发验证令牌以确认通知 URL。
- 收听来自 Microsoft Graph 的通知并使用 202 状态代码进行响应。
- 请求与使用通知中的数据更改的资源相关的更多信息。

此屏幕截图显示了用户登录之后应用的开始页。

![Microsoft Graph ASP.NET Webhook 示例屏幕截图](readme-images/Page1.PNG)

应用代表已登录的用户创建订阅之后，当用户数据中发生事件时，Microsoft Graph 将向注册的终结点发送通知。应用随后会对事件作出回应。

本示例为`已创建`更改订阅了 `me/mailFolders('Inbox')/messages` 资源。它会在用户收到电子邮件时收到通知，然后使用与邮件相关的信息更新页面。

## 先决条件

若要使用 Microsoft Graph ASP.NET Webhook 示例，需执行以下操作：

- 在开发计算机上安装 Visual Studio 2017。
- 一个[工作或学校帐户](http://dev.office.com/devprogram)。
- [在应用程序注册门户](#register-the-app)上注册的应用程序的 ID 和密钥。
- 用于接收和发送 HTTP 请求的公共 HTTPS 终结点。你可以将此托管到 Microsoft Azure 或其他服务上，或者你可以在测试时[使用 ngrok](#set-up-the-ngrok-proxy-optional)或类似工具。

### 注册应用
在此练习中，你将使用 Azure Active Directory 管理中心创建新的 Azure AD Web 应用程序注册。

1. 确定 ASP.NET 应用的 URL。在 Visual Studio 的“解决方案资源管理器”中，选择 **GraphWebhooks** 项目。在**属性**窗口中，找到 **SSL URL** 的值。复制此值。

    ![Visual Studio 属性窗口的屏幕截图](readme-images/vs-project-url.PNG)

1. 打开浏览器，并转到 [Azure Active Directory 管理中心](https://aad.portal.azure.com)。使用**工作或学校帐户**登录。

1. 选择左侧导航栏中的**Azure Active Directory**，再选择**管理**下的**应用注册**。

    ![“应用注册”的屏幕截图](readme-images/aad-portal-app-registrations.png)

1. 选择**“新注册”**。在**注册应用**页上，按如下方式设置值。

    - 设置首选**名称**，如 `GraphWebhooks 示例`。
    - 将**受支持的帐户类型**设置为**任何组织目录中的帐户**。
    - 在**重定向 URI** 下，将第一个下拉列表设置为 `Web`，并将值设置为在第 1 步中复制的 ASP.NET 应用 SSL URL。

    ![“注册应用程序”页的屏幕截图](readme-images/aad-register-an-app.png)

1. 选择**注册**。在 **GraphWebhook 示例**页上，复制并保存**应用程序(客户端) ID** 的值，将在下一步中用到它。

    ![新应用注册的应用程序 ID 的屏幕截图](readme-images/aad-application-id.PNG)

1. 选择**管理**下的**身份验证**。找到**隐式授予**部分，并启用 **ID 令牌**。选择**保存**。

    ![“隐式授予”部分的屏幕截图](readme-images/aad-implicit-grant.png)

1. 选择**管理**下的**证书和密码**。选择**新客户端密码**按钮。在**说明**中输入值，并选择一个**过期**选项，再选择**添加**。

    ![“添加客户端密码”对话框的屏幕截图](readme-images/aad-new-client-secret.png)

1. 离开此页前，先复制客户端密码值。将在下一步中用到它。

    > [重要提示！]
    > 此客户端密码不会再次显示，所以请务必现在就复制它。

    ![新添加的客户端密码的屏幕截图](readme-images/aad-copy-client-secret.png)


### 设置 ngrok 代理（可选）

你必须公开一个公共的 HTTPS 终结点才能创建订阅并接收来自 Microsoft Graph 的通知。测试时，你可以使用 ngrok 临时允许消息从 Microsoft Graph 经隧道传输至计算机上的 *localhost* 端口。

你可以使用 ngrok Web 界面 ([http://127.0.0.1:4040](http://127.0.0.1:4040)) 检查流经隧道的 HTTP 流量。若要了解与使用 ngrok 相关的详细信息，请参阅 [ngrok 网站](https://ngrok.com/)。

1. 在“解决方案资源管理器”中，选择 **GraphWebhooks** 项目。

1. 复制**属性**窗口中的 **URL** 端口号。如果未显示**属性**窗口，请选择**查看 > 属性窗口**。

    ![“属性”窗口中的 URL 端口号](readme-images/PortNumber.png)

1. [下载 Windows 版 ngrok](https://ngrok.com/download)。

1. 解压包并运行 ngrok.exe。

1. 将以下命令中的两个 *{port-number}* 占位符值替换为所复制的端口号，然后在 ngrok 控制台中运行以下命令。

    ```Shell
    ngrok http {port-number} -host-header=localhost:{port-number}
    ```

    ![要在 ngrok 控制台中运行的示例命令](readme-images/ngrok1.PNG)

1. 复制控制台中显示的 HTTPS URL。你将使用它来配置示例中的通知 URL。

    ![ngrok 控制台中的转发 HTTPS URL](readme-images/ngrok2.PNG)

    > **注意：**测试时，请保持控制台处于打开状态。如果关闭，则隧道也会关闭，并且你需要生成新的 URL 并更新示例。

有关更多信息，请参阅[不使用隧道托管](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Hosting-the-sample-without-a-tunnel)和[为什么必须使用隧道？](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Why-do-I-have-to-use-a-tunnel)

## 配置并运行示例

1. 公开公共 HTTPS 通知终结点。它可以在 Microsoft Azure 等服务上运行，或者你可以通过[使用 ngrok](#set-up-the-ngrok-proxy-optional) 或类似工具创新代理 Web 服务器。

1. 在相同的目录中创建 **GraphWebHooks/PrivateSettings.example.config** 的副本。重命名副本 **PrivateSettings.config**。

1. 在示例文件中打开 **GraphWebhooks.sln**。

    > **注意：**系统可能会提示你信任 localhost 的证书。

1. 在“解决方案资源管理器”中，打开项目根目录下的 **PrivateSettings.config** 文件。
    - 对于 **ClientId** 密钥，请将 *ENTER_YOUR_APP_ID* 替换为所注册应用的应用程序 ID。
    - 对于 **ClientSecret** 密钥，请将 *ENTER_YOUR_SECRET* 替换为所注册应用的密码。
    - 对于 **NotificationUrl** 密钥，请将 *ENTER_YOUR_URL* 替换为 HTTPS URL。保留 */notification/listen* 部分。如果使用的是 ngrok，请使用复制的 HTTPS URL。值与以下类似：

    ```xml
    <add key="ida:NotificationUrl" value="https://0f6fd138.ngrok.io/notification/listen" />
    ```

1. 确保 ngrok 控制台仍在运行，然后按 F5 在调试模式下构建和运行解决方案。
    > **注意：**如果在安装包时出现任何错误，请确保你放置该解决方案的本地路径并未太长/太深。若要解决此问题，可以将解决方案移到更接近根驱动器的位置。
    >
    > 如果更新此示例中的任何依赖项，**请不要将** `System.IdentityModel.Tokens.Jwt` 更新为 v5，后者专用于与 .NET Core 搭配使用。此外，请不要将任何 `Microsoft.Owin` 库更新为 v4。

### 使用应用

1. 使用你的工作或学校帐户登录。

1. 同意**读取你的邮件**和**登录并读取个人资料**权限。

    如果未看到**读取你的邮件**权限，请选择**取消**，然后将**读取用户邮件**权限添加到 Azure 门户中的应用。有关说明，请参阅[注册应用](#register-the-app)。

1. 选择**创建订阅**按钮。**订阅**页中将会加载与订阅相关的信息。

    > **注意：**此示例将订阅过期时间设为 15 分钟，以供测试使用。

    ![显示新订阅的属性的应用页面](readme-images/Page2.PNG)

1. 选择**监视通知**按钮。

1. 向你的工作或学校帐户发送一封电子邮件。**通知**页面将显示部分邮件属性。页面更新可能需要几秒钟。

    ![显示新邮件的属性的应用页面](readme-images/Page3.PNG)

1. 选择**删除订阅并注销**按钮。

## 示例主要组件

### 控制器

- [`NotificationController.cs`](GraphWebhooks/Controllers/NotificationController.cs) 用于接收通知。
- [`SubscriptionController.cs`](GraphWebhooks/Controllers/SubscriptionController.cs) 用于创建和接收 Webhook 订阅。

### 模型

- [`Message.cs`](GraphWebhooks/Models/Message.cs) 用于定义表示“通知”视图中显示的数据的 **MessageViewModel**。
- [`Notification.cs`](GraphWebhooks/Models/Notification.cs) 用于表示更改通知。
- [`Subscription.cs`](GraphWebhooks/Models/Subscription.cs) 用于定义表示“订阅”视图中显示的数据的 **SubscriptionViewModel**。

### 视图

- [`Notification/Notification.cshtml`](GraphWebhooks/Views/Notification/Notification.cshtml) 用于显示与收到的邮件相关的信息，并且包含**删除订阅并注销**按钮。
- [`Subscription/Index.cshtml`](GraphWebhooks/Views/Subscription/Index.cshtml) 包含**创建订阅**按钮的登陆页面。
- [`Subscription/Subscription.cshtml`](GraphWebhooks/Views/Subscription/Subscription.cshtml) 用于显示订阅属性，并且包含**监视通知**按钮。

### 其他

- [`Web.config`](GraphWebhooks/Web.config) 包含用于身份验证和授权的值。
- [`App_Start/Startup.Auth.cs`](GraphWebhooks/App_Start/Startup.Auth.cs) 包含用于身份验证和授权的代码。本示例使用 [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-openid-connect-code) 和 [Microsoft 身份验证库 (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) 对用户进行身份验证和授权。
- [`TokenStorage/SampleTokenCache.cs`](GraphWebhooks/TokenStorage/SampleTokenCache.cs) 使用 System.Runtime.Caching 的令牌缓存示例实施（以便在收到通知时令牌消息可用）。通常情况下，产品应用将使用一些持久性存储方法。
- [`Helpers/SubscriptionStore.cs`](GraphWebhooks/Helpers/SubscriptionStore.cs) 已存储订阅信息的访问层。示例实施会将信息临时存储于 HttpRuntime.Cache 中。通常情况下，产品应用将使用一些持久性存储方法。

## 故障排除

如果运行示例时出现错误或问题，请参阅[故障排除文档](TROUBLESHOOTING.md)。

## 参与

此项目已采用 [Microsoft 开放源代码行为准则](https://opensource.microsoft.com/codeofconduct/)。有关详细信息，请参阅[行为准则 FAQ](https://opensource.microsoft.com/codeofconduct/faq/)。如有其他任何问题或意见，也可联系 [opencode@microsoft.com](mailto:opencode@microsoft.com)。

## 问题和意见

我们乐意倾听你有关 Microsoft Graph ASP.NET Webhook 示例的反馈。你可以在该存储库中的[问题](https://github.com/microsoftgraph/aspnet-webhooks-rest-sample/issues)部分将问题和建议发送给我们。

与 Microsoft Graph 相关的一般问题应发布到 [Stack Overflow](https://stackoverflow.com/questions/tagged/MicrosoftGraph)。请确保你的问题或意见标记有 `MicrosoftGraph`。

如果有功能建议，请将你的想法发布在我们的 [User Voice](https://officespdev.uservoice.com/) 页上，并为你的建议进行投票。

## 其他资源

- [Microsoft Graph Node.js Webhook 示例](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample)
- [在 Microsoft Graph 中使用 Webhook](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)
- [订阅资源](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/subscription)
- [Microsoft Graph 开发人员网站](https://developer.microsoft.com/en-us/graph/)
- [在 ASP.NET MVC 应用中调用 Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/platform/aspnetmvc)

## 版权信息

版权所有 (c) 2019 Microsoft。保留所有权利。
