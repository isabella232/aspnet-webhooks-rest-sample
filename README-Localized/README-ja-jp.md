---
page_type: sample
description: "この ASP.NET MVC サンプルでは、Microsoft Graph からの通知の受け取りを開始する方法を示します。"
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

[Microsoft Graph webhook](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks) をサブスクライブすると、ユーザーのデータが変更された場合に通知を受け取ることができ、変更内容についてポーリングを行う必要がなくなります。

この ASP.NET MVC サンプルでは、Microsoft Graph からの通知の取得を開始する方法を示します。Microsoft Graph は、Microsoft クラウドのデータにアクセスするための統合 API エンドポイントを提供します。

> このサンプルでは、Azure AD V2 エンドポイントを使用して職場または学校のアカウントのアクセス トークンを取得します。このサンプルではユーザーから委任されたアクセス許可が使用されますが、メッセージ、イベント、連絡先の各リソースではアプリケーション (アプリのみ) アクセス許可がサポートされています。現在、個人用アカウントのサブスクリプションは、ドライブのルート アイテム リソースでのみサポートされています。これらの機能やその他の機能のサポートを引き続き追加しますので、ドキュメントを確認するようにしてください。

アプリケーションが Webhook のサブスクリプションを使用して実行する一般的なタスクを次に示します。

- ユーザーのリソースをサブスクライブするための同意を取得し、アクセス トークンを取得する。
- アクセス トークンを使用して、リソースへの[サブスクリプションを作成](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscription_post_subscriptions)する。
- 検証トークンを送り返して通知 URL を確認する。
- Microsoft Graph からの通知をリッスンし、状態コード 202 で応答する。
- 通知内のデータを使用して、変更されたリソースの詳細情報を要求する。

このスクリーンショットは、ユーザーがサインインした後に表示されるアプリのスタート ページのものです。

![ASP.NET 用の Microsoft Graph Webhook サンプルのスクリーンショット](readme-images/Page1.PNG)

サインインしているユーザーのためにアプリがサブスクリプションを作成した後は、ユーザーのデータでイベントが発生すると、Microsoft Graph は登録済みエンドポイントに通知を送信します。これに対して、アプリがイベントに反応します。

このサンプルでは、`created` 変更について、`me/mailFolders('Inbox')/messages` リソースをサブスクライブします。ユーザーがメール メッセージを受け取るとアプリに通知が送信され、そのメッセージに関する情報を使用してアプリはページを更新します。

## 前提条件

Microsoft Graph ASP.NET Webhook サンプルを使用するには、以下のものが必要です。

- 開発用コンピューターにインストールされている Visual Studio 2017。
- [職場または学校のアカウント](http://dev.office.com/devprogram)。
- [アプリケーション登録ポータルに登録](#register-the-app)するアプリケーションのアプリケーション ID とキー。
- HTTP 要求を送受信するためのパブリック HTTPS エンドポイント。Microsoft Azure または別のサービスでエンドポイントをホストすることも、テスト中は [ngrok](#set-up-the-ngrok-proxy-optional) または同様のツールを使用することもできます。

### アプリを登録する
この演習では、Azure Active Directory 管理センターを使用して Azure AD Web アプリケーションの新規登録を作成します。

1. ASP.NET アプリの URL を決めます。Visual Studio のソリューション エクスプ ローラーで、**GraphWebhooks** プロジェクトを選択します。[**プロパティ**] ウィンドウで、**SSL URL** の値を見つけます。この値をコピーします。

    ![Visual Studio の [プロパティ] ウィンドウのスクリーンショット](readme-images/vs-project-url.PNG)

1. ブラウザーを開き、[Azure Active Directory 管理センター](https://aad.portal.azure.com)に移動します。**職場または学校のアカウント**を使用してログインします。

1. 左側のナビゲーションで [**Azure Active Directory**] を選択し、次に [**管理**] で [**アプリの登録**] を選択します。

    ![アプリの登録のスクリーンショット ](readme-images/aad-portal-app-registrations.png)

1. **[新規登録]** を選択します。[**アプリケーションの登録**] ページで、次のように値を設定します。

    - "`GraphWebhooks サンプル`" など、希望する [**名称**] を設定します。
    - [**サポートされているアカウントの種類**] を [**任意の組織のディレクトリ内のアカウント**] に設定します。
    - [**リダイレクト URI**] で、1 つ目のドロップダウン リストを [`Web`] に設定し、手順 1 でコピーした ASP.NET アプリの SSL URL に値を設定します。

    ![[アプリケーションを登録する] ページのスクリーンショット](readme-images/aad-register-an-app.png)

1. [**登録**] を選択します。[**GraphWebhooks サンプル**] ページで、[**アプリケーション (クライアント) ID**] の値をコピーして保存します。この値は次の手順で必要です。

    ![新しいアプリ登録のアプリケーション ID のスクリーンショット](readme-images/aad-application-id.PNG)

1. [**管理**] で [**認証**] を選択します。[**暗黙的な許可**] セクションを見つけ、[**ID トークン**] を有効にします。[**保存**] を選択します。

    ![[暗黙的な許可] セクションのスクリーンショット](readme-images/aad-implicit-grant.png)

1. [**管理**] で [**証明書とシークレット**] を選択します。[**新しいクライアント シークレット**] ボタンを選択します。[**説明**] に値を入力し、[**有効期限**] のオプションのいずれかを選び、[**追加**] を選択します。

    ![[クライアント シークレットの追加] ダイアログのスクリーンショット](readme-images/aad-new-client-secret.png)

1. このページを離れる前に、クライアント シークレットの値をコピーします。この値は次の手順で必要になります。

    > [重要!]
    > このクライアント シークレットは今後表示されないため、この段階で必ずコピーするようにしてください。

    ![新規追加されたクライアント シークレットのスクリーンショット](readme-images/aad-copy-client-secret.png)


### ngrok プロキシをセットアップする (省略可)

サブスクリプションを作成し、Microsoft Graph から通知を受信するには、パブリック HTTPS エンドポイントを公開する必要があります。テスト中は、ngrok を使用して Microsoft Graph からのメッセージをコンピューター上の *localhost* ポートにトンネリングすることを一時的に許可できます。

ngrok Web インターフェイス ([http://127.0.0.1:4040](http://127.0.0.1:4040)) を使用して、トンネルを通過する HTTP トラフィックを検査できます。ngrok の使用方法の詳細については、[ngrok の Web サイト](https://ngrok.com/)を参照してください。

1. ソリューション エクスプローラーで、[**GraphWebhooks**] プロジェクトを選択します。

1. [**プロパティ**] ウィンドウから [**URL**] のポート番号をコピーします。[**のプロパティ**] ウィンドウが表示されていない場合は、**[表示]、[プロパティ ウィンドウ]** の順に選択します。

    ![[プロパティ] ウィンドウ内の URL ポート番号](readme-images/PortNumber.png)

1. Windows 用の [ngrok をダウンロード](https://ngrok.com/download)します。

1. パッケージを展開し、ngrok.exe を実行します。

1. 次のコマンドの 2 つの *{port-number}* プレースホルダー値をコピーしたポート番号に置き換え、ngrok コンソールでコマンドを実行します。

    ```Shell
    ngrok http {port-number} -host-header=localhost:{port-number}
    ```

    ![ngrok コンソール で実行するコマンドの例](readme-images/ngrok1.PNG)

1. コンソールに表示される HTTPS URL をコピーします。このサンプルでは、これを使用して通知 URL を設定します。

    ![ngrok コンソールに表示される転送用 HTTPS URL](readme-images/ngrok2.PNG)

    > **注:**テスト中はコンソールを開いたままにします。コンソールを閉じるとトンネルも閉じられるため、新しい URL を生成してサンプルを更新する必要が生じます。

詳細については、「[Hosting without a tunnel (トンネルを使用しないでホストする)](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Hosting-the-sample-without-a-tunnel)」 および 「[Why do I have to use a tunnel? (トンネルを使用する理由)](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Why-do-I-have-to-use-a-tunnel)」 を参照してください。

## アプリを構成して実行する

1. 通知エンドポイントのパブリック HTTPS を公開します。Microsoft Azure などのサービスで HTTPS を実行することも、[ngrok](#set-up-the-ngrok-proxy-optional) または同様のツールを使用してプロキシ Web サーバーを作成することもできます。

1. **GraphWebHooks/PrivateSettings.example.config** のコピーを同じディレクトリに作成します。コピーの名前を **PrivateSettings.config** に変更します。

1. サンプル ファイルにある **GraphWebhooks.sln** を開きます。

    > **注:**Localhost の証明書を信頼するように求められる場合があります。

1. ソリューション エクスプローラーで、プロジェクトのルート ディレクトリにある **PrivateSettings.confi** ファイルを開きます。
    - **ClientId** キーは、*ENTER\_YOUR\_APP\_ID* を登録済みアプリケーションのアプリケーション ID で置き換えます。
    - **ClientSecret** キーは、*ENTER\_YOUR\_SECRET* を登録済みアプリケーションのシークレットで置き換えます。
    - **NotificationUrl** キーは、 *ENTER\_YOUR\_URL* を HTTPS URL で置き換えます。*/notification/listen* の部分は残します。ngrok を使用している場合は、コピーした HTTPS URL を使用します。値は次のようになります。

    ```xml
    <add key="ida:NotificationUrl" value="https://0f6fd138.ngrok.io/notification/listen" />
    ```

1. ngrok コンソールがまだ実行中であることを確認し、F5 キーを押してデバッグ モードでソリューションをビルドして実行します。
    > **注:**パッケージのインストール中にエラーが発生した場合は、ソリューションを保存したローカル パスが長すぎたり深すぎたりしていないかご確認ください。この問題は、ドライブのルート近くにソリューションを移動すると解決します。
    >
    > このサンプルのいずれかの依存関係を更新した場合、`System.IdentityModel.Tokens.Jwt` を v5 に**更新しないでください**。v5 は .NET Core で使用すように作られています。また、いずれの `Microsoft.Owin` ライブラリも v4 に更新しないでください。

### アプリを使用する

1. 職場または学校のアカウントを使用してサインインします。

1. [**メールの読み取り**] と [**サインインとユーザー プロファイルの読み取り**] の各アクセス許可に同意します。

    [**メールの読み取り**] アクセス許可が表示されない場合は [**キャンセル**] を選択し、Azure ポータルで [**ユーザーのメールの読み取り**] アクセス許可をアプリに追加します。手順については、「[アプリを登録する](#register-the-app)」セクションを参照してください。

1. [**サブスクリプションの作成**] ボタンを選択します。[**サブスクリプション**] ページに、サブスクリプションに関する情報が表示されます。

    > **注:**このサンプルでは、テスト用にサブスクリプションの有効期限を 15 分に設定します。

    ![新しいサブスクリプションのプロパティを表示するアプリのページ](readme-images/Page2.PNG)

1. [**Watch for notifications (通知を監視する)**] ボタンを選択します。

1. 職場または学校のアカウントにメールを送信します。[**通知**] ページには、いくつかのメッセージ プロパティが表示されます。ページの更新に数秒かかることがあります。

    ![新しいメッセージのプロパティを表示するアプリのページ](readme-images/Page3.PNG)

1. [**Delete subscription and sign out (サブスクリプションを削除してサインアウト)**] ボタンを選択します。

## サンプルの主要なコンポーネント

### コントローラー

- [`NotificationController.cs`](GraphWebhooks/Controllers/NotificationController.cs) 通知を受信します。
- [`SubscriptionController.cs`](GraphWebhooks/Controllers/SubscriptionController.cs) Webhook サブスクリプションを作成して受信します。

### モデル

- [`Message.cs`](GraphWebhooks/Models/Message.cs) Notification (通知) ビューに表示されるデータを表す **MessageViewModel** を定義します。
- [`Notification.cs`](GraphWebhooks/Models/Notification.cs) 変更通知を表します。
- [`Subscription.cs`](GraphWebhooks/Models/Subscription.cs) Subscription (サブスクリプション) ビューに表示されるデータを表す **SubscriptionViewModel** を定義します。

### ビュー

- [`Notification/Notification.cshtml`](GraphWebhooks/Views/Notification/Notification.cshtml) 受信したメッセージに関する情報を表示し、[**Delete subscription and sign out (サブスクリプションを削除してサインアウト)**] ボタンが含まれます。
- [`Subscription/Index.cshtml`](GraphWebhooks/Views/Subscription/Index.cshtml) [**Create subscription (サブスクリプションの作成)**] ボタンが含まれる、ランディング ページです。
- [`Subscription/Subscription.cshtml`](GraphWebhooks/Views/Subscription/Subscription.cshtml) サブスクリプションのプロパティを表示し、[**Watch for notifications**] ボタンが含まれます。

### その他

- [`Web.config`](GraphWebhooks/Web.config) 認証と承認に使用される値が含まれています。
- [`App_Start/Startup.Auth.cs`](GraphWebhooks/App_Start/Startup.Auth.cs) 認証と承認に使用されるコードが含まれています。サンプル アプリでは、[OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-openid-connect-code) と [Microsoft Authentication Library (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) を使用してユーザーの認証と承認を行います。
- [`TokenStorage/SampleTokenCache.cs`](GraphWebhooks/TokenStorage/SampleTokenCache.cs) (通知受信時にトークン 情報を利用できるように) System.Runtime.Caching を使用するトークン キャッシュのサンプル実装です。運用アプリでは通常、永続的なストレージのための何かしらの方法が使用されます。
- [`Helpers/SubscriptionStore.cs`](GraphWebhooks/Helpers/SubscriptionStore.cs) 格納されたサブスクリプション情報のアクセス層です。サンプル実装では、この情報を一時的に HttpRuntime.Cache に格納します。運用アプリでは通常、永続的なストレージのための何かしらの方法が使用されます。

## トラブルシューティング

サンプルでエラーや問題が発生した場合は、[トラブルシューティング ドキュメント](TROUBLESHOOTING.md)を参照してください。

## 投稿

このプロジェクトでは、[Microsoft Open Source Code of Conduct (Microsoft オープン ソース倫理規定)](https://opensource.microsoft.com/codeofconduct/) が採用されています。詳細については、「[Code of Conduct の FAQ (倫理規定の FAQ)](https://opensource.microsoft.com/codeofconduct/faq/)」を参照してください。また、その他の質問やコメントがあれば、[opencode@microsoft.com](mailto:opencode@microsoft.com) までお問い合わせください。

## 質問とコメント

Microsoft Graph ASP.NET Webhook のサンプルに関するフィードバックをぜひお寄せください。質問や提案は、このリポジトリの「[問題](https://github.com/microsoftgraph/aspnet-webhooks-rest-sample/issues)」セクションで送信できます。

Microsoft Graph 全般の質問については、「[Stack Overflow](https://stackoverflow.com/questions/tagged/MicrosoftGraph)」に投稿してください。質問やコメントには、必ず "`MicrosoftGraph`" とタグを付けてください。

機能に関して提案がございましたら、「[User Voice](https://officespdev.uservoice.com/)」ページでアイデアを投稿してから、その提案に投票してください。

## その他のリソース

- [Microsoft Graph Node.js Webhook のサンプル](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample)
- [Microsoft Graph の Webhook での作業](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)
- [サブスクリプション リソース](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/subscription)
- [Microsoft Graph 開発者向けサイト](https://developer.microsoft.com/en-us/graph/)
- [ASP.NET MVC アプリで Microsoft Graph を呼び出す](https://developer.microsoft.com/en-us/graph/docs/platform/aspnetmvc)

## 著作権

Copyright (c) 2019 Microsoft.All rights reserved.
