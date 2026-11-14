# 🍔 FOODSMART
### Plataforma integrada de gerenciamento e pedidos para hamburguerias

<p align="center">
  <strong>Gestão inteligente, experiência digital e tecnologia integradas.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Status-Em%20desenvolvimento-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-Backend-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img src="https://img.shields.io/badge/Flutter-Mobile-02569B?style=for-the-badge&logo=flutter&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-Containers-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
</p>

---

## 📌 1. Sobre o projeto

O **FOODSMART** é um sistema desenvolvido para auxiliar na gestão e operação de hamburguerias, reunindo recursos de gerenciamento administrativo, catálogo de produtos, autenticação de usuários e processamento de pedidos.

A proposta é oferecer uma solução integrada, permitindo que clientes realizem pedidos por meio de interfaces digitais, enquanto a administração possui recursos para gerenciar produtos e acompanhar as operações do estabelecimento.

O projeto utiliza uma arquitetura baseada em API REST, responsável por intermediar a comunicação entre as interfaces e o banco de dados.

O FOODSMART foi desenvolvido como parte do **Projeto Integrado Multidisciplinar IV (PIM IV)** do curso de Análise e Desenvolvimento de Sistemas.

## 🎯 2. Objetivos

- Digitalizar o processo de pedidos de uma hamburgueria.
- Centralizar o gerenciamento de produtos e pedidos.
- Facilitar o acesso dos clientes ao cardápio.
- Implementar autenticação e controle de usuários.
- Integrar aplicações Web e Mobile a uma API central.
- Utilizar banco de dados relacional para armazenamento das informações.
- Aplicar conceitos de arquitetura de software, segurança e containerização.

## 🚀 3. Funcionalidades

### 👤 Gestão de usuários
- Cadastro de usuários.
- Autenticação de acesso.
- Validação de credenciais.
- Gerenciamento de informações do perfil.
- Autenticação baseada em tokens.
- Diferenciação entre usuários e administradores.

### 🍔 Gestão de produtos
- Cadastro e gerenciamento de produtos.
- Consulta de produtos disponíveis.
- Organização do cardápio.
- Informações e valores dos produtos.

### 🛒 Gestão de pedidos
- Adição de produtos ao pedido.
- Registro de pedidos.
- Associação de pedidos aos usuários autenticados.
- Possibilidade de pedidos como convidados.
- Consulta do histórico de pedidos.
- Consulta de informações e status dos pedidos.

### 📊 Administração
- Gerenciamento de produtos.
- Consulta de informações operacionais.
- Endpoint de análise e resumo de informações.
- Organização das operações administrativas.

## 🧰 4. Tecnologias utilizadas

O projeto utiliza diferentes tecnologias, cada uma responsável por uma camada específica da aplicação.

### Backend

| Tecnologia | Aplicação |
|---|---|
| C# | Linguagem principal do backend |
| .NET 8 | Plataforma de desenvolvimento |
| ASP.NET Core | Construção da API REST |
| Entity Framework Core | Mapeamento objeto-relacional |
| SQL Server | Banco de dados relacional |
| Docker | Containerização da API |

**C#**

Linguagem utilizada na implementação das regras de negócio, modelos, serviços e controladores da aplicação.

**ASP.NET Core (.NET 8)**

Framework responsável pela construção da API REST, gerenciamento de requisições HTTP e integração entre as diferentes camadas do sistema.

**Entity Framework Core**

ORM utilizado para mapear as entidades C# para as tabelas do SQL Server, facilitando operações de consulta e persistência de dados.

### Frontend Web

| Tecnologia | Aplicação |
|---|---|
| HTML5 | Estrutura das páginas |
| CSS3 | Estilização e responsividade |
| JavaScript | Interatividade e comunicação com a API |
| Vite | Ambiente de desenvolvimento e build |

**HTML5:** estruturação das páginas e componentes da interface.

**CSS3:** desenvolvimento visual, organização dos elementos e adaptação das interfaces.

**JavaScript:** implementação das interações, validações e comunicação com os endpoints da API.

**Vite:** ferramenta utilizada para iniciar o ambiente de desenvolvimento e disponibilizar os recursos do frontend.

### Aplicativo Mobile

| Tecnologia | Aplicação |
|---|---|
| Flutter | Framework de desenvolvimento mobile |
| Dart | Linguagem de programação |
| Flutter Secure Storage | Armazenamento local seguro de credenciais |

O aplicativo mobile foi desenvolvido utilizando Flutter e Dart, consumindo os serviços disponibilizados pelo backend.

Entre os recursos implementados estão autenticação, cadastro, perfil do usuário, consulta de produtos, finalização de pedidos e histórico de compras.

**Observação:** o aplicativo mobile possui diretório de desenvolvimento separado e não está incluído na estrutura atual deste repositório.

### Banco de dados

**Microsoft SQL Server**

Banco relacional utilizado para armazenar e organizar as informações da aplicação.

**Entity Framework Core**

Responsável pela comunicação entre a aplicação C# e o banco de dados.

Também são utilizados recursos próprios do SQL Server, como relacionamentos, chaves estrangeiras, procedures e triggers.

### DevOps e ferramentas

| Ferramenta | Utilização |
|---|---|
| Docker | Criação e execução de containers |
| Git | Controle de versão |
| GitHub | Hospedagem do repositório |
| Visual Studio | Desenvolvimento do backend |
| Visual Studio Code | Desenvolvimento e edição de arquivos |
| SQL Server Management Studio | Administração do banco de dados |
| Android Studio | Ambiente de desenvolvimento e emulação mobile |

---

## 🏗️ 5. Arquitetura do sistema

O FOODSMART utiliza uma arquitetura organizada em camadas, separando as interfaces, a lógica de negócio e o armazenamento de dados.

```text
                    FOODSMART
                        |
           +------------+------------+
           |                         |
       FRONTEND WEB              MOBILE
     HTML/CSS/JavaScript       Flutter/Dart
           |                         |
           +------------+------------+
                        |
                     API REST
                   ASP.NET Core
                      .NET 8
                        |
              Entity Framework Core
                        |
                  SQL SERVER
                        |
          +-------------+-------------+
          |             |             |
        Users         Products       Orders
                                      |
                                  OrderItems
```

### Fluxo de funcionamento

1. O usuário acessa o sistema pelo frontend Web ou aplicativo Mobile.
2. A interface realiza uma requisição HTTP para a API.
3. O ASP.NET Core recebe e processa a requisição.
4. Os controladores executam as operações necessárias.
5. O Entity Framework Core realiza a comunicação com o SQL Server.
6. O banco de dados retorna as informações solicitadas.
7. A API devolve uma resposta para a interface.

## 📁 6. Estrutura do repositório

```text
PIM_IV/
│
├── backend/
│   └── FoodSmart.Api/
│       ├── Contracts/
│       ├── Controllers/
│       ├── Data/
│       ├── Models/
│       ├── Properties/
│       ├── Services/
│       ├── Dockerfile
│       ├── .dockerignore
│       ├── Program.cs
│       ├── appsettings.json
│       └── FoodSmart.Api.csproj
│
├── database/
│   ├── sql/
│   │   └── 01_atualizacao_segura.sql
│   └── README.md
│
├── frontend/
│   ├── css/
│   │   └── styles.css
│   ├── js/
│   │   └── app.js
│   ├── admin.html
│   ├── cadastro.html
│   ├── cardapio.html
│   ├── contato.html
│   ├── index.html
│   ├── login.html
│   ├── pedido.html
│   ├── produto.html
│   ├── usuario.html
│   ├── package.json
│   └── vite.config.js
│
├── tests/
│   └── FoodSmart.Api.Tests/
│       ├── FoodSmart.Api.Tests.csproj
│       └── PasswordHasherTests.cs
│
├── .env.example
├── .gitignore
├── AUDITORIA_PIM_IV.md
├── foodsmart-full.sln
└── scripts-start-dev.ps1
```

### Organização das pastas

**backend/**

Contém a API desenvolvida em C# e ASP.NET Core.

- `Contracts/`: contratos utilizados na comunicação e transferência de dados.
- `Controllers/`: controladores responsáveis pelas requisições HTTP.
- `Data/`: contexto de acesso ao banco de dados.
- `Models/`: entidades que representam os dados do sistema.
- `Services/`: serviços responsáveis por funcionalidades específicas.
- `Program.cs`: configuração e inicialização da aplicação.

**frontend/**

Contém a interface Web, com páginas HTML, arquivos CSS e scripts JavaScript.

**database/**

Contém os scripts SQL e orientações relacionadas à estrutura e atualização do banco de dados.

**tests/**

Contém o projeto de testes automatizados do backend.

---

# ⚙️ 7. Requisitos para instalação

Antes de executar o projeto, instale as ferramentas necessárias.

| Ferramenta | Versão / requisito |
|---|---|
| .NET SDK | 8.0 |
| SQL Server | Instância configurada |
| SQL Server Management Studio | Para administração do banco |
| Node.js | Versão compatível com o Vite |
| npm | Gerenciador de pacotes |
| Git | Controle de versão |
| Docker Desktop | Para execução em containers |
| Flutter SDK | Para o aplicativo mobile |
| Android Studio | Para emulação Android |

## 🔧 8. Configuração do banco de dados

O projeto utiliza Microsoft SQL Server como banco de dados principal.

### Etapa 1 — Instalar o SQL Server

Instale o SQL Server e configure uma instância local.

Também instale o SQL Server Management Studio para administrar o banco.

### Etapa 2 — Criar o banco

No SSMS, conecte-se à instância SQL Server e crie o banco:

```sql
CREATE DATABASE FoodSmartDb;
GO
```

### Etapa 3 — Configurar a conexão

A aplicação utiliza uma string de conexão para acessar o banco.

A configuração pode ser fornecida por variável de ambiente:

```text
ConnectionStrings__DefaultConnection
```

Exemplo para ambiente local com autenticação integrada:

```text
Server=localhost\SQLEXPRESS;Database=FoodSmartDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Ajuste o servidor e o método de autenticação conforme sua instalação.

### Etapa 4 — Scripts SQL

Os scripts complementares estão localizados em:

```text
database/sql/
```

O projeto também possui recursos de integridade e consulta no SQL Server, incluindo:

- Trigger `TR_Orders_ValidateStatusTransition`.
- Procedure `usp_FoodSmart_OrderSummary`.

Execute os scripts correspondentes no banco, quando necessário, seguindo a ordem indicada nos arquivos.

---

# 🖥️ 9. Executando o Backend

### Etapa 1 — Abrir o projeto

Abra o terminal na raiz do repositório e acesse:

```bash
cd backend/FoodSmart.Api
```

### Etapa 2 — Restaurar dependências

```bash
dotnet restore
```

O comando restaura os pacotes NuGet necessários para o projeto.

### Etapa 3 — Compilar a aplicação

```bash
dotnet build
```

Esse comando compila o backend e identifica possíveis erros de compilação.

### Etapa 4 — Configurar variáveis de ambiente

Configure as variáveis necessárias para a conexão com o banco e autenticação.

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection="SUA_STRING_DE_CONEXAO"
$env:Auth__Secret="SUA_CHAVE_SECRETA"
$env:SeedAdmin__Password="SUA_SENHA_DE_ADMIN"
```

Utilize valores próprios do ambiente local. Nunca publique credenciais reais no GitHub.

### Etapa 5 — Executar a API

```bash
dotnet run
```

A API será iniciada conforme as configurações de execução do projeto.

A porta e o endereço devem ser conferidos na saída do terminal ou nas configurações do ambiente.

---

# 🌐 10. Executando o Frontend Web

### Etapa 1 — Acessar a pasta

```bash
cd frontend
```

### Etapa 2 — Instalar dependências

```bash
npm install
```

O npm instala os pacotes definidos no `package.json`.

### Etapa 3 — Configurar a comunicação com a API

Confira o endereço da API utilizado pelo JavaScript do frontend.

A URL deve apontar para o ambiente em que o backend está sendo executado.

### Etapa 4 — Iniciar o ambiente de desenvolvimento

```bash
npm run dev
```

O Vite iniciará o servidor de desenvolvimento e exibirá no terminal o endereço local para acessar a aplicação.

---

# 📱 11. Aplicativo Mobile — Flutter

O aplicativo mobile foi desenvolvido separadamente utilizando Flutter e Dart.

### Etapa 1 — Instalar o Flutter

Instale o Flutter SDK e configure o ambiente de desenvolvimento.

Confira a instalação:

```bash
flutter doctor
```

### Etapa 2 — Acessar o projeto mobile

Abra o diretório do aplicativo Flutter em seu ambiente de desenvolvimento.

### Etapa 3 — Instalar dependências

```bash
flutter pub get
```

### Etapa 4 — Configurar a API

Para o emulador Android, o endereço local da API utilizada no desenvolvimento é:

```text
http://10.0.2.2:5077/api
```

Esse endereço permite que o emulador Android acesse o servidor executado na máquina hospedeira.

### Etapa 5 — Executar o aplicativo

```bash
flutter run
```

O Flutter compilará e iniciará o aplicativo no dispositivo ou emulador selecionado.

---

# 🐳 12. Executando com Docker

O backend possui um Dockerfile para criação de uma imagem da API.

### Etapa 1 — Instalar Docker Desktop

Instale e inicie o Docker Desktop.

Confira se o Docker está funcionando:

```bash
docker --version
```

### Etapa 2 — Acessar a pasta do backend

```bash
cd backend/FoodSmart.Api
```

### Etapa 3 — Construir a imagem

```bash
docker build -t foodsmart-api:1.0 .
```

O Docker utilizará as instruções do Dockerfile para compilar e criar a imagem da aplicação.

### Etapa 4 — Executar o container

Exemplo de execução:

```bash
docker run -d \
  --name foodsmart-api \
  -p 5077:8080 \
  -e "ConnectionStrings__DefaultConnection=SUA_STRING_DE_CONEXAO" \
  -e "Auth__Secret=SUA_CHAVE_SECRETA" \
  foodsmart-api:1.0
```

Ajuste a string de conexão para permitir que o container alcance o SQL Server no ambiente utilizado.

### Etapa 5 — Verificar o container

Listar containers em execução:

```bash
docker ps
```

Consultar os logs:

```bash
docker logs foodsmart-api
```

Parar o container:

```bash
docker stop foodsmart-api
```

Remover o container:

```bash
docker rm foodsmart-api
```

**Importante:** as variáveis de ambiente devem conter credenciais próprias do ambiente. Não utilize senhas reais diretamente nos comandos salvos em arquivos ou publicados no repositório.

---

# 🧪 13. Testes automatizados

O repositório possui um projeto de testes para o backend.

Localização:

```text
tests/FoodSmart.Api.Tests/
```

Para executar os testes da solução, utilize:

```bash
dotnet test
```

O comando executa os testes automatizados encontrados nos projetos de teste da solução.

---

# 🔐 14. Segurança

Algumas práticas de segurança adotadas no projeto:

- Separação entre controladores, serviços e modelos.
- Utilização de autenticação baseada em tokens.
- Serviço específico para geração e validação de tokens.
- Serviço dedicado à verificação de senhas.
- Armazenamento de credenciais mobile com Flutter Secure Storage.
- Configurações sensíveis fornecidas por variáveis de ambiente.
- Separação de arquivos locais e artefatos de compilação por meio do `.gitignore`.

**Atenção:** nunca publique senhas reais, chaves privadas, tokens ou strings de conexão com credenciais no GitHub.

---

# 👨‍💻 15. Equipe de desenvolvimento

Projeto desenvolvido em equipe como parte do Projeto Integrado Multidisciplinar IV.

| Integrante | Responsabilidades |
|---|---|
| Kauã Santos | Desenvolvimento Frontend e Backend, API REST, integração com SQL Server, Entity Framework Core e autenticação |
| Vitor Hugo | Desenvolvimento Frontend e UI/UX |
| Igor Bonfim | Modelagem Frontend e documentação |
| Pedro Abreu | Colaborador do projeto |

---

# 🎓 16. Contexto acadêmico

**Instituição:** Universidade Paulista — UNIP  
**Curso:** Análise e Desenvolvimento de Sistemas  
**Projeto:** Projeto Integrado Multidisciplinar IV — PIM IV  
**Sistema:** FOODSMART

---

# 📌 17. Considerações finais

O FOODSMART representa a aplicação prática de conhecimentos adquiridos durante a formação em Análise e Desenvolvimento de Sistemas.

O desenvolvimento envolve integração de aplicações, programação orientada a objetos, construção de APIs REST, modelagem e persistência de dados, desenvolvimento Web e Mobile e conceitos de DevOps.

A proposta é demonstrar como diferentes tecnologias podem trabalhar em conjunto para construir uma solução digital voltada às necessidades de um estabelecimento comercial.

---

<p align="center">
  <strong>FOODSMART</strong><br/>
  Tecnologia, praticidade e gestão em um só lugar.
</p>