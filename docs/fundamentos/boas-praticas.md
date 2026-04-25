📌 Diretrizes de Código e Arquitetura

Este projeto segue princípios de Clean Code, separação de responsabilidades e boas práticas de desenvolvimento em .NET.

🎯 Objetivo

Garantir que o código seja:

legível
escalável
fácil de manter
consistente entre todas as camadas do sistema
🧱 Princípios obrigatórios
1. Separação de responsabilidades
Controllers não devem conter regra de negócio
Services concentram a lógica
Repositories lidam com acesso a dados
2. Código limpo (Clean Code)
Nomes claros e descritivos
Métodos curtos
Evitar comentários desnecessários (código deve ser autoexplicativo)
3. Boas práticas com Entity Framework
Não acessar DbContext diretamente em controllers
Usar Services para orquestração
Utilizar migrations para versionamento do banco
4. Padronização de API
Sempre usar DTOs (Request/Response)
Nunca expor entidades diretamente
Retornar respostas consistentes
DTOs partilhados com clientes no projeto TeacherApp.Contracts (ver ../arquitetura/contracts.md)
5. Evolução do projeto

Caso uma implementação fuja desses padrões:

avaliar impacto na manutenção
comparar com abordagem recomendada
sugerir melhoria antes de implementar
⚠️ Regra importante

Se houver dúvida entre:

“funcionar rápido”
ou “seguir padrão correto”

➡️ priorizar sempre a estrutura correta, pensando no crescimento do projeto.

🚀 Observação final

Este documento pode evoluir conforme o projeto cresce.