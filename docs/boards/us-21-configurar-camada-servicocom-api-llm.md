**2.1: Configurar a camada de serviço no .NET Core para integrar com a API de LLM**
  •	 **User Story:** 
      **Como** Desenvolvedor Backend, 
      **quero** integrar o ecossistema .NET com a API de uma LLM, 
      **para** que o sistema possa enviar prompts e receber respostas processadas. 
    •	**Critérios de Aceitação (AC):**
      o	A interface ILlmIntegrationService e sua classe concreta devem estar implementadas no projeto .NET. 
      o	As chaves de API não devem estar hardcoded; devem utilizar o Secret Manager no ambiente de desenvolvimento e Environment Variables para produção.
      o	Um endpoint de validação (ex: POST /api/ai/test) deve retornar status 200 OK com uma resposta em texto da IA.
----------------------
•	**Tasks:**
  o	**Task 1: **
      - Criação de Interface e Serviço: Criar a estrutura base de injeção de dependência para isolar a regra de negócio da comunicação externa.
  o	**Task 2:** 
      - Configuração de HTTP Client e Segurança: Configurar o HttpClient na classe Startup.cs/Program.cs e mapear as variáveis de ambiente para a API Key.
  o	**Task 3:** 
      - Construção do Endpoint de Teste: Criar um Controller simples no padrão RESTful para validar o fluxo de comunicação ponta a ponta. 

