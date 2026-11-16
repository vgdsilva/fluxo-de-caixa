<!-- Cópia versionada do documento https://claude.ai/artifact/GKZVx7AcfFVif3F7oViXfC (16/11/2026). Ajustes de stack e decisões posteriores ficam nos ADRs em docs/adr/. -->

# Fluxo de Caixa: plano base do sistema

2026-11-16 · Vinicius Silva

## Resumo

O sistema existe para responder uma pergunta: **"quanto vai sobrar nos próximos meses, e cabe esta compra?"**. O controle do mês corrente vocês já fazem bem no papel; o que falta é a projeção, e ela depende de três coisas que a planilha não tinha: fatura de cartão com parcelas, recorrências (assinaturas, contas fixas) e o mínimo possível de digitação.

A proposta é um app web (Vue + NestJS) hospedado no notebook, com duas portas para o mesmo núcleo:

- **Visão operacional:** CRUDs, dashboards e uma visão 360 de cada pessoa e da casa, como no CRM.
- **Visão natural:** um chat com Claude que consulta e registra dados chamando as mesmas regras de negócio (tool calling), nunca escrevendo direto no banco.

**Fora do escopo inicial:** integração automática com banco (Open Finance), app mobile nativo, investimentos com cotação em tempo real e uso público. Isso pode vir depois, mas não deve moldar o MVP.

## Como vocês trabalham hoje

Os dois anexos são, na prática, dois artefatos diferentes que o sistema precisa reproduzir: um **orçamento compartilhado do mês** (o papel) e um **plano de distribuição do salário** (o .txt).

**O papel de novembro** lista as contas da casa e marca quem paga cada uma (R = Re, V = Vini, R+V = dividido):

| Item         | Valor (R$)    | Quem paga | Vira no sistema                                                          |
| ------------ | ------------- | --------- | ------------------------------------------------------------------------ |
| Condomínio   | 390           | Re        | Recorrência mensal da casa                                               |
| Tim          | 60            | Re        | Assinatura                                                               |
| Luz          | 150           | Vini      | Recorrência com valor variável (estimado)                                |
| IR           | 223           | Re        | Parcelamento com fim definido                                            |
| Unimed       | 300           | Re        | Assinatura                                                               |
| Gasolina     | 500           | Re + Vini | Despesa variável dividida 50/50                                          |
| Cartão Re    | 1.800         | Re        | Fatura projetada do cartão                                               |
| Cartão Vini  | 2.500         | Vini      | Fatura projetada do cartão                                               |
| Investimento | 1.000 + 1.000 | Re e Vini | Aporte em meta                                                           |
| Rino         | 600           | Re        | Despesa pontual com data, ou meta se for juntando antes (cirurgia da Re) |

Rendas: Re R$ 4.630 e Vini R$ 3.900, com sobra anotada de R$ 300 e R$ 600. Somando as linhas pela marcação R/V, as sobras não fecham exatamente com esses números, o que é justamente o tipo de conta que o sistema deve fazer sozinho.

**O .txt** é uma cascata ordenada a partir do salário líquido: primeiro despesas (luz, gasolina, cartão), depois investimentos (apartamento, reserva de emergência), depois uma folga para imprevistos (R$ 200), e o que sobra. Os dois blocos são do mesmo mês: duas entradas de dinheiro, cada uma com sua cascata. O segundo ainda compara dois cenários de parcela do apartamento (R$ 402,60 contra R$ 603,90), sobrando R$ 1.063,16 ou R$ 861,86.

Três conceitos saem daqui direto para o modelo:

1. **Fluxo compartilhado com responsável por item**, montado em cima dos dados que cada um escolhe compartilhar.
2. **Plano de pagamento (cascata)**: uma lista ordenada de destinos para cada entrada de salário, com o saldo corrente após cada degrau.
3. **Cenários**: comparar "e se" lado a lado sem sujar os dados reais, que é a mesma mecânica do "posso comprar isso?".

**A planilha antiga (FluxoCaixaVinicius.xlsx)** é uma matriz de linhas (renda, despesas) por 36 meses, de jan/2023 a dez/2025, com sobra do mês e saldo acumulado. Ela já tinha a ideia certa de olhar para frente, e mostra exatamente onde travou:

- **Cartão como número chutado:** R$ 900 fixos todo mês (R$ 1.300 em dezembro). Sem parcelas, a projeção fica cega para o que já está comprometido.
- **Eventos anuais e sazonais** digitados à mão na coluna certa: 13º (nov e dez), férias, IPVA, revisão do carro. No sistema, isso vira recorrência anual ou semestral.
- **Itens com fim** (faculdade até dez/2023, seguro do carro por 6 meses, parcela do carro) viram parcelamentos com data final, para a sobra subir sozinha quando acabam.
- **Vale alimentação (Flash) como renda:** é dinheiro que só serve para comida. Vira uma conta do tipo _benefício_, que paga a categoria alimentação mas não entra na sobra livre.
- **Investimento lançado como despesa:** no sistema, é uma transferência para uma conta de meta, assim a sobra cai mas o patrimônio não.
- **A aba de combustível** (dias úteis × km ÷ consumo × preço do litro) é uma _recorrência calculada_: o valor previsto sai de uma fórmula, não de um número fixo. Vale suportar isso desde o modelo, mesmo que a tela venha depois.

O saldo inicial da planilha (poupança de R$ 7.540 lançada como renda em janeiro) também mostra a falta de um conceito de **conta com saldo de abertura**, que o modelo abaixo resolve.

## Referências para se inspirar

Vale passar uma noite usando duas ou três destas antes de modelar: elas já resolveram muitos dos problemas abaixo e mostram o que copiar e o que evitar. Descrições de memória, vale conferir nos sites.

| Ferramenta                                      | Tipo                                               | O que copiar                                                                                             | O que não resolve para vocês                                                |
| ----------------------------------------------- | -------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| [Actual Budget](https://actualbudget.org)       | Open source, self-hosted                           | Orçamento por envelopes, regras de categorização na importação, transações agendadas                     | Cartão com fatura e parcelas brasileiras; projeção de meses futuros é fraca |
| [Firefly III](https://www.firefly-iii.org)      | Open source, self-hosted                           | Contas de ativo/passivo, regras automáticas, "piggy banks" (metas), API REST, importador de CSV separado | Interface pesada; parcelamento no cartão não é nativo                       |
| [Maybe](https://github.com/maybe-finance/maybe) | Open source (projeto encerrou o produto comercial) | Visão de patrimônio e um assistente de IA sobre os dados                                                 | Pouca coisa de orçamento compartilhado                                      |
| [Ghostfolio](https://ghostfol.io)               | Open source, self-hosted                           | Acompanhamento de investimentos                                                                          | Não faz fluxo de caixa                                                      |
| Organizze / Mobills                             | Comercial, brasileiro                              | Modelo de cartão com fechamento, vencimento e parcelas; previsão de saldo                                | Fechado, sem chat, sem divisão de casa do jeito de vocês                    |
| YNAB                                            | Comercial                                          | O método: "dê um destino a cada real", que é exatamente a sua cascata do .txt                            | Sem cartão parcelado, sem projeção longa                                    |

O nicho que nenhuma cobre bem, e que justifica construir: **cartão brasileiro com parcelas + divisão do casal + projeção de meses + chat**.

## Modelo de domínio

A decisão central: **todo dinheiro, passado ou futuro, é um lançamento no mesmo livro**, com status _previsto_ ou _realizado_. A projeção vira uma soma, não um segundo sistema. Recorrências, parcelas e cenários são apenas geradores de lançamentos previstos.

**Dono do dado é sempre o usuário, sem tenant.** Como no CRM, tudo gira em torno da pessoa: suas contas, cartões e lançamentos são só seus. Para olhar a casa, você cria um **fluxo compartilhado**, convida participantes por e-mail ou username, e cada um escolhe quais contas e cartões leva para aquele fluxo e, no momento de aceitar o convite, com que nível de detalhe (lançamentos ou só totais). Sair do fluxo ou deixar de compartilhar uma conta é imediato e não apaga nada.

O fluxo tem seus próprios itens (condomínio, luz, Unimed) e cada item tem um **responsável**, escolhido por vocês mês a mês, como no papel. O sistema não impõe regra de divisão; ele só mostra o que ajuda a decidir: quem recebe antes e o tamanho da fatura de cada um. Quando um item do fluxo é atribuído a você, ele vira um lançamento previsto na _sua_ projeção.

Compra no cartão fica sempre no cartão de quem comprou, mesmo que seja para a casa. No fluxo compartilhado, ela aparece dentro da fatura daquela pessoa, que é o item que o fluxo enxerga.

| Entidade                    | Para que serve                                                                                                   | Campos que importam                                                                                                     |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Fluxo compartilhado         | Visão que junta o que cada participante escolheu compartilhar (ex.: "Casa Re e Vini"). Não é dono de dado nenhum | Nome, criador, participantes, itens com responsável                                                                     |
| Usuário                     | Dono de todos os seus dados: contas, cartões, recorrências, metas                                                | Renda base, perfil de acesso                                                                                            |
| Conta                       | Onde o dinheiro está: corrente, carteira, poupança, investimento, benefício (VA/VR)                              | Tipo, dono (sempre um usuário), saldo inicial                                                                           |
| Cartão                      | Crédito com ciclo próprio                                                                                        | Dia de fechamento, dia de vencimento, limite, conta que paga a fatura, dono                                             |
| Fatura                      | Um ciclo do cartão; é ela que sai da conta corrente                                                              | Competência (mês), fechamento, vencimento, status aberta/fechada/paga, total                                            |
| Compra no cartão            | O gasto em si, com N parcelas                                                                                    | Data, valor total, parcelas, categoria                                                                                  |
| Parcela                     | Uma fatia da compra presa a uma fatura                                                                           | Número (3/10), valor, fatura                                                                                            |
| Lançamento                  | O livro: entradas, saídas, transferências                                                                        | Data de caixa, data de competência, valor, conta, categoria, status, origem                                             |
| Recorrência                 | Gera lançamentos previstos: salário, condomínio, Unimed, Tim, Netflix                                            | Frequência (mensal, anual, semestral), dia, valor fixo, estimado ou calculado por fórmula, início, fim, conta ou cartão |
| Parcelamento fora do cartão | IR, financiamento, apartamento                                                                                   | Valor da parcela, total de parcelas, primeira data                                                                      |
| Participação                | Quem está num fluxo e o que trouxe para ele                                                                      | Usuário, contas e cartões compartilhados, nível de detalhe, papel (criador ou participante), status do convite          |
| Categoria e tags            | Agrupar para orçamento e relatório                                                                               | Hierarquia de dois níveis                                                                                               |
| Envelope (orçamento)        | Teto por categoria no mês                                                                                        | Categoria, mês, valor planejado                                                                                         |
| Meta                        | Apartamento, reserva de emergência, viagem                                                                       | Valor alvo, data alvo, conta vinculada, aporte mensal                                                                   |
| Plano de pagamento          | A cascata do .txt, salva como modelo                                                                             | Entrada de origem, degraus ordenados (despesa, meta, folga), sobra                                                      |
| Cenário                     | "E se" isolado dos dados reais                                                                                   | Lançamentos hipotéticos, ativo ou não                                                                                   |

**Regras de negócio que valem ouro (e que a IA deve testar primeiro):**

- Uma compra feita **depois** do dia de fechamento cai na fatura do mês seguinte. Parcelas seguem mês a mês a partir dali.
- No fluxo de caixa, quem sai da conta é a **fatura** no vencimento. A compra conta como gasto da categoria na competência. Separar as duas datas evita contar o mesmo dinheiro duas vezes.
- Fatura fechada com valor real substitui a fatura projetada; a diferença vira alerta ("cartão veio R$ 300 acima do previsto").
- Recorrência com valor estimado (luz) usa a média dos últimos 3 realizados.
- Dinheiro em centavos inteiros no código e numeric no banco, nunca float; datas de negócio sem hora.
- Lançamento nunca é apagado de verdade: soft delete com histórico, para a auditoria e para o chat poder desfazer.

## Projeção e "posso comprar isso?"

A projeção é uma função pura: recebe o estado atual e uma janela (ex.: 12 meses) e devolve, por mês e por pessoa, quanto sobra. A simulação é a mesma função rodada duas vezes, com e sem a compra, e a resposta é a diferença.

**Como calcular cada mês:**

1. Saldo de abertura = saldo realizado das contas (mês atual) ou saldo de fechamento do mês anterior (meses futuros).
2. Expande as recorrências e os parcelamentos que caem no mês, como lançamentos previstos em memória (não grava nada).
3. Monta cada fatura do cartão: parcelas já contratadas + gasto médio não parcelado (média de 3 meses) + assinaturas no cartão.
4. Aplica a cascata de cada entrada de dinheiro e soma por usuário; um fluxo compartilhado soma o que seus participantes trouxeram.
5. Aplica o cenário ativo, se houver.

```latex
\text{Sobra}(m) = \text{Renda}(m) - \text{Fixas}(m) - \text{Faturas}(m) - \text{Aportes}(m) - \text{Folga}(m)
```

**A tela que resolve a dor principal** é o _comprometimento futuro do cartão_: para cada um dos próximos 12 meses, quanto já está comprometido só com parcelas que existem hoje. É isso que mostra quando uma "escada" de parcelas termina e libera espaço.

**"Posso comprar isso?"** recebe valor, número de parcelas, cartão (ou à vista) e data, e devolve:

- A sobra de cada mês afetado, antes e depois.
- O pior mês da janela e se ele fica abaixo do **colchão mínimo** que vocês definirem (ex.: R$ 500 por pessoa).
- Impacto nas metas: "o apartamento atrasa 2 meses" ou "a reserva não recebe aporte em março".
- Um veredito simples: **cabe**, **cabe apertado** (abaixo do colchão em algum mês) ou **não cabe** (sobra negativa).
- Sugestão de alternativa: "em 10x cabe, em 6x não" ou "se esperar até abril, cabe à vista".

Cenários salvos reaproveitam isso para o caso do .txt: "apartamento opção 1" e "opção 2" lado a lado nos próximos 24 meses.

## Menos digitação

O objetivo realista é digitar só o que é novo e imprevisível; o resto chega por recorrência, importação ou confirmação com um toque. Em ordem de esforço:

| Mecanismo                                                                            | Quanto tira do trabalho manual  | Esforço                        | Fase          |
| ------------------------------------------------------------------------------------ | ------------------------------- | ------------------------------ | ------------- |
| Recorrências com confirmação ("Condomínio R$ 390 venceu hoje: pago?")                | Todas as contas fixas do papel  | Baixo                          | 1             |
| Compra parcelada cadastrada uma vez, gera as N parcelas                              | Todo o cartão parcelado         | Baixo                          | 1             |
| Importar fatura do cartão em CSV / OFX, com conciliação contra o previsto            | O cartão à vista do mês inteiro | Médio                          | 2             |
| Importar extrato da conta em OFX                                                     | Pix, débitos, salário           | Médio                          | 2             |
| Regras de categorização ("contém UBER → Transporte") que aprendem com suas correções | A parte chata da importação     | Médio                          | 2             |
| Entrada pelo chat: "gastei 87 no mercado no cartão da Re"                            | O que sobrar                    | Médio                          | 3             |
| Foto do cupom ou print da fatura lido pelo Claude (visão)                            | Quando não há CSV               | Médio                          | 3             |
| PDF da fatura lido pelo Claude, quando o banco só dá PDF                             | Cartões sem exportação          | Médio                          | 3             |
| Open Finance (agregadores como Pluggy ou Belvo)                                      | Quase tudo                      | Alto, e tem custo e burocracia | Depois do MVP |

Na importação, a conciliação é o que importa: cada linha do arquivo casa com um lançamento previsto (mesmo valor, data próxima, descrição parecida) e o promove a realizado; o que não casa entra como novo; nada é duplicado se o mesmo arquivo for importado duas vezes (hash por linha).

**Bancos de vocês:** Itaú, Nubank e Inter (Vini), Nubank e Bradesco (Re). Como o Nubank é comum aos dois, ele é o primeiro importador a construir. Antes da fase 3, vale exportar uma fatura e um extrato de cada banco e guardar como arquivos de teste (anonimizados), porque o formato de cada um muda e é isso que a IA vai usar para escrever os parsers.

## Visão operacional e visão natural

As duas visões são portas para a **mesma camada de aplicação** (comandos e consultas). A tela chama `RegistrarCompraCartao`; o chat chama a mesma coisa via tool. Assim, regra de negócio, permissão e auditoria valem igual para os dois, e a IA nunca toca no banco.

**Visão operacional (telas):**

| Tela                       | O que mostra                                                                                                    |
| -------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Painel do mês              | Cascata do salário ao vivo: entradas, contas pagas e a pagar, sobra prevista e realizada                        |
| Projeção 12 meses          | Sobra por mês e por pessoa, com o colchão mínimo marcado; clique em um mês abre o detalhe                       |
| Cartões                    | Fatura atual, próximas faturas, comprometimento futuro por parcelas, limite livre                               |
| Visão 360 da pessoa        | Renda, contas, cartões, metas, o que assume em cada fluxo compartilhado, últimos lançamentos, alertas           |
| Fluxo compartilhado        | O papel de novembro digitalizado: itens, responsável de cada um, sobra de cada participante, gestão de convites |
| Recorrências e assinaturas | Tudo que se repete, com custo mensal e anual somado ("assinaturas custam R$ 4.200/ano")                         |
| Metas                      | Progresso, aporte mensal, data prevista de conclusão                                                            |
| Simulador                  | "Posso comprar?" e cenários lado a lado                                                                         |
| Importação                 | Upload, conciliação, revisão de categorias                                                                      |
| Admin                      | Usuários, casas, perfis, logs de auditoria, uso e custo da IA                                                   |

**Visão natural (chat):** Claude via API com _tool calling_. O backend define as ferramentas, o modelo decide qual chamar, o backend executa com as permissões do usuário logado e devolve o resultado.

| Ferramenta                | Tipo    | Exemplo de pedido                                    |
| ------------------------- | ------- | ---------------------------------------------------- |
| `consultar_lancamentos`   | Leitura | "Quanto gastamos de mercado em outubro?"             |
| `projetar_meses`          | Leitura | "Vou conseguir guardar em janeiro?"                  |
| `simular_compra`          | Leitura | "Cabe uma TV de 3 mil em 10x no cartão da Re?"       |
| `resumo_cartao`           | Leitura | "Quando termina a escada de parcelas do meu cartão?" |
| `listar_recorrencias`     | Leitura | "Quanto pago de assinatura por mês?"                 |
| `registrar_lancamento`    | Escrita | "Gastei 87 no mercado no débito"                     |
| `registrar_compra_cartao` | Escrita | "Comprei um tênis de 600 em 3x no Nubank"            |
| `criar_recorrencia`       | Escrita | "Assinei o Spotify família, 34,90 todo dia 5"        |
| `marcar_pago`             | Escrita | "Paguei o condomínio"                                |

Regras do chat:

- **Escrita sempre com confirmação:** a tool de escrita devolve um rascunho e a interface mostra um cartão "Confirmar / Editar / Cancelar". Só grava depois do clique. Isso também resolve ambiguidade ("qual cartão?").
- **Números vêm das tools, não do modelo:** a IA explica e resume, mas todo valor exibido saiu de uma consulta. Instrua isso no prompt e mostre o resultado da tool na interface.
- **SDK:** o SDK oficial em TypeScript (pacote `@anthropic-ai/sdk no npm`) tem tool calling e um _tool runner_ que roda o loop por você. Streaming na resposta para a UX do chat.
- **Entrada por imagem e PDF:** o mesmo chat aceita foto de cupom ou PDF de fatura; o modelo extrai e devolve rascunhos de lançamentos para confirmar em lote.
- **Privacidade:** os dados enviados à API saem do notebook. Envie só o necessário (descrição, valor, data, categoria), nunca CPF, número de cartão ou senha.

**Custo mínimo (decisão):** o sistema funciona inteiro sem IA; o chat é um atalho, não dependência.

- **Modelo:** Claude Haiku 5.5 (`claude-haiku-5-5`), o mais barato da linha atual, para o chat e para categorizar. Um modelo maior só se algum caso específico falhar, e trocar é uma configuração.
- **Código antes da IA:** categorização por regras e conciliação por valor e data são determinísticas; a IA só entra no que sobrar.
- **Prompt caching:** prompt de sistema e lista de tools fixos e no início, para serem cobrados com desconto a partir da segunda pergunta.
- **Contexto enxuto:** histórico curto por conversa, dados só via tool, resultados de tool resumidos (totais, não 500 linhas).
- **Teto mensal** configurável no app (ex.: US$ 2) e limite na conta da Anthropic; ao bater o teto, o chat desliga e as telas seguem normais.
- **Ordem de grandeza (estimativa, não medida):** com Haiku 5.5, uma pergunta com algumas chamadas de tool deve custar frações de centavo de dólar; algumas centenas de perguntas por mês devem ficar na casa de 1 dólar. Registre tokens e custo por conversa desde o dia 1 para confirmar.

## Stack e arquitetura

Decisão: **monorepo TypeScript com front em Vue 3 e back em NestJS, um container de app, um banco, tudo em Docker Compose no notebook.** Para poucos usuários num hardware velho, um processo só basta; módulos bem separados dão organização e facilitam o trabalho da IA (contexto pequeno por módulo). Detalhes na spec da fase 0.

> _(Diagrama no documento original: arquitetura no notebook · duas portas, uma camada de aplicação)_

O chat nunca fala com o banco: ele pede ao Claude qual ferramenta usar e executa os mesmos comandos que as telas, com as permissões de quem está logado.

| Camada                    | Escolha                                                       | Por quê                                                                                                         |
| ------------------------- | ------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Runtime                   | Node 24 LTS                                                   | Suporte longo                                                                                                   |
| Front-end                 | Vue 3 + Vite + Pinia + PrimeVue, como PWA (`vite-plugin-pwa`) | SPA instalável no iPhone, abre sem sinal e volta do segundo plano sem reconectar                                |
| Back-end                  | NestJS 11                                                     | Módulos, injeção de dependência, guards e interceptors: a estrutura mais próxima do ASP.NET no mundo Node       |
| Contratos                 | Pacote `contracts` com schemas zod                            | Mesmo tipo e mesma validação no front e no back                                                                 |
| Banco                     | PostgreSQL em container                                       | Robusto, bom com concorrência e JSON                                                                            |
| Acesso a dados            | MikroORM + migrations                                         | Unit of work e change tracking como o EF Core, filtros globais e subscribers para dono, soft delete e auditoria |
| Jobs                      | `@nestjs/schedule`                                            | Gerar previstos, fechar faturas, mandar alertas                                                                 |
| Logs                      | pino + Seq                                                    | Logs estruturados com busca, roda em container                                                                  |
| IA                        | SDK oficial `@anthropic-ai/sdk`                               | Tool calling, tool runner com schemas zod e streaming                                                           |
| Testes (depois da fase 0) | Jest/Vitest + Testcontainers                                  | Regras de cartão e projeção testadas contra Postgres real                                                       |
| Deploy                    | Docker Compose + Tailscale Funnel                             | Um `docker compose up` sobe tudo                                                                                |

**Módulos sugeridos (Nest no back, pastas de feature no front):** Identidade e Admin, Contas e Lançamentos, Cartões, Recorrências, Orçamento e Metas, Projeção e Cenários, Fluxos compartilhados, Importação, Assistente (chat). Cada módulo expõe serviços de aplicação; nenhum lê a tabela do outro direto.

**Cuidados com o notebook velho:** desligar suspensão ao fechar a tampa, deixar na tomada com a bateria limitada (ou sem bateria), Linux leve (Ubuntu Server ou Debian), e backup noturno do banco criptografado para fora da casa (ex.: `pg_dump` + restic para um bucket). Se o notebook morrer, o restore precisa ser um comando documentado.

## Segurança, usuários e acessos

Decisão: **o Tailscale fica no notebook e o acesso é pelo Funnel**, então o app fica na internet aberta para qualquer pessoa com a URL, sem precisar de Tailscale nos celulares. Isso é prático, mas faz de todas as camadas abaixo requisitos do dia 1, não melhorias futuras. Se um dia quiser fechar, o **Tailscale Serve** deixa o app visível só dentro da tailnet.

**Camadas no app (valem nos dois casos):**

- **Cadastro só por convite** gerado pelo admin; sem tela de "criar conta" aberta.
- **Passkey ou 2FA (TOTP) obrigatório**, bloqueio após tentativas erradas e rate limiting no login.
- **Isolamento por usuário:** toda entidade tem `OwnerId`. Um filtro global do ORM devolve só o que é do usuário ou o que foi compartilhado com ele num fluxo do qual participa, respeitando o nível de detalhe. Testes automatizados tentam ler dados alheios e precisam falhar.
- **Compartilhamento sempre opt-in:** convite por e-mail ou username precisa ser aceito, e quem compartilha escolhe conta por conta. Participante vê o fluxo, mas só o dono altera seus próprios lançamentos.
- **Perfis do sistema:** Admin (você: usuários, convites, logs, custo de IA) e Usuário. Dentro de um fluxo: criador (edita itens e convida) e participante.
- **Auditoria:** quem mudou o quê, quando, por qual canal (tela, chat, importação), com antes e depois. É a mesma tabela que permite desfazer uma ação do chat.
- **Segredos** (chave da API da Anthropic, senha do banco) em variáveis de ambiente ou Docker secrets, nunca no repositório.
- **Limite de gasto da IA** por usuário e por mês no app, além do limite na conta da Anthropic.
- **Banco sem porta exposta**, só acessível pela rede interna do Compose; sistema operacional com atualizações automáticas de segurança; backup testado com restore.

## Roadmap do MVP

O MVP está pronto quando o **papel de novembro e a cascata do .txt vivem no sistema e ele responde "cabe esta compra?"**. O chat entra depois do núcleo estar confiável, porque ele só é tão bom quanto as consultas que chama. Cada fase termina com algo que vocês usam de verdade.

| Fase          | Entrega                                                                                                                                                                           | Pronto quando                                                      |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| 0. Fundação   | Solução e módulos, Docker Compose, Identity com convite e 2FA, perfis, auditoria, CI com testes, deploy no notebook via Tailscale                                                 | Você e a Re entram pelo celular                                    |
| 1. Núcleo     | Contas, categorias, lançamentos previsto/realizado, recorrências, cartões com fatura e parcelas, fluxo compartilhado com convite por e-mail e responsável por item, painel do mês | Novembro do papel reproduzido e os totais conferem                 |
| 2. Futuro     | Projeção 12 meses, comprometimento do cartão, metas, cascata do salário, simulador e cenários                                                                                     | A próxima compra grande é decidida pelo sistema, não pela planilha |
| 3. Importação | CSV/OFX de fatura e extrato, conciliação com o previsto, regras de categoria                                                                                                      | Um mês inteiro de cartão fechado só importando                     |
| 4. Chat       | Tools de leitura, depois escrita com confirmação, depois foto e PDF                                                                                                               | A Re registra um gasto pelo chat sem abrir tela                    |
| 5. Extras     | Alertas (Telegram ou e-mail), painel de admin e custo de IA, convite para amigos                                                                                                  | Conforme a vontade                                                 |

**Como trabalhar com IA neste projeto:**

1. **Este documento vira `docs/` no repositório**, e um `CLAUDE.md` na raiz resume: glossário do domínio (fatura, competência, previsto, cascata), regras de negócio, convenções de código e comandos de build e teste.
2. **Regras de negócio primeiro como testes.** Peça à IA os testes do fechamento de fatura, das parcelas e da projeção antes da implementação; revise os testes, não o código inteiro. O _golden test_ é o mês de novembro: entra o papel, sai a sobra de cada um.
3. **Fatias verticais pequenas:** uma funcionalidade por sessão (entidade + comando + tela + teste), um PR por fatia. Sessões curtas com contexto focado erram menos.
4. **Decisões em ADRs curtos** (`docs/adr/0001-postgres.md`), para a IA não "redecidir" a arquitetura a cada sessão.
5. **Dados de exemplo versionados** (seed com a sua casa fictícia) para testar telas e o chat sem usar dados reais.

## Decisões tomadas

Todas as perguntas em aberto foram respondidas e o plano já reflete cada uma.

| Tema                | Decisão                                                                                |
| ------------------- | -------------------------------------------------------------------------------------- |
| Dono dos dados      | Cada usuário, sem tenant                                                               |
| Visão da casa       | Fluxo compartilhado por convite (e-mail ou username)                                   |
| Nível de detalhe    | Escolhido por quem compartilha, ao aceitar o convite                                   |
| Quem paga cada item | Escolha de vocês (quem recebe antes, quem tem fatura menor); o sistema não impõe regra |
| Compras no cartão   | Ficam no cartão de quem comprou                                                        |
| .txt                | Um mês, duas entradas de dinheiro, cada uma com sua cascata                            |
| Rino                | Cirurgia da Re, R$ 600, despesa pontual ou meta                                        |
| Acesso              | Tailscale no notebook, Funnel aberto, segurança completa no dia 1                      |
| Bancos              | Itaú, Nubank, Inter (Vini); Nubank, Bradesco (Re); Nubank primeiro                     |
| Front-end           | Vue 3 (PWA) no front, NestJS no back, monorepo TypeScript                              |
| IA                  | Custo mínimo: Haiku 5.5, teto mensal, app funciona sem chat                            |

Próximo passo sugerido: criar o repositório com a estrutura da fase 0, este documento em `docs/` e um `CLAUDE.md` inicial.
