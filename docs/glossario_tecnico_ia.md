# Glossário Técnico de Engenharia de IA

Este glossário define os termos técnicos essenciais para o trabalho diário com Large Language Models (LLMs) e integrações de IA corporativas.

* **Tokens:** São as unidades básicas de processamento de um LLM. Um token pode representar uma palavra inteira, uma sílaba ou apenas um caractere, dependendo do idioma e do modelo. Os limites de custo e processamento das APIs são baseados na quantidade de tokens consumidos.
* **Embeddings:** São vetores matemáticos que representam o significado semântico de textos, imagens ou dados estruturados. Na prática do desenvolvimento C#, ferramentas como o Semantic Kernel serão utilizadas para transformar objetos e textos em embeddings, possibilitando a realização de buscas semânticas eficientes.
* **Temperatura (Temperature):** Parâmetro de configuração da API do LLM que controla a aleatoriedade e a "criatividade" da saída gerada. Valores próximos a 0.0 tornam o modelo mais determinístico e focado (ideal para código ou dados exatos), enquanto valores mais altos (ex: 0.7 ou 1.0) geram respostas mais variadas e criativas.
* **Janela de Contexto (Context Window):** É o limite máximo de tokens (somando o prompt de entrada e a resposta gerada) que o modelo consegue processar, analisar e "lembrar" em uma única interação. Informações que ultrapassam essa janela são esquecidas ou ignoradas pelo modelo durante a inferência.


