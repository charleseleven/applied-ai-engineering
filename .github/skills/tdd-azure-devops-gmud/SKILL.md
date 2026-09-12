---
name: tdd-azure-devops-gmud
description: 'Guia de Test-Driven Development (TDD) em C# para o projeto GmudAutomation (src/backend-api/GmudAutomation), com integração mockada às APIs do Azure DevOps (Boards e Repos). Use quando o usuário pedir para criar testes unitários, aplicar TDD, mockar Azure Boards/Repos, escrever testes com xUnit/Moq/FluentAssertions, ou implementar as Features 1-4 do GmudAutomation (extração de escopo/hierarquia, orquestração de branches e merge, Pull Requests, e anexos/artefatos).'
---

# TDD para Azure DevOps (Projeto GmudAutomation)

## Quando usar
- Criar ou revisar testes unitários em C# para serviços que integram com Azure Boards ou Azure Repos.
- Implementar qualquer uma das 4 features do GmudAutomation (parser de User Stories, orquestração de branches/merge, Pull Requests, manipulação de anexos).
- Garantir que testes não realizem chamadas HTTP reais durante CI/CD.

## Diretrizes gerais

### Stack de testes
- **xUnit** para execução dos testes.
- **Moq** para simular as APIs do Azure DevOps (Boards e Repos).
- **FluentAssertions** para validações semânticas e legíveis (`result.Should().Be(...)`, `action.Should().Throw<T>()`, etc.).

### Isolamento (Mocking)
- Toda integração externa deve passar por uma interface dedicada (ex: `IAzureDevOpsClient`, `IGitIntegration`).
- Os testes de unidade mockam essas interfaces — nunca fazem chamadas HTTP reais que possam poluir ambientes de produção ou pré-produção.
- Se a interface ainda não existir para o código sendo testado, crie-a antes de escrever o teste (isolar a regra de negócio da comunicação externa).

### Nomenclatura
- Use `Given_When_Then` ou `Metodo_Cenario_ResultadoEsperado` para nomear métodos de teste.
- Mantenha um padrão único por projeto/feature — não misture as duas convenções na mesma classe de teste.

## Procedimento por feature

### Feature 1: Extração de Escopo e Hierarquia
Testar o parser de User Stories que extrai links de tasks filhas a partir da descrição HTML retornada pelo Azure Boards.
1. Mockar um payload de retorno da API do Azure Boards com uma descrição HTML contendo três links de tasks filhas.
2. Validar que o serviço de extração retorna exatamente as três URLs corretas.
3. Criar um teste de *edge case* para descrição sem links, garantindo que nenhuma `NullReferenceException` é lançada.

### Feature 2: Orquestração e Conflitos de Merge
Testar a classe responsável pela orquestração de branches no Azure Repos, mockando a interface de integração com o Git.
1. Validar a formatação correta da string da nova branch (ex: `release/GMUD_PROD_Cliente_Data`).
2. Simular a API de merge retornando status de conflito (HTTP 409) e afirmar que o sistema lança uma `MergeConflictException` contendo o log dos arquivos conflitantes.

### Feature 3: Pull Requests
Testar o módulo de criação de Pull Requests, mockando a criação do PR de Database.
1. Validar que a rotina retorna o link do PR sem invocar o método de aprovação (`complete`).

### Feature 4: Artefatos/Anexos
Testar a manipulação de arquivos anexados às tasks.
1. Mockar a manipulação de arquivos.
2. Validar que um anexo genérico é renomeado estritamente para o padrão `<numero_us>_SCRIPT_<numero_task>.txt` antes do upload.

## Checklist antes de finalizar
- [ ] Todas as interfaces externas (`IAzureDevOpsClient`, integração Git, upload de arquivos) estão mockadas via Moq.
- [ ] Nenhum teste depende de rede, disco real ou credenciais.
- [ ] Nomes dos testes seguem `Given_When_Then` ou `Metodo_Cenario_ResultadoEsperado`.
- [ ] Asserções usam FluentAssertions (`.Should()...`) em vez de `Assert.*` do xUnit puro.
- [ ] Edge cases (payload vazio, nulo, conflito HTTP) têm teste dedicado.
