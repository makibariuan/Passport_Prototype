<template>
  <teleport to="body">
    <div v-if="show" class="dialog-overlay" @click.self="$emit('close')">
      <div class="dialog-box auth-box-style" :class="{ 'preview-mode': isHtml }">
        <h3 class="dialog-title-style">{{ title }}</h3>

        <div v-if="message" class="dialog-message-container">
          <div v-if="isHtml" v-html="message"></div>

          <div v-else>
            <div class="dialog-message-line"
                 v-for="(line, index) in message.split('\n')"
                 :key="index">
              {{ line }}
            </div>
          </div>
        </div>

        <slot></slot>

        <button v-if="!$slots.default" @click="$emit('close')" class="dialog-btn auth-btn-style">
          Close
        </button>
      </div>
    </div>
  </teleport>
</template>

<script setup>
  import { computed } from 'vue';

  const props = defineProps({
    show: Boolean,
    title: String,
    message: String
  });

  defineEmits(['close']);

  // Check if message starts with HTML to decide rendering mode
  const isHtml = computed(() => {
    return props.message && (props.message.trim().startsWith('<') || props.message.includes('<iframe'));
  });
</script>

<style scoped>
  /* --- Overlay --- */
  .dialog-overlay {
    position: fixed;
    inset: 0;
    background: rgba(19, 34, 56, 0.85); /* Darker blue-tinted overlay */
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    backdrop-filter: blur(4px);
  }

  /* --- Main Box --- */
  .auth-box-style {
    background: #ffffff;
    border-radius: 12px; /* Smoother radius to match PDS tables */
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    border: 1px solid #d1d5db;
    padding: 30px;
    width: 90%;
    max-width: 500px;
    transition: all 0.3s ease;
  }

  /* Expand the box if it's a file preview */
  .preview-mode {
    max-width: 900px !important;
    width: 95% !important;
  }

  /* --- Title --- */
  .dialog-title-style {
    font-size: 1.8rem;
    font-weight: 700;
    margin-bottom: 20px;
    color: #002a57;
    text-align: left;
    border-bottom: 2px solid #e5e7eb;
    padding-bottom: 15px;
  }

  /* --- Container --- */
  .dialog-message-container {
    margin: 10px 0;
    text-align: left;
  }

  /* Style for plain text messages */
  .dialog-message-line {
    color: #4b5563;
    font-size: 1.05rem;
    line-height: 1.6;
    margin-bottom: 8px;
  }

  /* --- Button --- */
  .auth-btn-style {
    margin-top: 25px;
    padding: 12px 24px;
    border: none;
    border-radius: 6px;
    font-size: 1rem;
    font-weight: 600;
    color: white;
    background: #002a57; /* Solid PDS Primary Blue */
    cursor: pointer;
    transition: background 0.2s;
    display: block;
    margin-left: auto; /* Right aligned */
  }

    .auth-btn-style:hover {
      background: #004085;
    }

  /* Support for HTML/Iframe height */
  :deep(iframe) {
    border-radius: 8px;
    border: 1px solid #d1d5db;
  }
</style>
