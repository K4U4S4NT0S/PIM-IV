# Auditoria e refatoração inicial — FOODSMART / PIM IV

## Alterações aplicadas

### Segurança
- A consulta `GET /api/orders/{id}/status` agora exige autenticação e vínculo do pedido com o usuário, exceto para administrador. Antes, pedidos de convidados (`UserId` nulo) podiam ser consultados sem autenticação por tentativa de IDs.
- A leitura de tokens agora trata payloads malformados como inválidos, sem deixar exceções de parsing escaparem.
- Removido o segredo de autenticação de `appsettings.json`; a chave local ficou restrita a `appsettings.Development.json`.
- Removido o segredo alternativo fixo do código. A API exige `Auth:Secret` configurado com pelo menos 32 bytes.
- A senha do administrador inicial passou a ser configurável por `SeedAdmin:Password`. O valor de demonstração existe somente em configuração de Development.

### Indicadores
- O ticket médio agora considera somente pedidos não cancelados.
- O faturamento diário agora ordena pelas datas reais antes de formatar `dd/MM`, evitando ordenação alfabética incorreta.
- Produtos mais vendidos e horário de pico desconsideram pedidos cancelados.
- Removido o campo `forecastNextDayRevenue`, que era apenas uma média simples e não uma previsão estatística, e não era utilizado pelo frontend.

### Frontend e organização
- Incluída a página `usuario.html` na configuração de build multipágina do Vite.
- Reunidas as instruções de execução e configuração em um único `README.md`.
- Removidos READMEs antigos com instruções conflitantes sobre MySQL, dados em memória e configurações já substituídas.
- Removido um `package-lock.json` vazio na raiz; o lockfile válido do frontend foi preservado.
- Removido o modelo de `.env` do frontend que anunciava uma variável `VITE_API_URL` não consumida pelo JavaScript atual.

### Limpeza do pacote
- O ZIP de entrega não inclui históricos `.git`, dependências instaladas, caches, `bin`, `obj` ou `dist`. Esses itens podem ser recriados localmente e não são código-fonte do sistema.

## Itens preservados
- Páginas HTML existentes, incluindo `usuario.html` e `produto.html`.
- JavaScript e CSS atuais.
- Controllers, contratos, models, serviços e DbContext.
- Solução .NET, projeto backend e lockfile npm do frontend.
- Funcionalidades de catálogo, autenticação, pedidos, painel administrativo e analytics.

## Pontos que ainda precisam de trabalho no PIM IV
1. Criar migrations formais do Entity Framework Core; atualmente o projeto usa `EnsureCreated`.
2. Separar `frontend/js/app.js` em módulos menores por domínio (autenticação, catálogo, pedidos, painel e utilitários).
3. Criar fluxo de alteração e redefinição de senha.
4. Implementar testes automatizados para regras de autorização e operações de pedidos.
5. Se pedidos de convidados precisarem de acompanhamento, criar um token aleatório próprio de acompanhamento; não reabrir consulta pública por ID sequencial.
6. Revisar CORS, HTTPS, política de logs, backup e segredos no ambiente de implantação.
7. Avaliar paginação e filtros no painel administrativo.

## Validação realizada neste ambiente
- Sintaxe JavaScript validada com `node --check` para `app.js` e `vite.config.js`.
- JSON validado para `appsettings.json`, `appsettings.Development.json` e `package.json`.
- O build Vite não foi concluído: a instalação de dependências excedeu o tempo disponível e o executável local `vite` não ficou disponível.
- O build .NET não foi executado porque o SDK `dotnet` não está instalado neste ambiente.

**Importante:** esta entrega aplica correções específicas e limpeza estrutural; não deve ser considerada homologação completa. Execute `npm ci`, `npm run build`, `dotnet restore` e `dotnet build` no seu computador antes de substituir sua cópia principal.
