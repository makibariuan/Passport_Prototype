<template>
  <DialogBox :show="show" title="Terms of Service" @update:show="updateShow">
    <div v-if="loading" class="terms-loading">
      <i class="fas fa-circle-notch fa-spin"></i>
      <p>Loading document...</p>
    </div>

    <div v-else class="terms-content-container">
      <div v-if="!hasTitle" class="terms-header">
        <img src="@/assets/makati-logo.png" alt="Makati Logo" class="tos-mini-logo" />
        <h2>Makati Onboarding System</h2>
        <div class="divider"></div>
        <p class="subtitle">Last updated: October 2025</p>
      </div>

      <div class="terms-body" v-html="marked(termsContent)"></div>
    </div>

    <template #footer>
      <div class="terms-actions">
        <button class="btn btn-outline" @click="close">Decline</button>
        <button class="btn btn-primary" @click="acceptTerms">Accept & Continue</button>
      </div>
    </template>
  </DialogBox>
</template>

<script setup>
  import { ref, watch } from 'vue'
  import { marked } from 'marked'
  import DialogBox from './DialogBoxToS.vue'

  const props = defineProps({
    show: { type: Boolean, required: true },
    file: { type: String, default: '/terms-of-service.md' },
  })
  const emits = defineEmits(['update:show', 'accepted'])

  const termsContent = ref('')
  const loading = ref(true)
  const hasTitle = ref(false)

  const loadTerms = async () => {
    loading.value = true
    try {
      const r = await fetch(props.file)
      const text = await r.text()
      termsContent.value = text
      hasTitle.value = /^#\s+/m.test(text) || /^##\s+/m.test(text)
    } catch {
      termsContent.value = '### ⚠️ Error\nUnable to load the Terms of Service. Please contact system administration.'
    } finally {
      loading.value = false
    }
  }

  watch(() => props.show, (open) => {
    if (open && !termsContent.value) loadTerms()
  })

  const updateShow = (val) => emits('update:show', val)
  const close = () => emits('update:show', false)
  const acceptTerms = () => {
    emits('accepted')
    emits('update:show', false)
  }
</script>

<style scoped>
  /* --- CONTAINER --- */
  .terms-content-container {
    padding: 10px 5px;
    color: #1e293b;
  }

  /* --- HEADER --- */
  .terms-header {
    text-align: center;
    margin-bottom: 2rem;
  }

  .tos-mini-logo {
    width: 60px;
    margin-bottom: 1rem;
  }

  .terms-header h2 {
    font-size: 1.25rem;
    font-weight: 800;
    color: #002a57;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    margin: 0;
  }

  .divider {
    height: 2px;
    width: 40px;
    background: #b79147; /* Makati Gold */
    margin: 12px auto;
  }

  .subtitle {
    font-size: 0.85rem;
    color: #64748b;
    font-weight: 500;
  }

  /* --- MARKDOWN BODY TYPOGRAPHY --- */
  .terms-body {
    line-height: 1.7;
    font-size: 0.95rem;
    color: #334155;
  }

  /* Deep Selectors for Markdown elements */
  :deep(.terms-body h3) {
    color: #002a57;
    font-size: 1.1rem;
    margin-top: 1.5rem;
    margin-bottom: 0.5rem;
    border-bottom: 1px solid #f1f5f9;
    padding-bottom: 4px;
  }

  :deep(.terms-body p) {
    margin-bottom: 1rem;
  }

  :deep(.terms-body ul), :deep(.terms-body ol) {
    margin-bottom: 1rem;
    padding-left: 1.5rem;
  }

  :deep(.terms-body li) {
    margin-bottom: 0.5rem;
  }

  :deep(.terms-body strong) {
    color: #0f172a;
  }

  /* --- ACTIONS & BUTTONS --- */
  .terms-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    width: 100%;
  }

  .btn {
    padding: 10px 24px;
    border-radius: 8px;
    font-size: 0.9rem;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 1.5px solid transparent;
  }

  .btn-primary {
    background-color: #002a57;
    color: #ffffff;
  }

    .btn-primary:hover {
      background-color: #004085;
      box-shadow: 0 4px 12px rgba(0, 42, 87, 0.2);
    }

  .btn-outline {
    background-color: transparent;
    border-color: #e2e8f0;
    color: #64748b;
  }

    .btn-outline:hover {
      background-color: #f8fafc;
      border-color: #cbd5e1;
      color: #1e293b;
    }

  /* --- LOADING STATE --- */
  .terms-loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
    color: #002a57;
  }

    .terms-loading i {
      font-size: 2rem;
      margin-bottom: 1rem;
    }
</style>
