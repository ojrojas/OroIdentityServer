import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MarkdownComponent } from 'ngx-markdown';
import { CopilotChatStore } from './copilot-chat.store';
import { CompanyStore } from '../company/company.store';

@Component({
  selector: 'app-copilot-chat-panel',
  standalone: true,
  imports: [FormsModule, MarkdownComponent],
  template: `
    <!-- Floating button -->
    @if (!store.isOpen()) {
      <button class="chat-fab oro-btn" (click)="store.openPanel()" aria-label="Abrir Copilot">
        🤖
      </button>
    }

    <!-- Panel -->
    @if (store.isOpen()) {
      <div class="chat-panel">
        <div class="chat-header">
          <div class="flex items-center gap-2">
            <span>🤖</span>
            <span class="font-semibold">Copilot Contable</span>
          </div>
          <button class="oro-btn oro-btn-ghost" (click)="store.closePanel()" aria-label="Cerrar">✕</button>
        </div>

        <div class="chat-messages" #scrollContainer>
          @if (!store.hasMessages()) {
            <div class="chat-welcome">
              <span style="font-size:2rem;">👋</span>
              <p class="font-medium">¡Hola! Soy tu Copilot contable.</p>
              <p class="text-muted text-sm">Puedo ayudarte con asientos, reportes, conciliaciones y más.</p>
            </div>
          }
          @for (msg of store.messages(); track msg.id) {
            <div class="chat-message" [class.user]="msg.role==='user'" [class.assistant]="msg.role==='assistant'">
              @if (msg.role === 'assistant') {
                <markdown [data]="msg.content" class="markdown-content" />
                @if (msg.isStreaming) { <span class="streaming-cursor">▊</span> }
              } @else {
                <span>{{ msg.content }}</span>
              }
            </div>
          }
          @if (store.isTyping()) {
            <div class="typing-indicator">
              <span></span><span></span><span></span>
            </div>
          }
        </div>

        <div class="chat-input-area">
          <textarea class="oro-input chat-textarea" rows="2"
                    placeholder="Escribe un mensaje... (Enter para enviar, Shift+Enter para nueva línea)"
                    [(ngModel)]="message"
                    (keydown.enter)="onEnter($event)"></textarea>
          <button class="oro-btn oro-btn-primary" (click)="send()" [disabled]="!message.trim()">
            ➤
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .chat-fab {
      position: fixed; bottom: 1.5rem; right: 1.5rem; z-index: 1000;
      width: 56px; height: 56px; border-radius: 50%;
      background: var(--oro-primary); color: white;
      font-size: 1.5rem; display: flex; align-items: center; justify-content: center;
      box-shadow: var(--oro-shadow-lg); transition: transform var(--oro-transition);
    }
    .chat-fab:hover { transform: scale(1.1); background: var(--oro-primary-light); }
    .chat-panel {
      position: fixed; bottom: 1rem; right: 1rem; z-index: 999;
      width: 400px; height: 560px; max-height: calc(100vh - 80px);
      background: var(--oro-surface); border: 1px solid var(--oro-border);
      border-radius: var(--oro-radius-xl); box-shadow: var(--oro-shadow-lg);
      display: flex; flex-direction: column; overflow: hidden;
    }
    .chat-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: .75rem 1rem; border-bottom: 1px solid var(--oro-border);
      background: var(--oro-primary); color: white; border-radius: var(--oro-radius-xl) var(--oro-radius-xl) 0 0;
    }
    .chat-header .oro-btn { color: white; }
    .chat-messages { flex: 1; overflow-y: auto; padding: 1rem; display: flex; flex-direction: column; gap: .75rem; }
    .chat-welcome { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; gap: .5rem; text-align: center; }
    .chat-message { max-width: 85%; padding: .5rem .75rem; border-radius: var(--oro-radius-lg); font-size: var(--oro-text-sm); line-height: 1.5; }
    .chat-message.user { align-self: flex-end; background: var(--oro-primary); color: white; border-radius: var(--oro-radius-lg) var(--oro-radius-lg) var(--oro-radius-sm) var(--oro-radius-lg); }
    .chat-message.assistant { align-self: flex-start; background: var(--oro-bg); border: 1px solid var(--oro-border); border-radius: var(--oro-radius-lg) var(--oro-radius-lg) var(--oro-radius-lg) var(--oro-radius-sm); }
    .markdown-content :global(table) { border-collapse: collapse; width: 100%; font-size: .75rem; }
    .markdown-content :global(td), .markdown-content :global(th) { border: 1px solid var(--oro-border); padding: .2rem .4rem; }
    .streaming-cursor { animation: blink .7s infinite; }
    @keyframes blink { 50% { opacity: 0; } }
    .typing-indicator { display: flex; align-items: center; gap: .3rem; padding: .5rem .75rem; background: var(--oro-bg); border-radius: var(--oro-radius-lg); width: fit-content; }
    .typing-indicator span { width: 8px; height: 8px; border-radius: 50%; background: var(--oro-text-muted); animation: bounce 1.2s infinite; }
    .typing-indicator span:nth-child(2) { animation-delay: .2s; }
    .typing-indicator span:nth-child(3) { animation-delay: .4s; }
    @keyframes bounce { 0%,80%,100% { transform: scale(0); } 40% { transform: scale(1); } }
    .chat-input-area { display: flex; gap: .5rem; padding: .75rem; border-top: 1px solid var(--oro-border); align-items: flex-end; }
    .chat-textarea { flex: 1; resize: none; min-height: 40px; max-height: 100px; }
    @media (max-width: 480px) { .chat-panel { width: calc(100vw - 1rem); right: .5rem; } }
  `]
})
export class CopilotChatPanelComponent {
  readonly store = inject(CopilotChatStore);
  private readonly companyStore = inject(CompanyStore);
  message = '';

  onEnter(e: Event): void {
    const ke = e as KeyboardEvent;
    if (!ke.shiftKey) { ke.preventDefault(); this.send(); }
  }

  send(): void {
    const text = this.message.trim();
    if (!text) return;
    this.message = '';
    this.store.sendMessage(text, this.companyStore.activeCompany()?.id ?? '');
  }
}
