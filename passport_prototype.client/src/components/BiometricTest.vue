<template>
  <div class="test-container">
    <div class="test-card">
      <h2>Biometric Asset Downloader</h2>
      <p class="subtitle">Enter an application code to retrieve the ZIP archive (Photo & Signature).</p>

      <div class="input-group">
        <input v-model="applicationCode"
               type="text"
               placeholder="e.g. APP-2024-001"
               @keyup.enter="handleDownload" />
        <button :disabled="loading" @click="handleDownload">
          <span v-if="loading">Downloading...</span>
          <span v-else>Download ZIP</span>
        </button>
      </div>

      <div v-if="message" :class="['message', isError ? 'error' : 'success']">
        {{ message }}
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref } from 'vue';
  import axios from 'axios';

  const applicationCode = ref('');
  const loading = ref(false);
  const message = ref('');
  const isError = ref(false);

  const handleDownload = async () => {
    if (!applicationCode.value) {
      alert("Please enter an application code.");
      return;
    }

    loading.value = true;
    isError.value = false;
    message.value = "Generating ZIP...";

    try {
      const url = `/api/Kit/download-picture-and-signature/${applicationCode.value}`;

      const response = await axios.get(url, {
        responseType: 'blob',
      });

      // 1. Double check if the Blob is actually JSON (an error) instead of a ZIP
      if (response.data.type === 'application/json') {
        const text = await response.data.text();
        const errorData = JSON.parse(text);
        throw new Error(errorData.message || errorData.error || "File not found.");
      }

      const blob = new Blob([response.data], { type: 'application/zip' });
      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.setAttribute('download', `${applicationCode.value}_Assets.zip`);
      document.body.appendChild(link);
      link.click();

      link.remove();
      window.URL.revokeObjectURL(downloadUrl);
      message.value = "Download successful!";

    } catch (err) {
      console.error("Export Error:", err);
      isError.value = true;

      // 2. THIS IS THE ADDITION: Handle error blobs from the server
      if (err.response && err.response.data instanceof Blob) {
        const reader = new FileReader();
        reader.onload = () => {
          try {
            const errorText = JSON.parse(reader.result);
            console.error("Detailed Backend Error:", errorText);
            message.value = errorText.error || errorText.message || "Server Crash (500)";
          } catch (e) {
            message.value = "Server returned an unreadable error.";
          }
        };
        reader.readAsText(err.response.data);
      } else {
        // Fallback for network errors or thrown logic errors
        message.value = err.message || "Failed to connect to backend.";
      }
    } finally {
      loading.value = false;
    }
  };
</script>

<style scoped>
  .test-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 50vh;
    font-family: 'Montserrat', sans-serif;
    /* Adding a slight background to the whole page helps the white card pop */
    background-color: #f4f7f9;
  }

  .test-card {
    background: white;
    padding: 2.5rem;
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0,0,0,0.15);
    width: 100%;
    max-width: 500px;
  }

  h2 {
    color: #1a1a1a; /* Near black */
    font-weight: 800;
    margin-bottom: 0.5rem;
  }

  .subtitle {
    color: #333333; /* Dark gray */
    font-size: 0.95rem;
    line-height: 1.4;
    margin-bottom: 1rem;
  }

  .input-group {
    display: flex;
    gap: 12px;
    margin-top: 1.5rem;
  }

  input {
    flex: 1;
    padding: 12px;
    border: 2px solid #cbd5e0; /* Slightly thicker border */
    border-radius: 6px;
    outline: none;
    color: #000000; /* Actual typed text is pure black */
    font-weight: 600;
    font-size: 1rem;
  }

    /* Make the placeholder text darker too */
    input::placeholder {
      color: #718096;
    }

    input:focus {
      border-color: #004a99;
    }

  button {
    padding: 12px 24px;
    background: #004a99;
    color: #ffffff;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 700;
    transition: background 0.2s;
  }

    button:hover:not(:disabled) {
      background: #003366;
    }

    button:disabled {
      background: #a0aec0;
      cursor: not-allowed;
    }

  .message {
    margin-top: 1.5rem;
    font-size: 1rem;
    font-weight: 600;
    padding: 10px;
    border-radius: 6px;
  }

  .error {
    color: #721c24;
    background-color: #f8d7da;
    border: 1px solid #f5c6cb;
  }

  .success {
    color: #155724;
    background-color: #d4edda;
    border: 1px solid #c3e6cb;
  }
</style>
