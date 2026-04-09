# Sprint 3 – AI ChatBot Feature

## Overview

An AI-powered ChatBot was integrated into the Inventory Management System to provide users with an intelligent assistant capable of answering questions about their inventory data and system usage.

---

## Architecture

```
┌──────────────┐     ┌─────────────────────┐     ┌──────────────┐
│   Browser     │────▶│  ChatBotController  │────▶│  ChatBot     │
│  (Chat UI)    │◀────│  (API)              │◀────│  Service     │
└──────────────┘     └─────────────────────┘     └──────┬───────┘
                                                         │
                                                         ▼
                                                  ┌──────────────┐
                                                  │ ChatResponse  │
                                                  │ Orchestrator  │
                                                  └──────┬───────┘
                                                         │
                                                         ▼
                                                  ┌──────────────┐
                                                  │   OpenAI API  │
                                                  └──────────────┘
```

## Components

### Service Layer

| Component | Responsibility |
|---|---|
| `IChatBotService` / `ChatBotService` | Core chat logic – manages conversation state and message processing |
| `IChatResponseOrchestrator` / `ChatResponseOrchestrator` | Orchestrates the flow between user input, context retrieval, and AI response generation |
| `OpenAIOptions` | Configuration class binding OpenAI API key and model settings from `appsettings.json` |

### API

- `ChatBotController` – Exposes REST endpoints for sending messages and retrieving conversation history.

### Data Models

| Model | Purpose |
|---|---|
| `ChatLog` | Persists individual chat messages (user and assistant) with timestamps |
| `ChatModels` | Request/response DTOs for the chat API |

### Database Changes

Three migrations were created to support the chat feature:

1. **`20260329192122_AddChatLog`** – Creates the `ChatLog` table for message persistence.
2. **`20260329195402_AddConversationTitle`** – Adds a conversation title field for organizing chat sessions.
3. **`20260329215851_AddConversationIsPinned`** – Adds a pinning flag to allow users to bookmark important conversations.

---

## Configuration

The ChatBot requires an OpenAI API key configured via environment variables or `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "<your-openai-api-key>",
    "Model": "gpt-4"
  }
}
```

---

## Testing

- `ChatBotServiceTests` – Unit tests for message processing and conversation state management.
- `ChatResponseOrchestratorTests` – Unit tests for the orchestration flow.
- `Api/ChatBotControllerTests` – API endpoint tests for the chat controller.
