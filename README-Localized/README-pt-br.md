---
page_type: sample
description: "Este exemplo do ASP.NET MVC mostra como começar a obter notificações do Microsoft Graph."
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
# Webhooks do ASP.NET do Microsoft Graph

Inscreva-se no [Microsoft Graph Webhooks](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks) para ser notificado quando os dados do usuário forem alterados, para que você não precise fazer enquetes de alterações.

Este exemplo de ASP.NET MVC mostra como começar a receber notificações do Microsoft Graph. O Microsoft Graph oferece um ponto de extremidade de API unificado para acessar dados da nuvem da Microsoft.

> Este exemplo usa o ponto de extremidade do Azure AD V2 para obter um token de acesso para contas corporativas ou escolares. O exemplo usa uma permissão delegada pelo usuário, mas os recursos mensagens, eventos e contatos também oferecem suporte a permissões do aplicativo (somente de aplicativo). No momento, somente os recursos de itens raiz da unidade de disco oferecem suporte a assinaturas para contas pessoais. Assista aos documentos, uma vez que continuamos a adicionar suporte a estes e outros recursos.

A seguir, são apresentadas tarefas comuns que um aplicativo executa com assinaturas dos webhooks:

- Obtenha consentimento para se inscrever nos recursos de usuários e receber um token de acesso.
- Use o token de acesso para [criar uma assinatura](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/subscription_post_subscriptions) para um recurso.
- Devolva um token de validação para confirmar a URL da notificação.
- Ouça as notificações do Microsoft Graph e responda com o código de status 202.
- Solicite mais informações dos recursos alterados usando os dados da notificação.

Esta captura de tela mostra a página inicial do aplicativo quando o usuário entra.

![Captura de tela do exemplo de Webhook do Microsoft Graph para ASP.NET](readme-images/Page1.PNG)

Depois que o aplicativo cria uma assinatura para o usuário conectado, o Microsoft Graph envia uma notificação para o ponto de extremidade registrado quando acontecem eventos nos dados do usuário. Em seguida, o aplicativo reage ao evento.

Esse aplicativo de exemplo se inscreve no recurso `me/mailFolders('Inbox')/messages` para as alterações `criadas`. Ela é notificada quando o usuário recebe uma mensagem de email e, em seguida, atualiza uma página com informações sobre a mensagem.

## Pré-requisitos

Para usar o exemplo de Webhook do Microsoft Graph, você precisa do seguinte:

- Visual Studio 2017 instalado no computador de desenvolvimento.
- Uma [conta corporativa ou de estudante](http://dev.office.com/devprogram).
- A ID do aplicativo e a chave do aplicativo que você [registra no portal de registro do aplicativo](#register-the-app).
- Um ponto de extremidade do HTTPS público para receber e enviar solicitações HTTP. Você pode hospedar isso no Microsoft Azure ou em outro serviço, ou pode [usar o ngrok](#set-up-the-ngrok-proxy-optional) ou uma ferramenta semelhante durante o teste.

### Registrar o aplicativo
Neste exercício, você criará um novo registro de aplicativo Web do Azure AD usando o centro de administração do Azure Active Directory.

1. Determine a URL do aplicativo ASP.NET. No Gerenciador de Soluções do Visual Studio, selecione o projeto **GraphWebhooks**. Na janela **Propriedades**, encontre o valor da **URL SSL**. Copie esse valor.

    ![Captura de tela da janela de propriedades do Visual Studio](readme-images/vs-project-url.PNG)

1. Abra um navegador e navegue até o [centro de administração do Azure Active Directory](https://aad.portal.azure.com). Faça o login usando uma **Conta Corporativa ou de Estudante**.

1. Selecione **Azure Active Directory** na navegação à esquerda e, em seguida, selecione **Registros de aplicativos** em **Gerenciar**.

    ![Captura de tela dos Registros de aplicativo](readme-images/aad-portal-app-registrations.png)

1. Selecione **Novo registro**. Na página **Registrar um aplicativo**, defina os valores da seguinte forma.

    - Defina um nome **preferencial** por exemplo, `exemplo do GraphWebhooks`.
    - Defina os **tipos de conta com suporte** para **Contas em qualquer diretório organizacional**.
    - Em **URI de redirecionamento**, defina o primeiro menu suspenso como `Web` e defina o valor como a URL SSL do aplicativo ASP.NET que você copiou na etapa 1.

    ![Captura de tela da página registrar um aplicativo](readme-images/aad-register-an-app.png)

1. Escolha **Registrar**. Na página **Exemplo de GraphWebhooks**, copie o valor da **ID do aplicativo (cliente)** e salve-o, você precisará dele na próxima etapa.

    ![Captura de tela da ID do aplicativo do novo registro do aplicativo](readme-images/aad-application-id.PNG)

1. Selecione **Autenticação** em **Gerenciar**. Localize a seção **Concessão Implícita** e habilite **Tokens de ID**. Escolha **Salvar**.

    ![Uma captura de tela da seção Concessão Implícita](readme-images/aad-implicit-grant.png)

1. Selecione **Certificados e segredos** em **Gerenciar**. Selecione o botão **Novo segredo do cliente**. Insira um valor em **Descrição**, selecione uma das opções para **Expira** e escolha **Adicionar**.

    ![Uma captura de tela da caixa de diálogo Adicionar um segredo do cliente](readme-images/aad-new-client-secret.png)

1. Copie o valor secreto do cliente antes de sair desta página. Você precisará dele na próxima etapa.

    > [!IMPORTANTE]
    > Este segredo do cliente nunca é mostrado novamente, portanto, copie-o agora.

    ![Uma captura de tela do segredo do cliente recém adicionado](readme-images/aad-copy-client-secret.png)


### Configurar o proxy ngrok (opcional)

Você deve expor um ponto de extremidade HTTPS público para criar uma assinatura e receber as notificações do Microsoft Graph. Ao testar, você pode usar o ngrok para permitir temporariamente que as mensagens do Microsoft Graph sejam encapsuladas para uma porta *localhost* em seu computador.

Você pode usar a interface da Web do ngrok ([http://127.0.0.1:4040](http://127.0.0.1:4040)) para inspecionar o tráfego HTTP que passa pelo encapsulamento. Para saber mais sobre como usar o ngrok, confira o [site do ngrok](https://ngrok.com/).

1. No gerenciador de soluções, selecione o projeto **GraphWebhooks**.

1. Copie o  número da porta da**URL** na janela **Propriedades**. Se a janela **Propriedades ** não estiver sendo exibida, escolha **Exibir > Janela de propriedades**.

    ![O número da porta da URL na janela Propriedades](readme-images/PortNumber.png)

1. [Baixar ngrok](https://ngrok.com/download) para Windows.

1. Descompacte o pacote e execute ngrok.exe.

1. Substitua os dois valores de espaço reservado *{número da porta}* no seguinte comando abaixo com o número da porta que você copiou e execute o comando no console ngrok.

    ```Shell
    ngrok http {port-number} -host-header=localhost:{port-number}
    ```

    ![Exemplo do comando para executar no console ngrok](readme-images/ngrok1.PNG)

1. Copie a URL HTTPS exibida no console. Você usará essa configuração para definir a URL de notificação no exemplo.

    ![A URL HTTPS de encaminhamento no console ngrok](readme-images/ngrok2.PNG)

    > **Observação:** Mantenha o console aberto durante o teste. Caso feche-o, o túnel também é fechado, e você precisará gerar uma nova URL e atualizar o exemplo.

Confira [hospedagem sem um túnel](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Hosting-the-sample-without-a-tunnel) e [por que eu preciso usar um túnel?](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample/wiki/Why-do-I-have-to-use-a-tunnel) para saber mais.

## Configurar e executar o exemplo

1. Exponha um ponto de extremidade de notificação de HTTPS público. Ele pode ser executado em um serviço como o Microsoft Azure ou você pode criar um servidor Web proxy [usando o ngrok](#set-up-the-ngrok-proxy-optional) ou uma ferramenta semelhante.

1. Faça uma cópia do **GraphWebHooks/PrivateSettings. example.config** no mesmo diretório. Renomeie a cópia **PrivateSettings.config**.

1. Abra **GraphWebhooks.sln** nos arquivos de exemplo.

    > **Observação:** Pode ser que você receba uma solicitação de confiança no localhost.

1. No Gerenciador de Soluções, abra o arquivo **PrivateSettings.config** no diretório raiz do projeto.
    - Na chave **clientId**, substitua *ENTER\_YOUR\_APP\_ID* com a ID do aplicativo de seu aplicativo registrado.
    - Na chave **ClientSecret**, substitua *ENTER\_YOUR\_SECRET* com o segredo do aplicativo registrado.
    - Na chave **NotificationUrl**, substitua *ENTER\_YOUR\_URL* pela URL HTTPS. Mantenha a parte */notification/listen*. Se você estiver usando o ngrok, use a URL HTTPS que você copiou. O valor será parecido com isto:

    ```xml
    <add key="ida:NotificationUrl" value="https://0f6fd138.ngrok.io/notification/listen" />
    ```

1. Verifique se o console ngrok ainda está em execução e pressione F5 para criar e executar a solução no modo de depuração.
    > **Observação:** Caso receba mensagens de erro durante a instalação de pacotes, verifique se o caminho para o local onde você colocou a solução não é muito longo ou extenso. Para resolver esse problema, coloque a solução junto à raiz da unidade.
    >
    > Caso atualize as dependências desse exemplo, **não atualize** `System.IdentityModel.Tokens.Jwt` a V5, projetado para ser usado com o .NET Core. Além disso, não atualize nenhuma das `bibliotecas Microsoft.Owin` para v4.

### Usar o aplicativo

1. Entre com sua conta corporativa ou de estudante.

1. Consentimento para o **ler seu email** e permissões de**entrar e ler seu perfil**.

    Se você não vir a permissão **ler seu email**, escolha **cancelar** e, em seguida, adicione a permissão **ler email do usuário** para o aplicativo no portal do Azure. Confira a seção [registrar o aplicativo](#register-the-app) para obter instruções.

1. Escolha o botão **Criar assinatura**. A página **Assinatura** é carregada com as informações dessa assinatura.

    > **Observação:** Este exemplo define a expiração da assinatura para 15 minutos para fins de teste.

    ![Página do aplicativo mostrando as propriedades da nova assinatura](readme-images/Page2.PNG)

1. Escolha o botão **Ver as notificações**.

1. Entrar em sua conta corporativa ou de estudante. A página de **Notificação** exibe as propriedades da mensagem. Pode levar alguns segundos para que a página seja atualizada.

    ![Página do aplicativo mostrando as propriedades da nova mensagem](readme-images/Page3.PNG)

1. Escolha o botão **excluir assinatura e sair**.

## Componentes principais do exemplo

### Controladores

- [`NotificationController.cs`](GraphWebhooks/Controllers/NotificationController.cs) recebe as notificações.
- [`SubscriptionController.cs`](GraphWebhooks/Controllers/SubscriptionController.cs) cria e recebe assinaturas do webhook.

### Modelos

- [`Message.cs`](GraphWebhooks/Models/Message.cs) define a **MessageViewModel** que representa os dados exibidos no modo de exibição de notificação.
- [`Notification.cs`](GraphWebhooks/Models/Notification.cs) representa uma notificação de alteração.
- [`Subscription.cs`](GraphWebhooks/Models/Subscription.cs) define a **SubscriptionViewModel** que representa os dados exibidos no modo de exibição de assinatura.

### Exibições

- [`Notification/Notification.cshtml`](GraphWebhooks/Views/Notification/Notification.cshtml) exibe informações sobre as mensagens recebidas, e contém o botão **excluir assinatura e sair**.
- [`Assinatura/index.cshtml`](GraphWebhooks/Views/Subscription/Index.cshtml) página inicial que contém o botão **criar assinatura**.
- [`Assinatura/Subscription.cshtml`](GraphWebhooks/Views/Subscription/Subscription.cshtml) exibe as propriedades da assinatura e contém o botão **monitorar notificações**.

### Outros

- [`Web.config`](GraphWebhooks/Web.config) contém valores usados para autenticação e autorização.
- [`App_Start/Startup.Auth.cs`](GraphWebhooks/App_Start/Startup.Auth.cs) contém o código usado para autenticação e autorização. O exemplo usa o [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-openid-connect-code) e [biblioteca de autenticação da Microsoft (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) para autenticar e autorizar o usuário.
- [`TokenStorage/SampleTokenCache.cs`](GraphWebhooks/TokenStorage/SampleTokenCache.cs) implementação de exemplo de um cache de token que usa System.Runtime.Caching (para que as informações de token estejam disponíveis quando uma notificação é recebida). Os aplicativos de produção geralmente usam algum método de armazenamento persistente.
- [`Auxiliares/SubscriptionStore.cs`](GraphWebhooks/Helpers/SubscriptionStore.cs) camada de acesso para informações de assinatura armazenadas. O exemplo armazena temporariamente as informações em HttpRuntime.Cache. Os aplicativos de produção geralmente usam algum método de armazenamento persistente.

## Solução de problemas

Se você encontrar erros ou problemas com o exemplo, confira o [documento de solução de problemas](TROUBLESHOOTING.md).

## Colaboração

Este projeto adotou o [Código de conduta de software livre da Microsoft](https://opensource.microsoft.com/codeofconduct/). Para saber mais, confira as [Perguntas frequentes sobre o Código de Conduta](https://opensource.microsoft.com/codeofconduct/faq/) ou entre em contato pelo [opencode@microsoft.com](mailto:opencode@microsoft.com) se tiver outras dúvidas ou comentários.

## Perguntas e comentários

Adoraríamos receber os seus comentários sobre o exemplo de Webhooks do Microsoft Graph para ASP.NET. Você pode enviar perguntas e sugestões na seção [Problemas](https://github.com/microsoftgraph/aspnet-webhooks-rest-sample/issues) deste repositório.

Em geral, as perguntas sobre o Microsoft Graph devem ser postadas no [Stack Overflow](https://stackoverflow.com/questions/tagged/MicrosoftGraph). Não deixe de marcar as perguntas ou comentários com `MicrosoftGraph`.

Se você tiver uma sugestão de recurso, poste sua ideia na nossa página em [Voz do Usuário](https://officespdev.uservoice.com/) e vote em suas sugestões.

## Recursos adicionais

- [Exemplo de Microsoft Graph Node.js Webhooks](https://github.com/microsoftgraph/nodejs-webhooks-rest-sample)
- [Trabalhando com Webhooks no Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/webhooks)
- [Recurso da assinatura](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/subscription)
- [Site do desenvolvedor do Microsoft Graph](https://developer.microsoft.com/en-us/graph/)
- [Chamar o Microsoft Graph em um aplicativo do ASP.NET MVC](https://developer.microsoft.com/en-us/graph/docs/platform/aspnetmvc)

## Direitos autorais

Copyright (c) 2019 Microsoft. Todos os direitos reservados.
