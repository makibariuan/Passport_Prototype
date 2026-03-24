<template>
  <teleport to="body">
    <transition name="fade">
      <div v-if="show" class="dialog-overlay">
        <div class="dialog-box vms-ultra-wide" role="dialog" :aria-labelledby="titleId" ref="dialogRef" tabindex="-1">
          <header class="dialog-header">
            <h3 :id="titleId">{{ title }}</h3>
          </header>

          <div class="dialog-body">
            <slot />
          </div>

          <footer v-if="$slots.footer" class="dialog-footer">
            <slot name="footer" />
          </footer>
        </div>
      </div>
    </transition>
  </teleport>
</template>

<script setup>
  import { ref, watch, onMounted, onBeforeUnmount } from 'vue';

  const props = defineProps({
    show: { type: Boolean, required: true },
    title: { type: String, default: '' },
  });

  const emit = defineEmits(['update:show', 'close']);

  const titleId = 'dialog-title-' + Math.random().toString(36).slice(2, 9);
  const dialogRef = ref(null);

  const close = () => {
    emit('update:show', false);
    emit('close');
  };

  watch(() => props.show, (open) => {
    document.body.style.overflow = open ? 'hidden' : '';
    if (open) {
      // Focus the dialog for accessibility
      setTimeout(() => dialogRef.value?.focus(), 50);
    }
  });

  const onKey = (e) => {
    if (e.key === 'Escape' && props.show) close();
  };

  onMounted(() => window.addEventListener('keydown', onKey));
  onBeforeUnmount(() => {
    window.removeEventListener('keydown', onKey);
    document.body.style.overflow = ''; // Ensure scroll returns if component unmounts
  });
</script>

<style scoped>
  .dialog-overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.7); /* Darker Slate backdrop */
    backdrop-filter: blur(4px); /* Modern blur effect */
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10000;
    padding: 20px;
  }

  .dialog-box.vms-ultra-wide {
    background: #ffffff;
    /* Matching VMS Registration Container Width */
    width: 100%;
    max-width: 1400px;
    height: 90vh;
    border-radius: 20px;
    display: flex;
    flex-direction: column;
    box-shadow: 0 30px 60px rgba(0, 0, 0, 0.4);
    overflow: hidden;
    animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  } 

  .dialog-header {
    display: flex;
    align-items: center;
    justify-content: center; /* Center the title since X is gone */
    padding: 24px;
    border-bottom: 1px solid #f1f5f9;
    background: #f8fafc;
  }

    .dialog-header h3 {
      margin: 0;
      font-size: 1.4rem; /* Slightly larger title */
      font-weight: 800;
      color: #002a57;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

  .dialog-close {
    background: #e2e8f0;
    border: none;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
    color: #64748b;
  }

    .dialog-close:hover {
      background: #cbd5e1;
      color: #0f172a;
    }

  .dialog-body {
    padding: 24px;
    flex-grow: 1;
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: #cbd5e1 transparent;
    text-align: justify;
    text-justify: inter-word; /* Improves spacing between words */
  }

  .dialog-footer {
    padding: 16px 24px;
    border-top: 1px solid #f1f5f9;
    background: #ffffff;
    display: flex;
    justify-content: flex-end;
  }

  /* Animations */
  @keyframes slideUp {
    from {
      opacity: 0;
      transform: translateY(20px);
    }

    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.2s;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }
</style>
