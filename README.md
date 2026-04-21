# FIX Trading Order Processor

Sistema de processamento de ordens financeiras utilizando o protocolo FIX 4.4, implementado com arquitetura limpa (Clean Architecture) e padrões de design modernos em .NET 8.

## Descrição

Este projeto simula um ambiente de trading eletrônico onde ordens de compra e venda são enviadas através do protocolo FIX (Financial Information eXchange). A aplicação é composta por uma API REST que gera ordens e um Worker Service que as processa e acumula, calculando a exposição financeira por ativo em tempo real.

## Tecnologias Utilizadas

- **Linguagem:** C# 12
- **Framework:** .NET 8
- **Protocolo:** FIX 4.4 (Financial Information eXchange)
- **Bibliotecas:**
  - QuickFIXn - Implementação do protocolo FIX
  - FluentValidation - Validação de requests
  - Swashbuckle - Documentação Swagger/OpenAPI
- **Arquitetura:** Clean Architecture (Domain, Application, Infrastructure)
- **Padrões:** DDD, Repository Pattern, Dependency Injection


## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022+ ou VS Code

## Instalação e Execução

1. **Clone o repositório:**

```bash
git clone https://github.com/aurilio/fix-trading-order-processor.git
cd fix-trading-order-processor
```

2. **Restaure as dependências:**
```bash
dotnet restore
```

### Opção 1: Visual Studio

3. **Abra a solution** `fix-trading-order-processor.sln` no Visual Studio

4. **Configure múltiplos projetos de inicialização:**
   - Clique com o botão direito na Solution __Set Startup Projects__
   - Selecione __Multiple startup projects__
   - Defina **Action = Start** para:
     - `FixTrading.OrderAccumulator.Worker`
     - `FixTrading.OrderGenerator.Api`
   - Clique em **OK**

5. **Pressione `F5`** ou clique em **Start** para executar ambos os projetos

### Opção 2: Linha de Comando

3. **Execute o Worker (Acceptor) primeiro:**
```bash
cd src/FixTrading.OrderAccumulator.Worker dotnet run
```

4. **Em outro terminal, execute a API (Initiator):**
```bash
cd src/FixTrading.OrderGenerator.Api dotnet run
```
