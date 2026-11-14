<<<<<<< HEAD
# PIM-IV
=======
# FOODSMART — PIM IV

Sistema web para gestão de uma hamburgueria, com catálogo de produtos, autenticação de clientes, pedidos e painel administrativo.

## Tecnologias

- **Frontend:** HTML5, CSS3, JavaScript e Vite
- **Backend:** ASP.NET Core 8 (C#)
- **Persistência:** Entity Framework Core 8 e SQL Server
- **Documentação da API:** Swagger

> O projeto utiliza um token próprio assinado com HMAC-SHA256. Não é um JWT padrão.

## Estrutura

```text
PIM-III/
├── backend/
│   └── FoodSmart.Api/
│       ├── Contracts/
│       ├── Controllers/
│       ├── Data/
│       ├── Models/
│       ├── Services/
│       └── Program.cs
├── frontend/
│   ├── css/
│   ├── js/
│   └── *.html
├── foodsmart-full.sln
└── README.md
```

## Requisitos

- .NET SDK 8
- Node.js LTS e npm
- SQL Server Express ou outra instância SQL Server acessível
- SQL Server Management Studio (opcional, para administrar o banco)

## Configuração e execução local

### 1. Banco e backend

A configuração local padrão aponta para `localhost\SQLEXPRESS` e para o banco `FoodSmartDb`.

Se necessário, ajuste `ConnectionStrings:DefaultConnection` em `backend/FoodSmart.Api/appsettings.Development.json` ou defina a variável de ambiente `ConnectionStrings__DefaultConnection`.

O ambiente Development contém uma chave de autenticação e uma senha inicial de administrador destinadas **somente ao computador local**. Não publique esses valores nem use-os em produção.

No PowerShell:

```powershell
cd backend/FoodSmart.Api
dotnet restore
dotnet run
```

Para executar os testes automatizados da solução:

```powershell
dotnet test
```

A API local é iniciada em `http://localhost:5077`. A documentação Swagger fica em `/swagger`.

Na primeira execução, o backend cria o banco/tabelas com `EnsureCreated` e inclui produtos de demonstração. O administrador local é criado somente quando o banco não possui usuários.

**Acesso local de demonstração:**

- E-mail: `admin@foodsmart.com`
- Senha: `Admin@123`

Troque essa senha antes de qualquer demonstração compartilhada ou publicação.

### 2. Frontend

Em outro terminal:

```powershell
cd frontend
npm ci
npm run dev
```

Abra `http://localhost:5173`.

A URL da API é definida em `frontend/js/app.js` pela constante `API_BASE`. Para ambientes diferentes, ajuste essa configuração antes de publicar.

### 3. Build de produção do frontend

```powershell
cd frontend
npm ci
npm run build
```

O Vite gera a saída em `frontend/dist`.

## Funcionalidades existentes

- Cadastro e login de clientes
- Catálogo e consulta de produtos
- Criação de pedidos vinculados ao cliente autenticado
- Área do usuário e consulta dos próprios pedidos
- Painel administrativo
- Cadastro, edição e desativação lógica de produtos
- Consulta e atualização de status de pedidos
- Resumo de vendas e indicadores administrativos

## Configuração para produção

Configure as variáveis de ambiente no servidor. Não armazene segredos no repositório:

```text
Auth__Secret=<chave aleatória forte com pelo menos 32 bytes>
SeedAdmin__Password=<senha forte para o administrador inicial>
ConnectionStrings__DefaultConnection=<connection string do SQL Server>
```

`SeedAdmin__Password` é usado apenas se o banco ainda não possuir usuários. Se o banco já tiver sido inicializado, altere a senha do administrador pelo fluxo administrativo disponível no ambiente ou por procedimento seguro de administração do banco.

Também revise as origens permitidas na política CORS em `Program.cs`, habilite HTTPS e configure backups do SQL Server.

## Observações técnicas

- O projeto ainda usa `EnsureCreated`; migrations formais do EF Core são recomendadas para evolução controlada do banco no PIM IV.
- Pedidos criados sem conta não podem usar o endpoint de acompanhamento por ID. O acompanhamento foi restringido para evitar exposição de pedidos de convidados por enumeração de IDs.
- A pasta `bin`, `obj`, `node_modules` e `dist` é gerada localmente e não deve ser versionada.
- Não há implementação de machine learning no código atual; os indicadores do dashboard são cálculos descritivos.

## Próximas melhorias recomendadas para o PIM IV

1. Criar migrations do Entity Framework Core.
2. Implementar fluxo seguro de redefinição/alteração de senha.
3. Adicionar testes automatizados para autenticação, produtos e pedidos.
4. Separar o JavaScript do frontend em módulos por responsabilidade.
5. Implementar uma credencial própria para acompanhamento de pedidos de convidados, caso esse fluxo seja necessário.
6. Adicionar paginação e filtros para pedidos e produtos.


## Estado da refatoração técnica (10/11/2026)

- Tokens assinados agora carregam somente o identificador e a expiração; a função e a situação ativa são consultadas no SQL Server em cada requisição autenticada.
- O verificador PBKDF2 rejeita hashes malformados sem propagar erros de parsing.
- O cadastro valida nome, e-mail e senha; o papel padrão de novos usuários é `Cliente`.
- Pedidos rejeitam itens inválidos, quantidades fora do limite e dados de cliente incompatíveis; os preços são obtidos do banco e o total é calculado pela API.
- Mudanças de status seguem transições permitidas; estados `Concluído` e `Cancelado` são finais.
- Foram adicionadas restrições de modelo e índices. Para bancos já existentes, essas alterações só entram no SQL Server após uma migration ou script de atualização.

### Migração segura do banco existente

O banco atual foi criado com `EnsureCreated` e contém uma alteração manual de esquema. Não execute `EnsureCreated` como substituto de migrations e não apague o banco para iniciar migrations.

Procedimento planejado:
1. Faça backup completo de `FoodSmartDb` no SSMS.
2. Em uma cópia de desenvolvimento do banco, gere uma migration inicial usando o modelo atual e revise o SQL gerado.
3. Para um banco que já existe, crie uma migration de baseline sem recriar tabelas existentes; compare o esquema real com o modelo e registre a baseline somente depois de validar que as tabelas/índices/constraints correspondem.
4. Gere e revise um script idempotente (`dotnet ef migrations script --idempotent`) antes de executar no banco real.
5. Só depois de validar backup, baseline e script, substitua a inicialização por `Database.MigrateAsync()` em ambiente controlado.
6. Nunca misture `EnsureCreated` e migrations no mesmo banco.

**A migration/baseline não foi gerada automaticamente nesta entrega**, pois precisa ser comparada com o esquema SQL Server real para evitar perda de dados. Os check constraints e índices adicionados ao modelo ainda não são aplicados ao banco existente até essa etapa.

### Testes automatizados

Foi adicionado `tests/FoodSmart.Api.Tests`, com testes do hash de senha (hash válido, senha incorreta e formatos inválidos). Execute `dotnet test` após restaurar os pacotes NuGet.

### Refatoração do JavaScript

O `frontend/js/app.js` foi preservado para evitar regressão nas páginas já validadas manualmente. A divisão em módulos deve ser feita em etapa separada, com testes de regressão por página, pois as nove páginas compartilham inicialização e estado.
>>>>>>> 1e82c29 (feat: initial FOODSMART PIM IV)
