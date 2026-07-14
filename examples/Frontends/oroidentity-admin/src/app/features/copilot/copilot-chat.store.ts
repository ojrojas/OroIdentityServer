import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signalStore, withState, withMethods, withComputed } from '@ngrx/signals';
import { computed } from '@angular/core';
import { ChatMessage } from '../../core/models';
import { SignalRService } from '../../core/services/signalr.service';

interface CopilotChatState {
  messages: ChatMessage[];
  isTyping: boolean;
  conversationId: string | null;
  isOpen: boolean;
}

export const CopilotChatStore = signalStore(
  { providedIn: 'root' },
  withState<CopilotChatState>({ messages: [], isTyping: false, conversationId: null, isOpen: false }),
  withComputed(store => ({
    hasMessages: computed(() => store.messages().length > 0),
  })),
  withMethods(store => {
    const http = inject(HttpClient);
    const signalR = inject(SignalRService);
    return {
      openPanel(): void { (store as any)._isOpen.set(true); },
      closePanel(): void { (store as any)._isOpen.set(false); },
      togglePanel(): void { (store as any)._isOpen.update((v: boolean) => !v); },

      sendMessage(content: string, companyId: string): void {
        const userMsg: ChatMessage = {
          id: crypto.randomUUID(),
          role: 'user',
          content,
          timestamp: new Date().toISOString(),
        };
        (store as any)._messages.update((msgs: ChatMessage[]) => [...msgs, userMsg]);
        (store as any)._isTyping.set(true);

        http.post<{ conversationId: string }>('/api/copilot/conversations/start', { companyId, userMessage: content })
          .subscribe(res => {
            (store as any)._conversationId.set(res.conversationId);
          });
      },

      appendStreamChunk(chunk: string): void {
        const msgs: ChatMessage[] = (store as any).messages();
        const last = msgs[msgs.length - 1];
        if (last?.isStreaming) {
          (store as any)._messages.update((m: ChatMessage[]) =>
            m.map((msg, i) => i === m.length - 1 ? { ...msg, content: msg.content + chunk } : msg)
          );
        } else {
          const assistantMsg: ChatMessage = {
            id: crypto.randomUUID(),
            role: 'assistant',
            content: chunk,
            timestamp: new Date().toISOString(),
            isStreaming: true,
          };
          (store as any)._messages.update((m: ChatMessage[]) => [...m, assistantMsg]);
        }
      },

      completeStream(): void {
        (store as any)._isTyping.set(false);
        (store as any)._messages.update((m: ChatMessage[]) =>
          m.map(msg => ({ ...msg, isStreaming: false }))
        );
      },
    };
  })
);
