<template>
  <div class="captcha-container">
    <p class="password-title">Security Verification:</p>

    <div class="captcha-controls">
      <canvas ref="captchaCanvas" width="120" height="32" class="captcha-canvas"></canvas>

      <input v-model="userInput"
             placeholder="Enter code"
             class="captcha-input"
             @keyup.enter="checkCaptcha" />

      <button type="button" @click="checkCaptcha" class="captcha-btn">Verify</button>

      <button type="button" @click="generateCaptcha" class="refresh-btn" title="Refresh">
        <i class="fas fa-sync-alt"></i>
      </button>
    </div>

    <p v-if="message" class="captcha-status-msg" :class="{'success': isValid, 'error': !isValid}">
      {{ message }}
    </p>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'

  const emit = defineEmits(['verified'])

  const captchaCanvas = ref(null)
  const captchaText = ref('')
  const userInput = ref('')
  const message = ref('')
  const isValid = ref(false)

  const generateCaptcha = () => {
    const ctx = captchaCanvas.value.getContext('2d')
    ctx.clearRect(0, 0, 150, 32)

    // Background
    ctx.fillStyle = '#f0f0f0'
    ctx.fillRect(0, 0, 150, 50)

    // Random Captcha
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'
    captchaText.value = Array.from({ length: 5 }, () =>
      chars.charAt(Math.floor(Math.random() * chars.length))
    ).join('')

    // Draw text
    ctx.font = '28px Arial'
    ctx.fillStyle = '#333'
    ctx.setTransform(1, 0.1, 0.1, 1, 0, 0)
    ctx.fillText(captchaText.value, 15, 24)

    // Noise
    for (let i = 0; i < 4; i++) {
      ctx.strokeStyle = `rgba(0,0,0,${Math.random()})`
      ctx.beginPath()
      ctx.moveTo(Math.random() * 150, Math.random() * 50)
      ctx.lineTo(Math.random() * 150, Math.random() * 50)
      ctx.stroke()
    }

    isValid.value = false
    userInput.value = ''
    message.value = ''
    emit('verified', false)
  }

  const checkCaptcha = () => {
    if (userInput.value.toUpperCase() === captchaText.value.toUpperCase()) {
      message.value = '✅ Captcha verified!'
      isValid.value = true
      emit('verified', true)
    } else {
      message.value = '❌ Incorrect captcha. Try again.'
      isValid.value = false
      emit('verified', false)
      generateCaptcha()
    }
  }

  onMounted(generateCaptcha)
</script>

<style scoped>
  .captcha-container {
    margin-bottom: 20px; /* Matches your other form-field spacing */
  }

  .password-title {
    font-weight: bold;
    font-size: 0.85rem;
    margin-bottom: 8px; /* Matches label spacing */
    color: #333;
  }

  .captcha-controls {
    display: flex;
    align-items: center;
    gap: 10px; /* Equal spacing between elements */
  }

  .captcha-canvas {
    border: 1px solid #ccc;
    border-radius: 4px;
    height: 32px; /* Matches input height */
    background: #f0f0f0;
  }

  .captcha-input {
    width: 120px !important;
    height: 32px !important;
    border: 1px solid #ccc;
    border-radius: 8px !important; /* Matches your "perfect" radius */
    padding: 0 10px;
    font-size: 0.8rem;
    margin-bottom: 0 !important; /* Prevents breaking the flex line */
  }

  .captcha-btn {
    height: 32px;
    padding: 0 15px;
    background-color: #3b82f6; /* Matches DFA blue */
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 0.8rem;
    font-weight: bold;
    cursor: pointer;
  }

  .refresh-btn {
    background: none;
    border: none;
    color: #3b82f6;
    cursor: pointer;
    font-size: 1rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .captcha-status-msg {
    font-size: 0.75rem;
    margin-top: 5px;
  }

    .captcha-status-msg.success {
      color: #16a34a;
    }

    .captcha-status-msg.error {
      color: #dc2626;
    }
</style>
